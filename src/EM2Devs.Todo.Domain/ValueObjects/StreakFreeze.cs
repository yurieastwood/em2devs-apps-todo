namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object representing an active streak freeze period.
/// A freeze protects a streak from being broken during a planned absence.
/// Maps to: docs/features/progression/streaks.feature — "Users can manually freeze their streak"
/// </summary>
public sealed record StreakFreeze
{
    public const int MaxFreezeDuration = 7;

    public DateOnly FrozenAt { get; }
    public int Duration { get; }

    public StreakFreeze(DateOnly frozenAt, int duration)
    {
        if (duration < 1)
        {
            throw new Exceptions.DomainException("Freeze duration must be at least 1 day.");
        }

        if (duration > MaxFreezeDuration)
        {
            throw new Exceptions.DomainException(
                $"The maximum freeze duration is 7 days.");
        }

        FrozenAt = frozenAt;
        Duration = duration;
    }

    /// <summary>
    /// Returns true if the freeze has expired as of the given date.
    /// The freeze covers <see cref="Duration"/> days starting from <see cref="FrozenAt"/>.
    /// </summary>
    public bool IsExpired(DateOnly asOfDate)
    {
        return asOfDate.DayNumber - FrozenAt.DayNumber >= Duration;
    }
}
