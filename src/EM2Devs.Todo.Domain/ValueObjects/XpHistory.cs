namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Immutable collection of XP earning records with daily and weekly aggregation.
/// Maps to: experience-points.feature — "View XP history over time"
/// </summary>
public sealed record XpHistory
{
    private readonly List<XpHistoryEntry> _entries;

    public IReadOnlyList<XpHistoryEntry> Entries => _entries.AsReadOnly();

    private XpHistory(List<XpHistoryEntry> entries)
    {
        _entries = entries;
    }

    public static XpHistory Empty() => new([]);

    /// <summary>
    /// Records an XP earning event and returns a new XpHistory with the entry appended.
    /// The cumulative total is automatically computed from the previous entries.
    /// </summary>
    public XpHistory RecordXpEarning(DateOnly date, ExperiencePoints xp, string source)
    {
        ArgumentNullException.ThrowIfNull(xp);

        ExperiencePoints previousCumulative = _entries.Count > 0
            ? _entries[^1].CumulativeTotal
            : new ExperiencePoints(0);

        ExperiencePoints newCumulative = previousCumulative.Add(xp);
        var entry = new XpHistoryEntry(date, xp, source, newCumulative);

        var newEntries = new List<XpHistoryEntry>(_entries) { entry };
        return new XpHistory(newEntries);
    }

    /// <summary>
    /// Returns the total XP earned on a specific date.
    /// </summary>
    public ExperiencePoints GetDailyTotal(DateOnly date)
    {
        int total = 0;
        foreach (XpHistoryEntry entry in _entries)
        {
            if (entry.Date == date)
            {
                total += entry.XpEarned.Value;
            }
        }

        return new ExperiencePoints(total);
    }

    /// <summary>
    /// Returns the total XP earned during the 7-day week starting from the given date (inclusive).
    /// </summary>
    public ExperiencePoints GetWeeklyTotal(DateOnly weekStart)
    {
        DateOnly weekEnd = weekStart.AddDays(7);
        int total = 0;
        foreach (XpHistoryEntry entry in _entries)
        {
            if (entry.Date >= weekStart && entry.Date < weekEnd)
            {
                total += entry.XpEarned.Value;
            }
        }

        return new ExperiencePoints(total);
    }
}
