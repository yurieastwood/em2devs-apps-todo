using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.Entities;
using Microsoft.Extensions.Logging;
using Quartz;

namespace EM2Devs.Todo.Worker.Jobs;

/// <summary>
/// Quartz job: runs at midnight UTC, calls Streak.ProcessDayEnd against
/// the persisted profile, and writes a StreakSnapshot row capturing the
/// end-of-day state.
///
/// "End of day" is interpreted as the end of yesterday — when this job runs
/// at 00:00 UTC on day N, it processes the day-end for day (N-1).
///
/// Cron: 0 0 0 * * ? (midnight UTC, configured in Program.cs).
/// Idempotent: skips if a snapshot for the evaluated date already exists.
/// </summary>
[DisallowConcurrentExecution]
public sealed partial class DailyStreakEvaluationJob : IJob
{
    private readonly IPlayerProfileRepository _profileRepository;
    private readonly IStreakSnapshotRepository _snapshotRepository;
    private readonly ILogger<DailyStreakEvaluationJob> _logger;
    private readonly TimeProvider _timeProvider;

    public DailyStreakEvaluationJob(
        IPlayerProfileRepository profileRepository,
        IStreakSnapshotRepository snapshotRepository,
        ILogger<DailyStreakEvaluationJob> logger,
        TimeProvider timeProvider)
    {
        _profileRepository = profileRepository;
        _snapshotRepository = snapshotRepository;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        DateOnly evaluatedDay = today.AddDays(-1);

        // Idempotency: skip if a snapshot for evaluatedDay already exists.
        StreakSnapshot? existing = await _snapshotRepository
            .GetByDateAsync(evaluatedDay, context.CancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            LogSnapshotAlreadyExists(evaluatedDay);
            return;
        }

        // Snapshot the BEFORE state so we know if the user was active yesterday.
        PlayerProfileReadModel before = await _profileRepository
            .GetProfileAsync(context.CancellationToken)
            .ConfigureAwait(false);

        await _profileRepository
            .ProcessDayEndAsync(evaluatedDay, context.CancellationToken)
            .ConfigureAwait(false);

        PlayerProfileReadModel after = await _profileRepository
            .GetProfileAsync(context.CancellationToken)
            .ConfigureAwait(false);

        bool wasActive = before.CurrentStreak > 0 || after.CurrentStreak > 0;

        StreakSnapshot snapshot = StreakSnapshot.Capture(
            snapshotDate: evaluatedDay,
            currentDays: after.CurrentStreak,
            longestDays: after.LongestStreak,
            graceDaysAvailable: 0, // TODO Plan 3: surface from PlayerProfile read model
            wasActive: wasActive);

        await _snapshotRepository.SaveAsync(snapshot, context.CancellationToken).ConfigureAwait(false);

        LogSnapshotWritten(evaluatedDay, after.CurrentStreak, after.LongestStreak, wasActive);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "DailyStreakEvaluationJob: snapshot for {EvaluatedDay} already exists, skipping write")]
    private partial void LogSnapshotAlreadyExists(DateOnly evaluatedDay);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "DailyStreakEvaluationJob: wrote snapshot for {EvaluatedDay} (current={Current}, longest={Longest}, wasActive={WasActive})")]
    private partial void LogSnapshotWritten(DateOnly evaluatedDay, int current, int longest, bool wasActive);
}
