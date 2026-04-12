namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Immutable record of a single XP earning event with its source and running cumulative total.
/// Maps to: experience-points.feature — "View XP history over time"
/// </summary>
public sealed record XpHistoryEntry
{
    public DateOnly Date { get; }
    public ExperiencePoints XpEarned { get; }
    public string Source { get; }
    public ExperiencePoints CumulativeTotal { get; }

    public XpHistoryEntry(DateOnly date, ExperiencePoints xpEarned, string source, ExperiencePoints cumulativeTotal)
    {
        ArgumentNullException.ThrowIfNull(xpEarned);

        if (string.IsNullOrWhiteSpace(source))
        {
            throw new Exceptions.DomainException("XP history entry source cannot be empty.");
        }

        ArgumentNullException.ThrowIfNull(cumulativeTotal);

        Date = date;
        XpEarned = xpEarned;
        Source = source;
        CumulativeTotal = cumulativeTotal;
    }
}
