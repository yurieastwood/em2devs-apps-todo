using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object tracking consecutive weeks of completed weekly reviews.
/// Supports a 1-week grace period: if a review is missed, the streak is paused
/// and the user has one additional week to catch up before the streak resets.
/// </summary>
public sealed record ReviewStreak
{
    public const int GracePeriodWeeks = 1;
    public const int ConsistentPlannerThreshold = 4;

    public int ConsecutiveWeeks { get; }
    public DateOnly? LastReviewWeekStart { get; }
    public bool IsPaused { get; }

    public ReviewStreak(int consecutiveWeeks, DateOnly? lastReviewWeekStart, bool isPaused)
    {
        if (consecutiveWeeks < 0)
        {
            throw new DomainException("Review streak weeks cannot be negative.");
        }

        ConsecutiveWeeks = consecutiveWeeks;
        LastReviewWeekStart = lastReviewWeekStart;
        IsPaused = isPaused;
    }

    public static ReviewStreak New() => new(0, null, false);

    /// <summary>
    /// Records a completed review for the given week. The weekStart parameter
    /// represents the Monday of the week being reviewed.
    /// Increments the streak and clears any paused state.
    /// </summary>
    public ReviewStreak RecordCompletion(DateOnly weekStart)
    {
        if (LastReviewWeekStart is null)
        {
            return new ReviewStreak(1, weekStart, false);
        }

        int daysDiff = weekStart.DayNumber - LastReviewWeekStart.Value.DayNumber;

        // Same week — no change
        if (daysDiff == 0)
        {
            return this;
        }

        // Consecutive week (7 days)
        if (daysDiff == 7)
        {
            return new ReviewStreak(ConsecutiveWeeks + 1, weekStart, false);
        }

        // Within grace period: 14 days = missed 1 week, catching up now
        if (daysDiff == 14)
        {
            return new ReviewStreak(ConsecutiveWeeks + 1, weekStart, false);
        }

        // Paused streak: gap can be up to 21 days (missed week + current week)
        // when ProcessWeekEnd paused the streak but lastReviewWeekStart wasn't advanced
        if (IsPaused && daysDiff == 21)
        {
            return new ReviewStreak(ConsecutiveWeeks + 1, weekStart, false);
        }

        // Beyond grace period — reset
        return new ReviewStreak(1, weekStart, false);
    }

    /// <summary>
    /// Evaluates the streak at the end of a week. If the current week was missed
    /// and the streak was active, pauses it with a grace period.
    /// If already paused and another week passes, the streak resets.
    /// </summary>
    public ReviewStreak ProcessWeekEnd(DateOnly currentWeekStart)
    {
        if (LastReviewWeekStart is null)
        {
            return this;
        }

        int daysDiff = currentWeekStart.DayNumber - LastReviewWeekStart.Value.DayNumber;

        // Current week or just completed — no action needed
        if (daysDiff <= 7)
        {
            return this;
        }

        // Missed one week — pause if not already paused
        if (daysDiff <= 14 && !IsPaused)
        {
            return new ReviewStreak(ConsecutiveWeeks, LastReviewWeekStart, true);
        }

        // Already paused and missed another week, or gap too large — reset
        return new ReviewStreak(0, null, false);
    }

    /// <summary>
    /// Returns true if the review streak has reached the Consistent Planner
    /// title threshold (4 consecutive weeks).
    /// </summary>
    public bool HasReachedConsistentPlannerThreshold() =>
        ConsecutiveWeeks >= ConsistentPlannerThreshold;
}
