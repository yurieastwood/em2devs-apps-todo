using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

/// <summary>
/// Computes today's daily brief for the authenticated user from current task data
/// and the player profile. Stateless and read-only: nothing is persisted — each call
/// regenerates the brief from scratch.
/// </summary>
public sealed record GetDailyBriefQuery : IRequest<Result<DailyBriefReadModel>>;

public sealed class GetDailyBriefQueryHandler
    : IRequestHandler<GetDailyBriefQuery, Result<DailyBriefReadModel>>
{
    /// <summary>Minimum number of core-plan items required to render a brief.</summary>
    internal const int MinimumCorePlanThreshold = 2;

    internal const string StatusAvailable = "Available";
    internal const string StatusInsufficientTasks = "InsufficientTasks";

    private readonly ITaskRepository _taskRepository;
    private readonly IPlayerProfileRepository _profileRepository;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public GetDailyBriefQueryHandler(
        ITaskRepository taskRepository,
        IPlayerProfileRepository profileRepository,
        ICurrentUser currentUser,
        TimeProvider timeProvider)
    {
        _taskRepository = taskRepository;
        _profileRepository = profileRepository;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Result<DailyBriefReadModel>> Handle(GetDailyBriefQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        IReadOnlyList<TodoTask> tasks = await _taskRepository.GetAllAsync(ct).ConfigureAwait(false);

        // "core plan" = today + overdue (ForToday). "overdue" is highlighted separately.
        IReadOnlyList<TodoTask> corePlan = TaskViewFilter.ForToday(tasks, today);
        List<TodoTask> overdue = corePlan.Where(t => t.ScheduledDate!.Value < today).ToList();

        // "if time allows" = first upcoming day that actually has tasks.
        IReadOnlyList<TodoTask> ifTimeAllows = TaskViewFilter.ForUpcoming(tasks, today)
            .FirstOrDefault(g => g.Tasks.Count > 0)?.Tasks ?? [];

        // Compute per-user estimation calibration from historical (estimated, actual) pairs.
        // NotEnoughData yields a neutral 1.0 factor and null CalibratedMinutes (UI falls back
        // to the raw estimate in that case).
        EstimationCalibration calibration = TimeEstimationCalibrator.Calibrate(tasks);

        PlayerProfileReadModel profile = await _profileRepository.GetProfileAsync(ct).ConfigureAwait(false);

        string displayName = string.IsNullOrWhiteSpace(_currentUser.DisplayName)
            ? "there"
            : _currentUser.DisplayName;
        string greeting = $"{GreetingFor(_timeProvider.GetUtcNow())}, {displayName}";

        string status = corePlan.Count < MinimumCorePlanThreshold
            ? StatusInsufficientTasks
            : StatusAvailable;

        DailyBriefReadModel brief = new(
            today,
            greeting,
            profile.CurrentStreak,
            corePlan.Count,
            ifTimeAllows.Count,
            overdue.Count,
            corePlan.Select(t => MapTask(t, calibration)).ToList(),
            ifTimeAllows.Select(t => MapTask(t, calibration)).ToList(),
            overdue.Select(t => MapTask(t, calibration)).ToList(),
            status);

        return brief;
    }

    private static DailyBriefTaskReadModel MapTask(TodoTask task, EstimationCalibration calibration)
    {
        int? estimated = task.EstimatedTime?.Minutes;
        int? calibrated = estimated.HasValue ? calibration.ApplyTo(estimated.Value) : null;

        return new DailyBriefTaskReadModel(
            task.Id.Value,
            task.Title.Value,
            task.Difficulty.ToString(),
            task.Priority.ToString(),
            estimated,
            calibrated,
            task.ScheduledDate);
    }

    private static string GreetingFor(DateTimeOffset now)
    {
        int hour = now.UtcDateTime.Hour;
        if (hour < 12)
        {
            return "Good morning";
        }
        if (hour < 18)
        {
            return "Good afternoon";
        }
        return "Good evening";
    }
}
