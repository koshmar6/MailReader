using MailReader.Domain.Entities;
using MailReader.Domain.Interfaces;
using MailReader.Domain.Primitives;
using MailReader.Domain.ValueObjects;
using MediatR;

namespace MailReader.Application.Mailboxes.Commands.CreateMailbox;

public sealed record CreateMailboxCommand(
    string Name,
    string Host,
    int Port,
    string Username,
    string Password,
    bool UseSsl = true) : IRequest<Result<Guid>>;

public sealed class CreateMailboxCommandHandler(
    IMailboxRepository mailboxRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateMailboxCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMailboxCommand request, CancellationToken ct)
    {
        var credentialsResult = ImapCredentials.Create(
            request.Host,
            request.Port,
            request.Username,
            request.Password,
            request.UseSsl);

        if (credentialsResult.IsFailure)
            return Result<Guid>.Failure(credentialsResult.Error);

        var mailboxResult = Mailbox.Create(request.Name, credentialsResult.Value);

        if (mailboxResult.IsFailure)
            return Result<Guid>.Failure(mailboxResult.Error);

        mailboxRepository.Add(mailboxResult.Value);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(mailboxResult.Value.Id);
    }
}
