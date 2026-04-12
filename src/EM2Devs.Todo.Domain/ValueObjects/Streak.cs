namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Streak value object tracking consecutive days of task completion.
/// Supports grace days to protect streaks from occasional missed days,
/// and streak freezes for planned absences.
/// </summary>
public sealed record Streak
{
    public const int MaxGraceDays = 3;

    public int CurrentDays { get; }
    public DateOnly? LastActiveDate { get; }
    public int GraceDaysAvailable { get; }
    public StreakFreeze? ActiveFreeze { get; }

    public bool IsFrozen => ActiveFreeze is not null;

    public Streak(int currentDays, DateOnly? lastActiveDate, int graceDaysAvailable,
        StreakFreeze? activeFreeze = null)
    {
        if (currentDays < 0)
        {
            throw new Exceptions.DomainException("Streak days cannot be negative.");
        }

        if (graceDaysAvailable < 0)
        {
            throw new Exceptions.DomainException("Grace days cannot be negative.");
        }

        if (graceDaysAvailable > MaxGraceDays)
        {
            throw new Exceptions.DomainException($"Grace days cannot exceed {MaxGraceDays}.");
        }

        CurrentDays = currentDays;
        LastActiveDate = lastActiveDate;
        GraceDaysAvailable = graceDaysAvailable;
        ActiveFreeze = activeFreeze;
    }

    public static Streak NewStreak() => new(0, null, 0);

    /// <summary>
    /// Activates a streak freeze for the specified duration.
    /// While frozen, missed days do not break the streak.
    /// </summary>
    public Streak Freeze(DateOnly frozenAt, int duration)
    {
        if (IsFrozen)
        {
            throw new Exceptions.DomainException("Streak is already frozen.");
        }

        var freeze = new StreakFreeze(frozenAt, duration);
        return new Streak(CurrentDays, LastActiveDate, GraceDaysAvailable, freeze);
    }

    /// <summary>
    /// Manually ends an active streak freeze. The streak continues from where it was.
    /// </summary>
    public Streak Unfreeze(DateOnly unfreezeDate)
    {
        if (!IsFrozen)
        {
            return this;
        }

        return new Streak(CurrentDays, unfreezeDate, GraceDaysAvailable);
    }

    public Streak RecordCompletion(DateOnly today)
    {
        // During a freeze, record the active date but don't change streak days
        if (IsFrozen)
        {
            return new Streak(CurrentDays, today, GraceDaysAvailable, ActiveFreeze);
        }

        // Already completed today — no change
        if (LastActiveDate == today)
        {
            return this;
        }

        // Consecutive day — increment streak
        if (LastActiveDate is not null && today.DayNumber - LastActiveDate.Value.DayNumber == 1)
        {
            return new Streak(CurrentDays + 1, today, GraceDaysAvailable);
        }

        // First ever completion or missed days without grace — start new streak
        return new Streak(1, today, GraceDaysAvailable);
    }

    public Streak ProcessDayEnd(DateOnly today)
    {
        // If frozen, check if freeze has expired
        if (IsFrozen)
        {
            if (ActiveFreeze!.IsExpired(today))
            {
                // Freeze expired — remove it, set LastActiveDate to today so next
                // completion counts as consecutive
                return new Streak(CurrentDays, today, GraceDaysAvailable);
            }

            // Still within freeze period — skip streak-break logic
            return this;
        }

        // Only act if there's an active streak that missed today
        bool missedToday = CurrentDays > 0
            && LastActiveDate is not null
            && today.DayNumber > LastActiveDate.Value.DayNumber;

        if (!missedToday)
        {
            return this;
        }

        // Use grace day if available
        if (GraceDaysAvailable > 0)
        {
            return new Streak(CurrentDays, LastActiveDate, GraceDaysAvailable - 1);
        }

        // No grace days — reset streak
        return new Streak(0, LastActiveDate, 0);
    }

    /// <summary>
    /// Returns the milestone reached by the current streak day count, or null if not a milestone.
    /// Typically called after <see cref="RecordCompletion"/> to detect newly reached milestones.
    /// </summary>
    public StreakMilestone? CheckMilestone()
    {
        return StreakMilestone.ForDays(CurrentDays);
    }

    public Streak AddGraceDay()
    {
        if (GraceDaysAvailable >= MaxGraceDays)
        {
            return this;
        }

        return new Streak(CurrentDays, LastActiveDate, GraceDaysAvailable + 1);
    }
}
