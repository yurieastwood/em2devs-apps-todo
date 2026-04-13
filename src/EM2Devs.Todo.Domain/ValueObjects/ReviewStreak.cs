using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object tracking consecutive weeks of completing weekly reviews.
/// Supports a 1-week grace period so that missing a single week does not
/// immediately break the streak.
/// </summary>
public sealed record ReviewStreak
{
    public const int GracePeriodWeeks = 1;

    public int ConsecutiveWeeks { get; }
    public DateOnly? LastReviewWeek { get; }
    public bool IsPaused { get; }

    public ReviewStreak(int consecutiveWeeks, DateOnly? lastReviewWeek, bool isPaused = false)
    {
        if (consecutiveWeeks < 0)
        {
            throw new DomainException("Consecutive weeks cannot be negative.");
        }

        ConsecutiveWeeks = consecutiveWeeks;
        LastReviewWeek = lastReviewWeek;
        IsPaused = isPaused;
    }

    public static ReviewStreak NewStreak() => new(0, null);

    /// <summary>
    /// Records the completion of a weekly review for the given week.
    /// The weekStart should be the Monday of the review's week.
    /// </summary>
    public ReviewStreak RecordCompletion(DateOnly weekStart)
    {
        if (LastReviewWeek is null)
        {
            return new ReviewStreak(1, weekStart);
        }

        int weeksDifference = (weekStart.DayNumber - LastReviewWeek.Value.DayNumber) / 7;

        if (weeksDifference == 0)
        {
            return IsPaused
                ? new ReviewStreak(ConsecutiveWeeks, LastReviewWeek, isPaused: false)
                : this;
        }

        if (weeksDifference == 1)
        {
            return new ReviewStreak(ConsecutiveWeeks + 1, weekStart);
        }

        if (weeksDifference == 2 && IsPaused)
        {
            return new ReviewStreak(ConsecutiveWeeks + 1, weekStart);
        }

        return new ReviewStreak(1, weekStart);
    }

    /// <summary>
    /// Called when a week passes without a review. Pauses the streak
    /// and starts the grace period.
    /// </summary>
    public ReviewStreak MissWeek()
    {
        if (ConsecutiveWeeks == 0)
        {
            return this;
        }

        if (IsPaused)
        {
            return new ReviewStreak(0, LastReviewWeek);
        }

        return new ReviewStreak(ConsecutiveWeeks, LastReviewWeek, isPaused: true);
    }

    /// <summary>
    /// Calculates the week start (Monday) for a given date.
    /// </summary>
    public static DateOnly GetWeekStart(DateOnly date)
    {
        int daysFromMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-daysFromMonday);
    }
}
