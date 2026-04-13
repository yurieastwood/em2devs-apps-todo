using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents an optional timer used during task execution.
/// When stopped, exposes elapsed time which can be used to auto-populate the actual time.
/// </summary>
public sealed record TaskTimer
{
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset? StoppedAt { get; }

    private TaskTimer(DateTimeOffset startedAt, DateTimeOffset? stoppedAt)
    {
        StartedAt = startedAt;
        StoppedAt = stoppedAt;
    }

    public static TaskTimer Start(DateTimeOffset startedAt)
    {
        return new TaskTimer(startedAt, null);
    }

    public TaskTimer Stop(DateTimeOffset stoppedAt)
    {
        if (StoppedAt.HasValue)
        {
            throw new DomainException("Timer has already been stopped.");
        }

        if (stoppedAt < StartedAt)
        {
            throw new DomainException("Stop time cannot precede start time.");
        }

        return new TaskTimer(StartedAt, stoppedAt);
    }

    public bool IsRunning => !StoppedAt.HasValue;

    /// <summary>
    /// Elapsed time between start and stop, or TimeSpan.Zero if the timer is still running.
    /// </summary>
    public TimeSpan Elapsed => StoppedAt.HasValue ? StoppedAt.Value - StartedAt : TimeSpan.Zero;

    /// <summary>
    /// Returns the elapsed time rounded up to whole minutes, suitable for auto-populating actual time.
    /// Throws when the timer is still running.
    /// </summary>
    public TimeEstimate ToTimeEstimate()
    {
        if (!StoppedAt.HasValue)
        {
            throw new DomainException("Timer must be stopped before converting to a time estimate.");
        }

        int minutes = Math.Max(1, (int)Math.Ceiling(Elapsed.TotalMinutes));
        return TimeEstimate.FromMinutes(minutes);
    }
}
