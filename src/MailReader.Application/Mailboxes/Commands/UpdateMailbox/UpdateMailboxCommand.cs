using MailReader.Domain.Interfaces;
using MailReader.Domain.Primitives;
using MailReader.Domain.ValueObjects;
using MediatR;

namespace MailReader.Application.Mailboxes.Commands.UpdateMailbox;

public sealed record UpdateMailboxCommand(
    Guid MailboxId,
    string Name,
    string Host,
    int Port,
    string Username,
    string Password,
    bool UseSsl = true) : IRequest<Result>;

public sealed class UpdateMailboxCommandHandler(
    IMailboxRepository mailboxRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateMailboxCommand, Result>
{
    public async Task<Result> Handle(UpdateMailboxCommand request, CancellationToken ct)
    {
        var mailbox = await mailboxRepository.GetByIdAsync(request.MailboxId, ct);

        if (mailbox is null)
            return Result.Failure(Domain.Entities.MailboxErrors.NotFound);

        var credentialsResult = ImapCredentials.Create(
            request.Host,
            request.Port,
            request.Username,
            request.Password,
            request.UseSsl);

        if (credentialsResult.IsFailure)
            return Result.Failure(credentialsResult.Error);

        var updateResult = mailbox.Update(request.Name, credentialsResult.Value);

        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
