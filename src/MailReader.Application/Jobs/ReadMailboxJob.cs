using MailReader.Application.Abstractions;
using MailReader.Application.Messages.Events;
using MailReader.Domain.Entities;
using MailReader.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MailReader.Application.Jobs;

/// <summary>
/// Читает новые письма из одного ящика через IMAP,
/// сохраняет в БД и публикует событие в Kafka.
/// Каждый инстанс worker'а конкурентно разбирает джобы из очереди Hangfire.
/// </summary>
public sealed class ReadMailboxJob(
    IMailboxRepository mailboxRepository,
    IMailMessageRepository messageRepository,
    IImapService imapService,
    IMessageBus messageBus,
    IUnitOfWork unitOfWork,
    ILogger<ReadMailboxJob> logger)
{
    public async Task ExecuteAsync(Guid mailboxId, CancellationToken ct)
    {
        var mailbox = await mailboxRepository.GetByIdAsync(mailboxId, ct);

        if (mailbox is null)
        {
            logger.LogWarning("Mailbox {MailboxId} not found. Skipping.", mailboxId);
            return;
        }

        if (!mailbox.IsActive)
        {
            logger.LogInformation("Mailbox {MailboxId} is inactive. Skipping.", mailboxId);
            return;
        }

        logger.LogInformation("Starting IMAP fetch for mailbox {MailboxName} ({MailboxId}), lastUid={LastUid}.",
            mailbox.Name, mailboxId, mailbox.LastSeenUid);

        var connectionSettings = new ImapConnectionSettings(
            mailbox.Credentials.Host,
            mailbox.Credentials.Port,
            mailbox.Credentials.Username,
            mailbox.Credentials.Password,
            mailbox.Credentials.UseSsl);

        var fetchedMails = await imapService.FetchNewMessagesAsync(
            connectionSettings,
            mailbox.LastSeenUid,
            ct);

        if (fetchedMails.Count == 0)
        {
            logger.LogInformation("No new messages for mailbox {MailboxName}.", mailbox.Name);
            return;
        }

        logger.LogInformation("Fetched {Count} new messages for mailbox {MailboxName}.",
            fetchedMails.Count, mailbox.Name);

        var publishTasks = new List<Task>(fetchedMails.Count);

        foreach (var fetched in fetchedMails)
        {
            // Дедупликация по Message-ID — защита от повторной обработки
            var alreadyExists = await messageRepository.ExistsByMessageIdAsync(
                mailboxId, fetched.MessageId, ct);

            if (alreadyExists)
            {
                logger.LogDebug("Message {MessageId} already exists. Skipping.", fetched.MessageId);
                continue;
            }

            var message = MailMessage.Create(
                mailboxId,
                fetched.Uid,
                fetched.MessageId,
                fetched.Subject,
                fetched.FromAddress,
                fetched.FromName,
                fetched.BodyText,
                fetched.BodyHtml,
                fetched.SentAt);

            messageRepository.Add(message);
            mailbox.UpdateLastSeenUid(fetched.Uid);

            // Готовим публикацию в Kafka
            var kafkaEvent = new MailMessageReceivedEvent(
                message.Id,
                mailboxId,
                mailbox.Name,
                fetched.Subject,
                fetched.FromAddress,
                fetched.FromName,
                fetched.BodyText,
                fetched.BodyHtml,
                fetched.SentAt,
                DateTime.UtcNow);

            publishTasks.Add(messageBus.PublishAsync(kafkaEvent, ct));
        }

        // Сначала сохраняем в БД...
        await unitOfWork.SaveChangesAsync(ct);

        // ...потом публикуем в Kafka
        await Task.WhenAll(publishTasks);

        logger.LogInformation("Completed processing mailbox {MailboxName}. Processed {Count} messages.",
            mailbox.Name, fetchedMails.Count);
    }
}
