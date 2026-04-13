using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Tracks how many times a user has dismissed capacity warnings within the current week.
/// When dismissals exceed the threshold, warnings are displayed in a reduced, non-intrusive form
/// until the following week.
/// </summary>
public sealed record CapacityDismissalTracker
{
    /// <summary>
    /// Threshold of dismissals after which the warning is reduced to a non-intrusive indicator.
    /// </summary>
    public const int DismissalThreshold = 3;

    public int DismissalCount { get; }
    public DateOnly WeekStart { get; }

    private CapacityDismissalTracker(int dismissalCount, DateOnly weekStart)
    {
        DismissalCount = dismissalCount;
        WeekStart = weekStart;
    }

    public static CapacityDismissalTracker StartWeek(DateOnly weekStart)
    {
        return new CapacityDismissalTracker(0, weekStart);
    }

    /// <summary>
    /// Returns a tracker with the dismissal recorded. If the provided date is outside the
    /// current week, the tracker resets for the new week.
    /// </summary>
    public CapacityDismissalTracker RecordDismissal(DateOnly dismissalDate)
    {
        if (dismissalDate < WeekStart)
        {
            throw new DomainException("Dismissal date cannot precede the tracker's week start.");
        }

        if (dismissalDate >= WeekStart.AddDays(7))
        {
            DateOnly newWeekStart = StartOfWeekFor(dismissalDate);
            return new CapacityDismissalTracker(1, newWeekStart);
        }

        return new CapacityDismissalTracker(DismissalCount + 1, WeekStart);
    }

    /// <summary>
    /// Indicates whether warnings should be presented in reduced form rather than as a modal/interruptive prompt.
    /// </summary>
    public bool ShouldReduceWarning => DismissalCount >= DismissalThreshold;

    private static DateOnly StartOfWeekFor(DateOnly date)
    {
        int diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }
}
