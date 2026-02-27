namespace MailReader.Domain.Primitives;

public abstract class AggregateRoot : Entity
{
    protected AggregateRoot(Guid id) : base(id) { }

    // EF Core
    protected AggregateRoot() { }
}
