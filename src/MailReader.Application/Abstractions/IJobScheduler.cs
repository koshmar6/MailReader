namespace MailReader.Application.Abstractions;

/// <summary>
/// Абстракция для планирования фоновых задач.
/// Позволяет Application слою не зависеть от конкретной реализации (Hangfire).
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Ставит задачу в очередь на выполнение.
    /// </summary>
    void Enqueue<TJob>(System.Linq.Expressions.Expression<Func<TJob, Task>> methodCall);
}
