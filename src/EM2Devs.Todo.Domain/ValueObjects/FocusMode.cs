namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a focus mode session for a Boss Task.
/// Tracks start time, task, active state, and duration.
/// Maps to: boss-tasks.feature — "Trigger focus mode for a Boss Task"
/// </summary>
public sealed record FocusMode
{
    public TaskId TaskId { get; }
    public DateTimeOffset StartedAt { get; }
    public bool IsActive { get; }
    public TimeSpan Duration { get; }

    private FocusMode(TaskId taskId, DateTimeOffset startedAt, bool isActive, TimeSpan duration)
    {
        TaskId = taskId;
        StartedAt = startedAt;
        IsActive = isActive;
        Duration = duration;
    }

    public static FocusMode Start(TaskId taskId, DateTimeOffset startedAt)
    {
        return new FocusMode(taskId, startedAt, isActive: true, duration: TimeSpan.Zero);
    }

    public FocusMode End(DateTimeOffset endedAt)
    {
        if (!IsActive)
        {
            throw new Exceptions.DomainException("Focus mode has already ended.");
        }

        if (endedAt < StartedAt)
        {
            throw new Exceptions.DomainException("End time cannot be before start time.");
        }

        return new FocusMode(TaskId, StartedAt, isActive: false, duration: endedAt - StartedAt);
    }
}
