using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Quartz;

namespace EM2Devs.Todo.Worker.Jobs;

/// <summary>
/// Quartz job: scans active RecurringTasks and generates a new TodoTask
/// instance for each one whose IsDueForGeneration(today) returns true.
///
/// Cron: every 5 minutes (configured in Program.cs).
/// Idempotent: re-running the job within the same calendar day generates
/// nothing for already-processed daily tasks because IsDueForGeneration
/// re-checks LastGeneratedAt against today's date.
/// </summary>
[DisallowConcurrentExecution]
public sealed partial class RecurringTaskGenerationJob : IJob
{
    private readonly IRecurringTaskRepository _recurringRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<RecurringTaskGenerationJob> _logger;
    private readonly TimeProvider _timeProvider;

    public RecurringTaskGenerationJob(
        IRecurringTaskRepository recurringRepository,
        ITaskRepository taskRepository,
        ILogger<RecurringTaskGenerationJob> logger,
        TimeProvider timeProvider)
    {
        _recurringRepository = recurringRepository;
        _taskRepository = taskRepository;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        IReadOnlyList<RecurringTask> all = await _recurringRepository.GetAllAsync(context.CancellationToken).ConfigureAwait(false);

        int generated = 0;
        foreach (RecurringTask recurring in all)
        {
            // Note: RecurringTask.IsDueForGeneration has a known clock-skew asymmetry for Monthly. Tracked as a follow-up.
            if (!recurring.IsDueForGeneration(today))
            {
                continue;
            }

            TodoTask instance = recurring.GenerateNextInstance();
            await _taskRepository.SaveAsync(instance, context.CancellationToken).ConfigureAwait(false);

            recurring.MarkInstanceGenerated(today);
            await _recurringRepository.SaveAsync(recurring, context.CancellationToken).ConfigureAwait(false);

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
        Level = LogLevel.Information,
        Message = "RecurringTaskGenerationJob completed at {Today}. Generated {Generated} instances from {Total} recurring tasks.")]
    private static partial void LogJobCompleted(
        ILogger logger,
        DateOnly today,
        int generated,
        int total);
}
