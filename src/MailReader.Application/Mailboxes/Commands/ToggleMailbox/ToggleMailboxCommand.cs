using MailReader.Domain.Primitives;
using MediatR;

namespace MailReader.Application.Mailboxes.Commands.ToggleMailbox;

public sealed record ToggleMailboxCommand(Guid MailboxId, bool Activate) : IRequest<Result>;