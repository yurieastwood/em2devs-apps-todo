namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A seasonal cosmetic reward (badge, avatar border, etc.).
/// Cosmetics are tied to a specific season and earned by completing quest line stages.
/// Once a season ends, unearned cosmetics become permanently locked.
/// </summary>
public sealed record CosmeticItem
{
    public string Name { get; }
    public string SeasonName { get; }
    public CosmeticRarity Rarity { get; }
    public int RequiredStage { get; }
    public bool IsSeasonExclusive { get; }

    public CosmeticItem(string name, string seasonName, CosmeticRarity rarity, int requiredStage, bool isSeasonExclusive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new Exceptions.DomainException("Cosmetic item name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(seasonName))
        {
            throw new Exceptions.DomainException("Cosmetic item season name cannot be empty.");
        }

        if (requiredStage < 1 || requiredStage > SeasonalQuestLine.MaxStages)
        {
            throw new Exceptions.DomainException(
                $"Required stage must be between 1 and {SeasonalQuestLine.MaxStages}.");
        }

        Name = name;
        SeasonName = seasonName;
        Rarity = rarity;
        RequiredStage = requiredStage;
        IsSeasonExclusive = isSeasonExclusive;
    }

    /// <summary>
    /// Determines whether this cosmetic can still be earned given the season state.
    /// </summary>
    public bool IsEarnable(Season season, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(season);
        return season.IsActive(today) && season.Name == SeasonName;
    }
}
