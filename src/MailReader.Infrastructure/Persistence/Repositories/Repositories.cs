using MailReader.Domain.Entities;
using MailReader.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MailReader.Infrastructure.Persistence.Repositories;

public sealed class MailboxRepository(AppDbContext dbContext) : IMailboxRepository
{
    public Task<Mailbox?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Mailboxes.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<Mailbox>> GetAllActiveAsync(CancellationToken ct = default) =>
        await dbContext.Mailboxes
            .Where(m => m.IsActive)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Mailboxes.AnyAsync(m => m.Id == id, ct);

    public void Add(Mailbox mailbox) => dbContext.Mailboxes.Add(mailbox);

    public void Update(Mailbox mailbox) => dbContext.Mailboxes.Update(mailbox);

    public void Remove(Mailbox mailbox) => dbContext.Mailboxes.Remove(mailbox);
}

public sealed class MailMessageRepository(AppDbContext dbContext) : IMailMessageRepository
{
    public Task<bool> ExistsByMessageIdAsync(Guid mailboxId, string messageId, CancellationToken ct = default) =>
        dbContext.MailMessages.AnyAsync(m => m.MailboxId == mailboxId && m.MessageId == messageId, ct);

    public Task<bool> ExistsByImapUidAsync(Guid mailboxId, uint imapUid, CancellationToken ct = default) =>
        dbContext.MailMessages.AnyAsync(m => m.MailboxId == mailboxId && m.ImapUid == imapUid, ct);

    public void Add(MailMessage message) => dbContext.MailMessages.Add(message);
}
