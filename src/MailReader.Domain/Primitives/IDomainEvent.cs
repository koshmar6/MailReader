using MediatR;

namespace MailReader.Domain.Primitives;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
