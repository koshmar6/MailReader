using Wolverine;

namespace MailReader.Infrastructure.Messaging;

/// <summary>
///     Реализация IMessageBus через Wolverine.
///     Wolverine сам маршрутизирует сообщение в Kafka на основе конфигурации в Program.cs
/// </summary>
public sealed class WolverineMessageBus(IMessageBus bus) : Application.Abstractions.IMessageBus
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken ct = default)
        where TMessage : class =>
        bus.PublishAsync(message).AsTask();
}