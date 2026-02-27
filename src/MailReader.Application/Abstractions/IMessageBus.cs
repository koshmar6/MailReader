namespace MailReader.Application.Abstractions;

public interface IMessageBus
{
    Task PublishAsync<TMessage>(TMessage message, CancellationToken ct = default)
        where TMessage : class;
}
