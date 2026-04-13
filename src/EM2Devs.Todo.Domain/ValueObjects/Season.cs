namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A quarterly season with themed content, quest line, and date range.
/// Seasons run for a fixed period and cannot overlap.
/// </summary>
public sealed record Season
{
    public string Name { get; }
    public string Theme { get; }
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }
    public IReadOnlyList<CosmeticItem> AvailableCosmetics { get; }

    public Season(string name, string theme, DateOnly startDate, DateOnly endDate)
        : this(name, theme, startDate, endDate, [])
    {
    }

    public Season(string name, string theme, DateOnly startDate, DateOnly endDate, IReadOnlyList<CosmeticItem> availableCosmetics)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new Exceptions.DomainException("Season name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(theme))
        {
            throw new Exceptions.DomainException("Season theme cannot be empty.");
        }

        if (endDate <= startDate)
        {
            throw new Exceptions.DomainException("Season end date must be after start date.");
        }

        ArgumentNullException.ThrowIfNull(availableCosmetics);

        Name = name;
        Theme = theme;
        StartDate = startDate;
        EndDate = endDate;
        AvailableCosmetics = availableCosmetics;
    }

    public bool IsActive(DateOnly today) =>
        today >= StartDate && today <= EndDate;

    public int DaysRemaining(DateOnly today)
    {
        if (!IsActive(today))
        {
            return 0;
        }

        return EndDate.DayNumber - today.DayNumber;
    }

    public bool HasEnded(DateOnly today) =>
        today > EndDate;

    /// <summary>
    /// Validates that a new season can follow this one with no gap or overlap.
    /// The next season must start the day after this season ends.
    /// </summary>
    public bool CanTransitionTo(Season nextSeason)
    {
        ArgumentNullException.ThrowIfNull(nextSeason);
        return nextSeason.StartDate == EndDate.AddDays(1);
    }

    /// <summary>
    /// Creates the next season that starts immediately after this one ends.
    /// </summary>
    public Season TransitionTo(string name, string theme, DateOnly newEndDate, IReadOnlyList<CosmeticItem> cosmetics) =>
        new(name, theme, EndDate.AddDays(1), newEndDate, cosmetics);

    /// <summary>
    /// Returns the cosmetics that are locked (no longer earnable) after the season ends.
    /// </summary>
    public IReadOnlyList<CosmeticItem> GetLockedCosmetics(IReadOnlyList<CosmeticItem> earnedCosmetics)
    {
        ArgumentNullException.ThrowIfNull(earnedCosmetics);
        var locked = new List<CosmeticItem>();

        foreach (var cosmetic in AvailableCosmetics)
        {
            if (!WasEarned(cosmetic, earnedCosmetics))
            {
                locked.Add(cosmetic);
            }
        }

        return locked.AsReadOnly();
    }

    private static bool WasEarned(CosmeticItem cosmetic, IReadOnlyList<CosmeticItem> earnedCosmetics)
    {
        foreach (var earned in earnedCosmetics)
        {
            if (earned.Name == cosmetic.Name && earned.SeasonName == cosmetic.SeasonName)
            {
                return true;
            }
        }

        return false;
    }
}
