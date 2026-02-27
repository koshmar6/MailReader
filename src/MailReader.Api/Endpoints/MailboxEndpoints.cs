using MailReader.Application.Mailboxes.Commands.CreateMailbox;
using MailReader.Application.Mailboxes.Commands.DeleteMailbox;
using MailReader.Application.Mailboxes.Commands.ToggleMailbox;
using MailReader.Application.Mailboxes.Commands.UpdateMailbox;
using MailReader.Application.Mailboxes.Queries.GetMailboxById;
using MailReader.Application.Mailboxes.Queries.GetMailboxes;
using MailReader.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MailReader.Api.Endpoints;

public static class MailboxEndpoints
{
    public static void MapMailboxEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/mailboxes")
            .WithTags("Mailboxes");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:guid}", Update);
        group.MapDelete("/{id:guid}", Delete);
        group.MapPatch("/{id:guid}/toggle", Toggle);
    }

    private static async Task<IResult> GetAll(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMailboxesQuery(), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMailboxByIdQuery(id), ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error);
    }

    private static async Task<IResult> Create(
        [FromBody] CreateMailboxCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? Results.Created($"/api/mailboxes/{result.Value}", new { id = result.Value })
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> Update(
        Guid id,
        [FromBody] UpdateMailboxRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new UpdateMailboxCommand(
            id,
            request.Name,
            request.Host,
            request.Port,
            request.Username,
            request.Password,
            request.UseSsl);

        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(result.Error.Message, statusCode: result.Error == MailboxErrors.NotFound ? 404 : 400);
    }

    private static async Task<IResult> Delete(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteMailboxCommand(id), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.Error);
    }

    private static async Task<IResult> Toggle(
        Guid id,
        [FromBody] ToggleMailboxRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new ToggleMailboxCommand(id, request.Activate), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.Error);
    }
}

public sealed record UpdateMailboxRequest(
    string Name,
    string Host,
    int Port,
    string Username,
    string Password,
    bool UseSsl = true);

public sealed record ToggleMailboxRequest(bool Activate);