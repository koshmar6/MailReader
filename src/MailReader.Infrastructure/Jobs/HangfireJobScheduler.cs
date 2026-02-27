using Hangfire;
using MailReader.Application.Abstractions;

namespace MailReader.Infrastructure.Jobs;

/// <summary>
/// Реализация IJobScheduler через Hangfire.
/// </summary>
public sealed class HangfireJobScheduler(IBackgroundJobClient client) : IJobScheduler
{
    public void Enqueue<TJob>(System.Linq.Expressions.Expression<Func<TJob, Task>> methodCall) =>
        client.Enqueue(methodCall);
}
