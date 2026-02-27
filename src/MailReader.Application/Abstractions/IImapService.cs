namespace MailReader.Application.Abstractions;

public interface IImapService
{
    Task<IReadOnlyList<FetchedMail>> FetchNewMessagesAsync(
        ImapConnectionSettings settings,
        uint lastSeenUid,
        CancellationToken ct = default);
}

public sealed record ImapConnectionSettings(
    string Host,
    int Port,
    string Username,
    string Password,
    bool UseSsl);

public sealed record FetchedMail(
    uint Uid,
    string MessageId,
    string Subject,
    string FromAddress,
    string FromName,
    string BodyText,
    string? BodyHtml,
    DateTime SentAt);
