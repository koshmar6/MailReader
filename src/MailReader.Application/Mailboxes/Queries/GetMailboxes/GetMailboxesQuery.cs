using MailReader.Domain.Entities;
using MailReader.Domain.Interfaces;
using MediatR;

namespace MailReader.Application.Mailboxes.Queries.GetMailboxes;

public sealed record MailboxDto(
    Guid Id,
    string Name,
    string Host,
    int Port,
    string Username,
    bool UseSsl,
    bool IsActive,
    uint LastSeenUid,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// ── GetMailboxes ────────────────────────────────────────────────

public sealed record GetMailboxesQuery : IRequest<IReadOnlyList<MailboxDto>>;

public sealed class GetMailboxesQueryHandler(
    IMailboxRepository mailboxRepository) : IRequestHandler<GetMailboxesQuery, IReadOnlyList<MailboxDto>>
{
    public async Task<IReadOnlyList<MailboxDto>> Handle(GetMailboxesQuery request, CancellationToken ct)
    {
        var mailboxes = await mailboxRepository.GetAllActiveAsync(ct);
        return mailboxes.Select(ToDto).ToList();
    }

    private static MailboxDto ToDto(Mailbox m) => new(
        m.Id,
        m.Name,
        m.Credentials.Host,
        m.Credentials.Port,
        m.Credentials.Username,
        m.Credentials.UseSsl,
        m.IsActive,
        m.LastSeenUid,
        m.CreatedAt,
        m.UpdatedAt);
}