namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Streak value object tracking consecutive days of task completion.
/// Supports grace days to protect streaks from occasional missed days.
/// </summary>
public sealed record Streak
{
    public const int MaxGraceDays = 3;

    public int CurrentDays { get; }
    public DateOnly? LastActiveDate { get; }
    public int GraceDaysAvailable { get; }

    public Streak(int currentDays, DateOnly? lastActiveDate, int graceDaysAvailable)
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
    }

    public static Streak NewStreak() => new(0, null, 0);

    public Streak RecordCompletion(DateOnly today)
    {
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
        // Already active today — no action needed
        if (LastActiveDate == today)
        {
            return this;
        }

        // No streak to protect
        if (CurrentDays == 0)
        {
            return this;
        }

        // Day ended without completion — missed today
        if (LastActiveDate is not null && today.DayNumber > LastActiveDate.Value.DayNumber)
        {
            // Use grace day if available
            if (GraceDaysAvailable > 0)
            {
                return new Streak(CurrentDays, LastActiveDate, GraceDaysAvailable - 1);
            }

            // No grace days — reset streak
            return new Streak(0, LastActiveDate, 0);
        }

        return this;
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
