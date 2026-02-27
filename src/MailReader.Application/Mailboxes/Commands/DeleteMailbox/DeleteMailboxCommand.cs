using MailReader.Domain.Entities;
using MailReader.Domain.Interfaces;
using MailReader.Domain.Primitives;
using MediatR;

namespace MailReader.Application.Mailboxes.Commands.DeleteMailbox;

public sealed record DeleteMailboxCommand(Guid MailboxId) : IRequest<Result>;

public sealed class DeleteMailboxCommandHandler(
    IMailboxRepository mailboxRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteMailboxCommand, Result>
{
    public async Task<Result> Handle(DeleteMailboxCommand request, CancellationToken ct)
    {
        var mailbox = await mailboxRepository.GetByIdAsync(request.MailboxId, ct);

        if (mailbox is null)
        {
            return Result.Failure(MailboxErrors.NotFound);
        }

        mailboxRepository.Remove(mailbox);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}