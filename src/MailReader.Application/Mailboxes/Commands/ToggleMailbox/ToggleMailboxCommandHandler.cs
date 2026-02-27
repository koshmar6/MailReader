using MailReader.Domain.Entities;
using MailReader.Domain.Interfaces;
using MailReader.Domain.Primitives;
using MediatR;

namespace MailReader.Application.Mailboxes.Commands.ToggleMailbox;

public sealed class ToggleMailboxCommandHandler(
    IMailboxRepository mailboxRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ToggleMailboxCommand, Result>
{
    public async Task<Result> Handle(ToggleMailboxCommand request, CancellationToken ct)
    {
        var mailbox = await mailboxRepository.GetByIdAsync(request.MailboxId, ct);

        if (mailbox is null)
        {
            return Result.Failure(MailboxErrors.NotFound);
        }

        if (request.Activate)
        {
            mailbox.Activate();
        }
        else
        {
            mailbox.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}