using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// A personalised daily plan recommending a prioritised task sequence.
/// Contains a core plan (capacity-limited), optional "if time allows" overflow,
/// highlighted overdue tasks, and feedback tracking for brief accuracy improvement.
/// Users can accept, modify, or dismiss the brief.
/// </summary>
public sealed class DailyBrief
{
    private const int MinimumTaskThreshold = 2;

    public DailyBriefId Id { get; }
    public DateOnly Date { get; }
    public IReadOnlyList<TaskId> CorePlan { get; private set; }
    public IReadOnlyList<TaskId> IfTimeAllows { get; private set; }
    public IReadOnlyList<TaskId> OverdueTasks { get; }
    public int Capacity { get; }
    public DailyBriefStatus Status { get; private set; }
    public bool HasCalendarIntegration { get; }
    public int CalendarBlockMinutes { get; }
    public int FeedbackAcceptCount { get; private set; }
    public int FeedbackModifyCount { get; private set; }
    public int FeedbackDismissCount { get; private set; }

    public bool HasOverdueTasks => OverdueTasks.Count > 0;
    public bool ExceedsCapacity => CorePlan.Count > Capacity;
    public int TotalTaskCount => CorePlan.Count + IfTimeAllows.Count;

    public string? CapacityWarning => ExceedsCapacity
        ? $"This plan exceeds your typical daily capacity of {Capacity} tasks — you may want to mark some as 'if time allows'"
        : null;

    private DailyBrief(
        DailyBriefId id,
        DateOnly date,
        IReadOnlyList<TaskId> corePlan,
        IReadOnlyList<TaskId> ifTimeAllows,
        int capacity,
        IReadOnlyList<TaskId> overdueTasks,
        bool hasCalendarIntegration,
        int calendarBlockMinutes,
        int acceptCount,
        int modifyCount,
        int dismissCount)
    {
        Id = id;
        Date = date;
        CorePlan = corePlan;
        IfTimeAllows = ifTimeAllows;
        Capacity = capacity;
        OverdueTasks = overdueTasks;
        HasCalendarIntegration = hasCalendarIntegration;
        CalendarBlockMinutes = calendarBlockMinutes;
        FeedbackAcceptCount = acceptCount;
        FeedbackModifyCount = modifyCount;
        FeedbackDismissCount = dismissCount;
        Status = DailyBriefStatus.Generated;
    }

    public static DailyBrief Create(
        DateOnly date,
        IReadOnlyList<TaskId> corePlan,
        IReadOnlyList<TaskId> ifTimeAllows,
        int capacity,
        IReadOnlyList<TaskId>? overdueTasks = null,
        bool hasCalendarIntegration = false,
        int calendarBlockMinutes = 0,
        int acceptCount = 0,
        int modifyCount = 0,
        int dismissCount = 0)
    {
        ArgumentNullException.ThrowIfNull(corePlan);
        ArgumentNullException.ThrowIfNull(ifTimeAllows);
        overdueTasks ??= [];

        if (corePlan.Count + ifTimeAllows.Count < MinimumTaskThreshold)
        {
            throw new DomainException(
                $"Daily brief must contain at least {MinimumTaskThreshold} tasks.");
        }

        if (capacity < 0)
        {
            throw new DomainException("Capacity cannot be negative.");
        }

        if (calendarBlockMinutes < 0)
        {
            throw new DomainException("Calendar block minutes cannot be negative.");
        }

        if (acceptCount < 0)
        {
            throw new DomainException("Feedback accept count cannot be negative.");
        }

        if (modifyCount < 0)
        {
            throw new DomainException("Feedback modify count cannot be negative.");
        }

        if (dismissCount < 0)
        {
            throw new DomainException("Feedback dismiss count cannot be negative.");
        }

        return new DailyBrief(
            DailyBriefId.New(), date, corePlan, ifTimeAllows, capacity,
            overdueTasks, hasCalendarIntegration, calendarBlockMinutes,
            acceptCount, modifyCount, dismissCount);
    }

    public void Accept()
    {
        Status = DailyBriefStatus.Accepted;
        FeedbackAcceptCount++;
    }

    public void Dismiss()
    {
        Status = DailyBriefStatus.Dismissed;
        FeedbackDismissCount++;
    }

    public void Modify(IReadOnlyList<TaskId> newCorePlan, IReadOnlyList<TaskId> newIfTimeAllows)
    {
        ArgumentNullException.ThrowIfNull(newCorePlan);
        ArgumentNullException.ThrowIfNull(newIfTimeAllows);

        if (newCorePlan.Count == 0)
        {
            throw new DomainException("Modified brief must contain at least one task.");
        }

        CorePlan = newCorePlan;
        IfTimeAllows = newIfTimeAllows;
        Status = DailyBriefStatus.Modified;
        FeedbackModifyCount++;
    }
}
