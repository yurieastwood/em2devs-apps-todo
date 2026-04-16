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
    private readonly ICalendarService _calendarService;

    public GetDailyBriefQueryHandler(
        ITaskRepository taskRepository,
        IPlayerProfileRepository profileRepository,
        ICurrentUser currentUser,
        TimeProvider timeProvider,
        ICalendarService calendarService)
    {
        _taskRepository = taskRepository;
        _profileRepository = profileRepository;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _calendarService = calendarService;
    }

    public async Task<Result<DailyBriefReadModel>> Handle(GetDailyBriefQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        IReadOnlyList<TodoTask> tasks = await _taskRepository.GetAllAsync(ct).ConfigureAwait(false);

        // "core plan" = today + overdue (ForToday). "overdue" is highlighted separately.
        IReadOnlyList<TodoTask> allTodayTasks = TaskViewFilter.ForToday(tasks, today);
        List<TodoTask> overdue = allTodayTasks.Where(t => t.ScheduledDate!.Value < today).ToList();

        // "if time allows" = first upcoming day that actually has tasks.
        IReadOnlyList<TodoTask> upcomingTasks = TaskViewFilter.ForUpcoming(tasks, today)
            .FirstOrDefault(g => g.Tasks.Count > 0)?.Tasks ?? [];

        int? dailyCapacity = ComputeCapacity(tasks, today);

        IReadOnlyList<CalendarBlock> calendarBlocks = await _calendarService
            .GetTodayBlocksAsync(today, ct).ConfigureAwait(false);
        int calendarBlockMinutes = calendarBlocks.Sum(b => b.DurationMinutes);

        if (calendarBlockMinutes > 0 && dailyCapacity.HasValue)
        {
            int minutesPerTask = 30;
            int blockedSlots = calendarBlockMinutes / minutesPerTask;
            dailyCapacity = Math.Max(1, dailyCapacity.Value - blockedSlots);
        }

        List<TodoTask> corePlan;
        List<TodoTask> ifTimeAllows;

        if (dailyCapacity.HasValue && allTodayTasks.Count > dailyCapacity.Value)
        {
            var sorted = allTodayTasks.OrderByDescending(t => t.Priority).ToList();
            corePlan = sorted.Take(dailyCapacity.Value).ToList();
            ifTimeAllows = sorted.Skip(dailyCapacity.Value).Concat(upcomingTasks).ToList();
        }
        else
        {
            corePlan = allTodayTasks.ToList();
            ifTimeAllows = upcomingTasks.ToList();
        }

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

        bool exceedsCapacity = dailyCapacity.HasValue && allTodayTasks.Count > dailyCapacity.Value;

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
            status,
            dailyCapacity,
            exceedsCapacity,
            calendarBlockMinutes);

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

    private static int? ComputeCapacity(IReadOnlyList<TodoTask> tasks, DateOnly today)
    {
        var completedByDay = tasks
            .Where(t => t.CompletedAt.HasValue && t.ScheduledDate.HasValue && t.ScheduledDate.Value < today)
            .GroupBy(t => t.ScheduledDate!.Value)
            .Where(g => g.Key.DayOfWeek == today.DayOfWeek)
            .Select(g => g.Count())
            .ToList();

        if (completedByDay.Count < 2)
        {
            return null;
        }

        return (int)Math.Round(completedByDay.Average());
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
