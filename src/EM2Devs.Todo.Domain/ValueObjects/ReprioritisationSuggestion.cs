using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Identifies the action to take on a task when reprioritising to fit within capacity.
/// </summary>
public enum ReprioritisationAction
{
    Keep,
    Defer
}

/// <summary>
/// Represents a single suggestion within a reprioritisation plan.
/// </summary>
public sealed record ReprioritisationTaskSuggestion
{
    public TaskId TaskId { get; }
    public ReprioritisationAction Action { get; }
    public DateOnly? DeferTo { get; }

    public ReprioritisationTaskSuggestion(TaskId taskId, ReprioritisationAction action, DateOnly? deferTo)
    {
        ArgumentNullException.ThrowIfNull(taskId);
        if (action == ReprioritisationAction.Defer && !deferTo.HasValue)
        {
            throw new DomainException("Deferred tasks must specify a defer-to date.");
        }
        if (action == ReprioritisationAction.Keep && deferTo.HasValue)
        {
            throw new DomainException("Kept tasks must not specify a defer-to date.");
        }

        TaskId = taskId;
        Action = action;
        DeferTo = deferTo;
    }
}

/// <summary>
/// Represents a reprioritisation plan suggesting which tasks to defer to fit within capacity.
/// Tasks are ranked by priority-then-deadline; the lowest-ranked tasks are deferred first.
/// </summary>
public sealed record ReprioritisationSuggestion
{
    private readonly IReadOnlyList<ReprioritisationTaskSuggestion> _suggestions;

    public IReadOnlyList<ReprioritisationTaskSuggestion> Suggestions => _suggestions;
    public DayOfWeek Day { get; }
    public int Capacity { get; }
    public int ScheduledUnits { get; }

    private ReprioritisationSuggestion(
        DayOfWeek day,
        int capacity,
        int scheduledUnits,
        IReadOnlyList<ReprioritisationTaskSuggestion> suggestions)
    {
        Day = day;
        Capacity = capacity;
        ScheduledUnits = scheduledUnits;
        _suggestions = suggestions;
    }

    /// <summary>
    /// Builds a reprioritisation plan for a day given scheduled task units and an ordered list
    /// of candidate tasks (highest priority first). Deferred tasks are rescheduled to
    /// <paramref name="nextAvailableDay"/>.
    /// </summary>
    public static ReprioritisationSuggestion Build(
        DayOfWeek day,
        int capacity,
        IReadOnlyList<(TaskId Id, int Units)> orderedTasks,
        DateOnly nextAvailableDay)
    {
        ArgumentNullException.ThrowIfNull(orderedTasks);

        if (capacity < 0)
        {
            throw new DomainException("Capacity cannot be negative.");
        }

        int scheduledUnits = orderedTasks.Sum(t => t.Units);

        List<ReprioritisationTaskSuggestion> plan = new List<ReprioritisationTaskSuggestion>(orderedTasks.Count);
        int running = 0;
        foreach ((TaskId id, int units) in orderedTasks)
        {
            if (running + units <= capacity)
            {
                plan.Add(new ReprioritisationTaskSuggestion(id, ReprioritisationAction.Keep, null));
                running += units;
            }
            else
            {
                plan.Add(new ReprioritisationTaskSuggestion(id, ReprioritisationAction.Defer, nextAvailableDay));
            }
        }

        return new ReprioritisationSuggestion(day, capacity, scheduledUnits, plan);
    }
}
