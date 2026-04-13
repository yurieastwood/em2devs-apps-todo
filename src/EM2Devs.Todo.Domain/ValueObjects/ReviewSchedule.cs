namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object representing the user's preferred weekly review schedule.
/// Defaults to Sunday at 6 PM when no preference is set.
/// </summary>
public sealed record ReviewSchedule
{
    public static readonly DayOfWeek DefaultDayOfWeek = DayOfWeek.Sunday;
    public static readonly TimeOnly DefaultTimeOfDay = new(18, 0);

    public DayOfWeek DayOfWeek { get; }
    public TimeOnly TimeOfDay { get; }

    public ReviewSchedule(DayOfWeek dayOfWeek, TimeOnly timeOfDay)
    {
        DayOfWeek = dayOfWeek;
        TimeOfDay = timeOfDay;
    }

    /// <summary>
    /// Creates the default review schedule: Sunday at 6 PM.
    /// </summary>
    public static ReviewSchedule Default() =>
        new(DefaultDayOfWeek, DefaultTimeOfDay);

    /// <summary>
    /// Determines whether the given date and time match this schedule.
    /// Compares the day of week and the hour/minute of the time.
    /// </summary>
    public bool IsScheduledTime(DayOfWeek day, TimeOnly time) =>
        DayOfWeek == day && TimeOfDay.Hour == time.Hour && TimeOfDay.Minute == time.Minute;
}
