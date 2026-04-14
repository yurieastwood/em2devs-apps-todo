using System.Globalization;
using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Parses natural-language date expressions into a concrete <see cref="DateOnly"/>
/// relative to a reference "today". Supports:
/// "today", "tomorrow", "next &lt;weekday&gt;", "&lt;weekday&gt;", "in N days",
/// and full weekday names (case-insensitive).
/// </summary>
public static class NaturalDateParser
{
    /// <summary>
    /// Parses the natural-language expression. Throws <see cref="DomainException"/>
    /// when the input is null, empty, or unrecognised.
    /// </summary>
    public static DateOnly Parse(string expression, DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new DomainException("Date expression cannot be empty.");
        }

        string normalised = expression.Trim().ToLowerInvariant();

        if (normalised == "today")
        {
            return today;
        }

        if (normalised == "tomorrow")
        {
            return today.AddDays(1);
        }

        if (normalised.StartsWith("next ", StringComparison.Ordinal))
        {
            string weekdayPart = normalised.Substring(5).Trim();
            if (TryParseWeekday(weekdayPart, out DayOfWeek nextDay))
            {
                return NextOccurrenceOf(today, nextDay, alwaysAdvance: true);
            }

            throw new DomainException($"Unrecognised weekday after 'next': '{weekdayPart}'.");
        }

        if (normalised.StartsWith("in ", StringComparison.Ordinal))
        {
            string remainder = normalised.Substring(3).Trim();
            string[] parts = remainder.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2
                && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int days)
                && days >= 0
                && (parts[1] == "day" || parts[1] == "days"))
            {
                return today.AddDays(days);
            }

            throw new DomainException($"Unrecognised 'in N days' expression: '{expression}'.");
        }

        if (TryParseWeekday(normalised, out DayOfWeek weekday))
        {
            return NextOccurrenceOf(today, weekday, alwaysAdvance: false);
        }

        throw new DomainException($"Unrecognised date expression: '{expression}'.");
    }

    /// <summary>
    /// Attempts to parse. Returns false instead of throwing on failure.
    /// </summary>
    public static bool TryParse(string expression, DateOnly today, out DateOnly result)
    {
        try
        {
            result = Parse(expression, today);
            return true;
        }
        catch (DomainException)
        {
            result = default;
            return false;
        }
    }

    private static bool TryParseWeekday(string value, out DayOfWeek day)
    {
        return Enum.TryParse(value, ignoreCase: true, out day)
            && Enum.IsDefined(day);
    }

    private static DateOnly NextOccurrenceOf(DateOnly today, DayOfWeek targetDay, bool alwaysAdvance)
    {
        int delta = ((int)targetDay - (int)today.DayOfWeek + 7) % 7;
        if (delta == 0 && alwaysAdvance)
        {
            delta = 7;
        }
        return today.AddDays(delta);
    }
}
