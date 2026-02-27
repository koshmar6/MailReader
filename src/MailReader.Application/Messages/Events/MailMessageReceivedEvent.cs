namespace MailReader.Application.Messages.Events;

/// <summary>
/// Публикуется в Kafka после успешного сохранения письма в БД.
/// Downstream-сервисы подписываются на этот топик.
/// </summary>
public sealed record MailMessageReceivedEvent(
    Guid MessageId,
    Guid MailboxId,
    string MailboxName,
    string Subject,
    string FromAddress,
    string FromName,
    string BodyText,
    string? BodyHtml,
    DateTime SentAt,
    DateTime ReceivedAt);
