using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using EM2Devs.Todo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Quartz;

namespace EM2Devs.Todo.Worker.Jobs;

/// <summary>
/// Quartz job: scans active RecurringTasks and generates a new TodoTask instance
/// for each one that is due for generation today.
///
/// "Is due" is decided by <see cref="RecurringTask.IsDueForGeneration(DateOnly?, DateOnly)"/>, a
/// pure function that takes the last scheduled date of this template's instances plus today. The
/// single source of truth for "last generation" is the instances table itself — this job queries
/// <see cref="ITaskRepository.GetByRecurringTaskIdAsync"/> and picks the max scheduled date.
///
/// Cron: every 5 minutes (configured in Program.cs).
/// Idempotent: re-running within the same calendar day generates nothing for already-processed
/// daily tasks because the new instance carries today's scheduled date, and the predicate will
/// return false on the next tick.
///
/// Note: <see cref="RecurringTask.IsDueForGeneration(DateOnly?, DateOnly)"/> has a known clock-skew
/// asymmetry for Monthly when <paramref name="lastScheduledDate"/> is in a future month. Tracked
/// as a follow-up.
/// </summary>
[DisallowConcurrentExecution]
public sealed partial class RecurringTaskGenerationJob : IJob
{
    private readonly IRecurringTaskRepository _recurringRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly TodoDbContext _dbContext;
    private readonly ILogger<RecurringTaskGenerationJob> _logger;
    private readonly TimeProvider _timeProvider;

    public RecurringTaskGenerationJob(
        IRecurringTaskRepository recurringRepository,
        ITaskRepository taskRepository,
        TodoDbContext dbContext,
        ILogger<RecurringTaskGenerationJob> logger,
        TimeProvider timeProvider)
    {
        _recurringRepository = recurringRepository;
        _taskRepository = taskRepository;
        _dbContext = dbContext;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        // Slice 2: bypass the per-user filter — the job runs outside any user scope and
        // generates instances on behalf of every owning user. Each recurring task carries
        // its UserId, which flows onto the generated TodoTask via GenerateNextInstance.
        IReadOnlyList<RecurringTask> all = await _recurringRepository
            .GetAllForGenerationAsync(context.CancellationToken)
            .ConfigureAwait(false);

        int generated = 0;
        foreach (RecurringTask recurring in all)
        {
            if (!recurring.IsActive)
            {
                continue;
            }

            DateOnly? lastScheduledDate = await _taskRepository
                .GetMaxScheduledDateForGenerationAsync(recurring.Id, context.CancellationToken)
                .ConfigureAwait(false);

            if (!recurring.IsDueForGeneration(lastScheduledDate, today))
            {
                continue;
            }

            TodoTask instance = recurring.GenerateNextInstance(today);
            try
            {
                await _taskRepository.SaveForGenerationAsync(instance, context.CancellationToken).ConfigureAwait(false);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                // Unique constraint on (source_recurring_task_id, scheduled_date) — another
                // process already generated an instance for this recurring task + date.
                // Clear the tracker so the failed Added entity doesn't poison subsequent
                // iterations' SaveChangesAsync calls.
                _dbContext.ChangeTracker.Clear();
                LogDuplicateSkipped(_logger, recurring.Id, today);
                continue;
            }

            generated++;
            LogInstanceGenerated(_logger, instance.Id, recurring.Id, recurring.Title.Value);
        }

        LogJobCompleted(_logger, today, generated, all.Count);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Generated instance {InstanceId} for recurring task {RecurringTaskId} ({Title})")]
    private static partial void LogInstanceGenerated(
        ILogger logger,
        TaskId instanceId,
        RecurringTaskId recurringTaskId,
        string title);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Duplicate instance for recurring task {RecurringTaskId} on {ScheduledDate} — another process already generated it; skipping.")]
    private static partial void LogDuplicateSkipped(
        ILogger logger,
        RecurringTaskId recurringTaskId,
        DateOnly scheduledDate);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "RecurringTaskGenerationJob completed at {Today}. Generated {Generated} instances from {Total} recurring tasks.")]
    private static partial void LogJobCompleted(
        ILogger logger,
        DateOnly today,
        int generated,
        int total);
}
