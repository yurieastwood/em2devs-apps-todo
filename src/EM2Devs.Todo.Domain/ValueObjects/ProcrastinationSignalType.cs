namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Types of procrastination signals detected by the system.
/// </summary>
public enum ProcrastinationSignalType
{
    /// <summary>Task rescheduled 3+ times.</summary>
    RepeatedRescheduling,

    /// <summary>Task viewed 5+ times without status change.</summary>
    RepeatedViewingWithoutAction,

    /// <summary>High/critical priority task skipped in favour of lower-priority tasks.</summary>
    HighPrioritySkipped,

    /// <summary>Task is overdue by 7+ days with no progress.</summary>
    OverduePastThreshold
}
