using MailReader.Domain.Events;
using MailReader.Domain.Primitives;
using MailReader.Domain.ValueObjects;

namespace MailReader.Domain.Entities;

public sealed class Mailbox : AggregateRoot
{
    private Mailbox(
        Guid id,
        string name,
        ImapCredentials credentials) : base(id)
    {
        Name = name;
        Credentials = credentials;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    // EF Core
    private Mailbox() { }

    public string Name { get; private set; } = default!;
    public ImapCredentials Credentials { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// UID последнего прочитанного письма — используется чтобы не вычитывать письма повторно
    /// </summary>
    public uint LastSeenUid { get; private set; }

    public static Result<Mailbox> Create(string name, ImapCredentials credentials)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Mailbox>.Failure(MailboxErrors.NameEmpty);

        var mailbox = new Mailbox(Guid.NewGuid(), name, credentials);
        mailbox.RaiseDomainEvent(new MailboxCreatedEvent(mailbox.Id));

        return Result<Mailbox>.Success(mailbox);
    }

    public Result Update(string name, ImapCredentials credentials)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(MailboxErrors.NameEmpty);

        Name = name;
        Credentials = credentials;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new MailboxDeactivatedEvent(Id));
    }

    public void UpdateLastSeenUid(uint uid)
    {
        if (uid > LastSeenUid)
            LastSeenUid = uid;
    }
}

public static class MailboxErrors
{
    public static readonly Error NameEmpty = new("Mailbox.NameEmpty", "Mailbox name cannot be empty.");
    public static readonly Error NotFound = new("Mailbox.NotFound", "Mailbox with the specified ID was not found.");
}
