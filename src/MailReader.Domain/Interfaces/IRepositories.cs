using MailReader.Domain.Entities;

namespace MailReader.Domain.Interfaces;

public interface IMailboxRepository
{
    Task<Mailbox?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Mailbox>> GetAllActiveAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    void Add(Mailbox mailbox);
    void Update(Mailbox mailbox);
    void Remove(Mailbox mailbox);
}

public interface IMailMessageRepository
{
    Task<bool> ExistsByMessageIdAsync(Guid mailboxId, string messageId, CancellationToken ct = default);
    Task<bool> ExistsByImapUidAsync(Guid mailboxId, uint imapUid, CancellationToken ct = default);
    void Add(MailMessage message);
}
