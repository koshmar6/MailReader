using MailReader.Domain.Primitives;

namespace MailReader.Domain.Events;

public sealed record MailboxCreatedEvent(Guid MailboxId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record MailboxDeactivatedEvent(Guid MailboxId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record MailMessageReceivedDomainEvent(Guid MessageId, Guid MailboxId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
