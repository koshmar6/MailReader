using MailReader.Application.Abstractions;
using MailReader.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MailReader.Application.Jobs;

/// <summary>
/// Рекуррентная джоба. Запускается раз в 5 минут.
/// Загружает все активные ящики и ставит дочернюю джобу на каждый.
/// </summary>
public sealed class ScheduleMailboxJobsJob(
    IMailboxRepository mailboxRepository,
    IJobScheduler jobScheduler,
    ILogger<ScheduleMailboxJobsJob> logger)
{
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var mailboxes = await mailboxRepository.GetAllActiveAsync(ct);

        if (mailboxes.Count == 0)
        {
            logger.LogInformation("No active mailboxes found. Skipping scheduling.");
            return;
        }

        logger.LogInformation("Scheduling read jobs for {Count} active mailboxes.", mailboxes.Count);

        foreach (var mailbox in mailboxes)
        {
            jobScheduler.Enqueue<ReadMailboxJob>(
                job => job.ExecuteAsync(mailbox.Id, CancellationToken.None));

            logger.LogDebug("Enqueued ReadMailboxJob for mailbox {MailboxId} ({MailboxName}).",
                mailbox.Id, mailbox.Name);
        }
    }
}
