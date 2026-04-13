namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A user's profile for a specific season, tracking seasonal XP, rank, cosmetics earned,
/// quest line progress, and completion status.
/// Maps to: docs/features/progression/seasons.feature
/// </summary>
public sealed record SeasonalProfile
{
    public string SeasonName { get; }
    public ExperiencePoints SeasonalXp { get; init; }
    public int? FinalRank { get; init; }
    public IReadOnlyList<CosmeticItem> EarnedCosmetics { get; init; }
    public SeasonalQuestLine QuestLine { get; init; }
    public bool IsComplete { get; init; }
    public string? ActiveBadge { get; init; }

    public SeasonalProfile(
        string seasonName,
        ExperiencePoints seasonalXp,
        int? finalRank,
        IReadOnlyList<CosmeticItem> earnedCosmetics,
        SeasonalQuestLine questLine,
        bool isComplete = false,
        string? activeBadge = null)
    {
        if (string.IsNullOrWhiteSpace(seasonName))
        {
            throw new Exceptions.DomainException("Season name cannot be empty.");
        }

        ArgumentNullException.ThrowIfNull(seasonalXp);
        ArgumentNullException.ThrowIfNull(earnedCosmetics);
        ArgumentNullException.ThrowIfNull(questLine);

        if (finalRank.HasValue && finalRank.Value < 1)
        {
            throw new Exceptions.DomainException("Final rank must be at least 1.");
        }

        SeasonName = seasonName;
        SeasonalXp = seasonalXp;
        FinalRank = finalRank;
        EarnedCosmetics = earnedCosmetics;
        QuestLine = questLine;
        IsComplete = isComplete;
        ActiveBadge = activeBadge;
    }

    /// <summary>
    /// Creates a new seasonal profile when joining a season.
    /// </summary>
    public static SeasonalProfile StartNew(string seasonName, int questLineStages) =>
        new(
            seasonName,
            new ExperiencePoints(0),
            null,
            [],
            SeasonalQuestLine.Start(questLineStages));

    /// <summary>
    /// Adds seasonal XP earned from quest line progression.
    /// </summary>
    public SeasonalProfile AddSeasonalXp(ExperiencePoints xp) =>
        this with { SeasonalXp = SeasonalXp.Add(xp) };

    /// <summary>
    /// Records a cosmetic item earned by completing a quest line stage.
    /// </summary>
    public SeasonalProfile EarnCosmetic(CosmeticItem cosmetic)
    {
        ArgumentNullException.ThrowIfNull(cosmetic);
        var newCosmetics = new List<CosmeticItem>(EarnedCosmetics) { cosmetic };
        return this with { EarnedCosmetics = newCosmetics.AsReadOnly() };
    }

    /// <summary>
    /// Records the final rank when the season ends.
    /// </summary>
    public SeasonalProfile RecordFinalRank(int rank)
    {
        if (rank < 1)
        {
            throw new Exceptions.DomainException("Final rank must be at least 1.");
        }

        return this with { FinalRank = rank, IsComplete = true };
    }

    /// <summary>
    /// Advances the quest line and optionally earns a cosmetic for completing a stage.
    /// </summary>
    public SeasonalProfile CompleteQuestStage(
        int tasksRequired,
        ExperiencePoints stageXp,
        CosmeticItem? stageCosmetic = null)
    {
        ArgumentNullException.ThrowIfNull(stageXp);
        var updatedQuestLine = QuestLine.RecordTaskCompletion(tasksRequired);

        bool stageAdvanced = updatedQuestLine.CurrentStage > QuestLine.CurrentStage;
        var result = this with { QuestLine = updatedQuestLine };

        if (stageAdvanced)
        {
            result = result.AddSeasonalXp(stageXp);

            if (stageCosmetic is not null)
            {
                result = result.EarnCosmetic(stageCosmetic);
            }
        }

        return result;
    }

    /// <summary>
    /// Marks the quest line as complete with a completion bonus.
    /// </summary>
    public SeasonalProfile CompleteFullQuestLine(
        ExperiencePoints completionBonus,
        CosmeticItem completionCosmetic)
    {
        ArgumentNullException.ThrowIfNull(completionCosmetic);
        var newCosmetics = new List<CosmeticItem>(EarnedCosmetics) { completionCosmetic };
        return this with
        {
            SeasonalXp = SeasonalXp.Add(completionBonus),
            EarnedCosmetics = newCosmetics.AsReadOnly()
        };
    }

    /// <summary>
    /// Sets the active badge displayed on the user's profile.
    /// The badge must be one of the earned cosmetics.
    /// </summary>
    public SeasonalProfile SetActiveBadge(string badgeName)
    {
        if (string.IsNullOrWhiteSpace(badgeName))
        {
            throw new Exceptions.DomainException("Badge name cannot be empty.");
        }

        if (!HasEarnedCosmetic(badgeName))
        {
            throw new Exceptions.DomainException("Cannot set a badge that has not been earned.");
        }

        return this with { ActiveBadge = badgeName };
    }

    private bool HasEarnedCosmetic(string name)
    {
        foreach (var cosmetic in EarnedCosmetics)
        {
            if (cosmetic.Name == name)
            {
                return true;
            }
        }

        return false;
    }
}
