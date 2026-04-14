using System.Globalization;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Result of parsing a quick-add input string.
/// </summary>
public sealed record QuickAddResult
{
    public TaskTitle Title { get; }
    public IReadOnlyList<Tag> Tags { get; }
    public TaskPriority? Priority { get; }
    public DateOnly? DueDate { get; }

    public QuickAddResult(TaskTitle title, IReadOnlyList<Tag> tags, TaskPriority? priority, DateOnly? dueDate)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(tags);

        Title = title;
        Tags = tags;
        Priority = priority;
        DueDate = dueDate;
    }
}

/// <summary>
/// Parses a raw quick-add string containing inline directives:
/// <c>#tag</c> adds a tag, <c>!priority</c> sets priority (low/medium/high/critical),
/// <c>^date</c> sets the due date (either a natural-language expression like "tomorrow",
/// a weekday, or a month/day like "April 15"). The remaining text becomes the task title.
/// </summary>
public static class QuickAddParser
{
    public static QuickAddResult Parse(string input, DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new DomainException("Quick-add input cannot be empty.");
        }

        string[] tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var titleTokens = new List<string>();
        var tags = new List<Tag>();
        TaskPriority? priority = null;
        DateOnly? dueDate = null;
        var dateTokens = new List<string>();
        bool collectingDate = false;

        foreach (string token in tokens)
        {
            if (collectingDate)
            {
                if (IsDirective(token))
                {
                    FinaliseDate(dateTokens, today, ref dueDate);
                    dateTokens.Clear();
                    collectingDate = false;
                    // fall through to process this directive token
                }
                else
                {
                    dateTokens.Add(token);
                    continue;
                }
            }

            if (token.StartsWith('#') && token.Length > 1)
            {
                tags.Add(Tag.From(token.Substring(1)));
            }
            else if (token.StartsWith('!') && token.Length > 1)
            {
                priority = ParsePriority(token.Substring(1));
            }
            else if (token.StartsWith('^') && token.Length > 1)
            {
                dateTokens.Add(token.Substring(1));
                collectingDate = true;
            }
            else
            {
                titleTokens.Add(token);
            }
        }

        if (collectingDate)
        {
            FinaliseDate(dateTokens, today, ref dueDate);
        }

        if (titleTokens.Count == 0)
        {
            throw new DomainException("Quick-add input must contain a title.");
        }

        var title = new TaskTitle(string.Join(' ', titleTokens));
        return new QuickAddResult(title, tags, priority, dueDate);
    }

    private static bool IsDirective(string token) =>
        token.Length > 1 && (token[0] == '#' || token[0] == '!' || token[0] == '^');

    private static TaskPriority ParsePriority(string value)
    {
        if (!Enum.TryParse(value, ignoreCase: true, out TaskPriority priority)
            || !Enum.IsDefined(priority))
        {
            throw new DomainException($"Unrecognised priority: '{value}'.");
        }

        return priority;
    }

    private static void FinaliseDate(List<string> tokens, DateOnly today, ref DateOnly? dueDate)
    {
        string expression = string.Join(' ', tokens);
        if (TryParseMonthDay(expression, today, out DateOnly monthDay))
        {
            dueDate = monthDay;
            return;
        }

        dueDate = NaturalDateParser.Parse(expression, today);
    }

    private static bool TryParseMonthDay(string expression, DateOnly today, out DateOnly result)
    {
        string[] parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2
            && DateTime.TryParseExact(
                $"{parts[0]} {parts[1]}",
                ["MMMM d", "MMM d"],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsed))
        {
            var candidate = new DateOnly(today.Year, parsed.Month, parsed.Day);
            if (candidate < today)
            {
                candidate = candidate.AddYears(1);
            }
            result = candidate;
            return true;
        }

        result = default;
        return false;
    }
}
