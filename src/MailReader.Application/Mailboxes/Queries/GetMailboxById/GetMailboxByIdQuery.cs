using MailReader.Application.Mailboxes.Queries.GetMailboxes;
using MailReader.Domain.Interfaces;
using MailReader.Domain.Primitives;
using MediatR;

namespace MailReader.Application.Mailboxes.Queries.GetMailboxById;

public sealed record GetMailboxByIdQuery(Guid MailboxId) : IRequest<Result<MailboxDto>>;

public sealed class GetMailboxByIdQueryHandler(
    IMailboxRepository mailboxRepository) : IRequestHandler<GetMailboxByIdQuery, Result<MailboxDto>>
{
    public async Task<Result<MailboxDto>> Handle(GetMailboxByIdQuery request, CancellationToken ct)
    {
        var mailbox = await mailboxRepository.GetByIdAsync(request.MailboxId, ct);

        if (mailbox is null)
            return Result<MailboxDto>.Failure(Domain.Entities.MailboxErrors.NotFound);

        return Result<MailboxDto>.Success(new MailboxDto(
            mailbox.Id,
            mailbox.Name,
            mailbox.Credentials.Host,
            mailbox.Credentials.Port,
            mailbox.Credentials.Username,
            mailbox.Credentials.UseSsl,
            mailbox.IsActive,
            mailbox.LastSeenUid,
            mailbox.CreatedAt,
            mailbox.UpdatedAt));
    }
}