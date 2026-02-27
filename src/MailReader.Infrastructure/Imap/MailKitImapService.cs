using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailReader.Application.Abstractions;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace MailReader.Infrastructure.Imap;

public sealed class MailKitImapService(ILogger<MailKitImapService> logger) : IImapService
{
    public async Task<IReadOnlyList<FetchedMail>> FetchNewMessagesAsync(
        ImapConnectionSettings settings,
        uint lastSeenUid,
        CancellationToken ct = default)
    {
        using var client = new ImapClient();

        try
        {
            await client.ConnectAsync(settings.Host, settings.Port, settings.UseSsl, ct);
            await client.AuthenticateAsync(settings.Username, settings.Password, ct);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly, ct);

            // Ищем все письма с UID > lastSeenUid
            var query = lastSeenUid == 0
                ? SearchQuery.All
                : SearchQuery.Uids(new UniqueIdRange(new UniqueId(lastSeenUid + 1), UniqueId.MaxValue));

            var uids = await inbox.SearchAsync(query, ct);

            if (uids.Count == 0)
                return [];

            logger.LogDebug("Found {Count} new messages in {Host}/{Username}.", uids.Count, settings.Host, settings.Username);

            var fetchItems = MessageSummaryItems.UniqueId
                           | MessageSummaryItems.Envelope
                           | MessageSummaryItems.Body;

            var summaries = await inbox.FetchAsync(uids, fetchItems, ct);
            var result = new List<FetchedMail>(summaries.Count);

            foreach (var summary in summaries)
            {
                try
                {
                    var mimeMessage = await inbox.GetMessageAsync(summary.UniqueId, ct);
                    result.Add(MapToFetchedMail(summary.UniqueId.Id, mimeMessage));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to fetch message UID={Uid} from {Host}. Skipping.",
                        summary.UniqueId.Id, settings.Host);
                }
            }

            await client.DisconnectAsync(true, ct);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "IMAP connection failed for {Host}/{Username}.", settings.Host, settings.Username);
            throw;
        }
    }

    private static FetchedMail MapToFetchedMail(uint uid, MimeMessage message)
    {
        var from = message.From.Mailboxes.FirstOrDefault();

        return new FetchedMail(
            Uid: uid,
            MessageId: message.MessageId ?? Guid.NewGuid().ToString(),
            Subject: message.Subject ?? string.Empty,
            FromAddress: from?.Address ?? string.Empty,
            FromName: from?.Name ?? string.Empty,
            BodyText: message.TextBody ?? string.Empty,
            BodyHtml: message.HtmlBody,
            SentAt: message.Date.UtcDateTime);
    }
}
