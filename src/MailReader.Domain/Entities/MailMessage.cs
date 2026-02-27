using MailReader.Domain.Events;
using MailReader.Domain.Primitives;

namespace MailReader.Domain.Entities;

public sealed class MailMessage : AggregateRoot
{
    private MailMessage(
        Guid id,
        Guid mailboxId,
        uint imapUid,
        string messageId,
        string subject,
        string fromAddress,
        string fromName,
        string bodyText,
        string? bodyHtml,
        DateTime sentAt) : base(id)
    {
        MailboxId = mailboxId;
        ImapUid = imapUid;
        MessageId = messageId;
        Subject = subject;
        FromAddress = fromAddress;
        FromName = fromName;
        BodyText = bodyText;
        BodyHtml = bodyHtml;
        SentAt = sentAt;
        ReceivedAt = DateTime.UtcNow;
    }

    // EF Core
    private MailMessage() { }

    public Guid MailboxId { get; private set; }

    /// <summary>IMAP UID письма в ящике</summary>
    public uint ImapUid { get; private set; }

    /// <summary>Message-ID заголовок из письма (для дедупликации)</summary>
    public string MessageId { get; private set; } = default!;

    public string Subject { get; private set; } = default!;
    public string FromAddress { get; private set; } = default!;
    public string FromName { get; private set; } = default!;
    public string BodyText { get; private set; } = default!;
    public string? BodyHtml { get; private set; }
    public DateTime SentAt { get; private set; }
    public DateTime ReceivedAt { get; private set; }

    public static MailMessage Create(
        Guid mailboxId,
        uint imapUid,
        string messageId,
        string subject,
        string fromAddress,
        string fromName,
        string bodyText,
        string? bodyHtml,
        DateTime sentAt)
    {
        var message = new MailMessage(
            Guid.NewGuid(),
            mailboxId,
            imapUid,
            messageId,
            subject,
            fromAddress,
            fromName,
            bodyText,
            bodyHtml,
            sentAt);

        message.RaiseDomainEvent(new MailMessageReceivedDomainEvent(message.Id, mailboxId));

        return message;
    }
}
