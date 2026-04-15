using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

/// <summary>
/// Computes the weekly-review read model for the authenticated user. The
/// review week is anchored on a Sunday; callers may pass <paramref name="WeekOf"/>
/// explicitly or omit it to use the current week's Sunday (local UTC).
/// The read model aggregates completed tasks, XP earned, streak delta, and any
/// previously-saved reflection for that week.
/// </summary>
public sealed record GetWeeklyReviewQuery(DateOnly? WeekOf = null) : IRequest<Result<WeeklyReviewReadModel>>;

public sealed class GetWeeklyReviewQueryHandler
    : IRequestHandler<GetWeeklyReviewQuery, Result<WeeklyReviewReadModel>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IPlayerProfileRepository _profileRepository;
    private readonly IWeeklyReflectionRepository _reflectionRepository;
    private readonly TimeProvider _timeProvider;

    public GetWeeklyReviewQueryHandler(
        ITaskRepository taskRepository,
        IPlayerProfileRepository profileRepository,
        IWeeklyReflectionRepository reflectionRepository,
        TimeProvider timeProvider)
    {
        _taskRepository = taskRepository;
        _profileRepository = profileRepository;
        _reflectionRepository = reflectionRepository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<WeeklyReviewReadModel>> Handle(GetWeeklyReviewQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        DateOnly weekOf = request.WeekOf ?? GetWeekOfSunday(today);
        DateOnly weekEnd = weekOf.AddDays(6);

        IReadOnlyList<TodoTask> tasks = await _taskRepository.GetAllAsync(ct).ConfigureAwait(false);

        List<TodoTask> completedThisWeek = tasks
            .Where(t => t.CompletedAt.HasValue)
            .Where(t =>
            {
                DateOnly completedOn = DateOnly.FromDateTime(t.CompletedAt!.Value.UtcDateTime);
                return completedOn >= weekOf && completedOn <= weekEnd;
            })
            .ToList();

        PlayerProfileReadModel profile = await _profileRepository.GetProfileAsync(ct).ConfigureAwait(false);

        int xpEarned = 0;
        if (profile.XpHistory is not null)
        {
            foreach (XpHistoryEntryReadModel entry in profile.XpHistory)
            {
                if (entry.Date >= weekOf && entry.Date <= weekEnd)
                {
                    xpEarned += entry.XpEarned;
                }
            }
        }

        int streakEnd = profile.CurrentStreak;
        // Best-effort approximation: streak at start of week is (current - days survived
        // this week), floored at zero. The streak domain does not expose a time-series here,
        // so we use the fact that a live streak increments at most once per day.
        int daysElapsed = (int)Math.Min(7, Math.Max(0, (today.DayNumber - weekOf.DayNumber) + 1));
        int streakStart = Math.Max(0, streakEnd - daysElapsed);

        List<string> notableEvents = [];
        if (profile.XpHistory is not null)
        {
            foreach (XpHistoryEntryReadModel entry in profile.XpHistory)
            {
                if (entry.Date >= weekOf && entry.Date <= weekEnd && entry.XpEarned > 0)
                {
                    notableEvents.Add($"+{entry.XpEarned} XP ({entry.Source}) on {entry.Date:yyyy-MM-dd}");
                }
            }
        }
        if (completedThisWeek.Count > 0)
        {
            notableEvents.Insert(0, $"Completed {completedThisWeek.Count} task(s) this week");
        }

        WeeklyReflectionReadModel? reflection = await _reflectionRepository
            .GetAsync(weekOf, ct)
            .ConfigureAwait(false);

        WeeklyReviewReadModel readModel = new(
            weekOf,
            completedThisWeek.Count,
            xpEarned,
            streakStart,
            streakEnd,
            notableEvents,
            reflection);

        return readModel;
    }

    /// <summary>
    /// Returns the Sunday that anchors the review week containing <paramref name="date"/>.
    /// If <paramref name="date"/> is itself a Sunday, it is returned unchanged.
    /// </summary>
    public static DateOnly GetWeekOfSunday(DateOnly date)
    {
        int daysSinceSunday = (int)date.DayOfWeek;
        return date.AddDays(-daysSinceSunday);
    }
}
