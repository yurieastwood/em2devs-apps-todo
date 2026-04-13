using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object representing the user's preferred weekly review schedule.
/// Consists of a day of the week and a time of day.
/// Defaults to Sunday at 6 PM when no preference is set.
/// </summary>
public sealed record ReviewSchedule
{
    public static readonly ReviewSchedule Default = new(DayOfWeek.Sunday, new TimeOnly(18, 0));

    public DayOfWeek DayOfWeek { get; }
    public TimeOnly TimeOfDay { get; }

    public ReviewSchedule(DayOfWeek dayOfWeek, TimeOnly timeOfDay)
    {
        if (!Enum.IsDefined(dayOfWeek))
        {
            throw new DomainException("Invalid day of week.");
        }

        DayOfWeek = dayOfWeek;
        TimeOfDay = timeOfDay;
    }

    /// <summary>
    /// Parses a schedule string like "Saturday at 10 AM" into a ReviewSchedule.
    /// </summary>
    public static ReviewSchedule Parse(string scheduleText)
    {
        if (string.IsNullOrWhiteSpace(scheduleText))
        {
            throw new DomainException("Schedule text cannot be empty.");
        }

        string normalized = scheduleText.Trim();
        int atIndex = normalized.IndexOf(" at ", StringComparison.OrdinalIgnoreCase);
        if (atIndex == -1)
        {
            throw new DomainException($"Invalid schedule format: '{scheduleText}'. Expected format: 'DayOfWeek at Time'.");
        }

        string dayPart = normalized[..atIndex].Trim();
        string timePart = normalized[(atIndex + 4)..].Trim();

        if (!Enum.TryParse<DayOfWeek>(dayPart, ignoreCase: true, out DayOfWeek day))
        {
            throw new DomainException($"Invalid day of week: '{dayPart}'.");
        }

        if (!TryParseTime(timePart, out TimeOnly time))
        {
            throw new DomainException($"Invalid time: '{timePart}'.");
        }

        return new ReviewSchedule(day, time);
    }

    private static bool TryParseTime(string timePart, out TimeOnly time)
    {
        // Try parsing formats like "10 AM", "7 PM", "10:30 AM"
        string[] formats = ["h tt", "h:mm tt", "hh tt", "hh:mm tt", "H:mm"];
        return TimeOnly.TryParseExact(
            timePart,
            formats,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out time);
    }
}
