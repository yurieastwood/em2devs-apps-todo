namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Defines the engagement-based thresholds for progressive feature disclosure.
/// Unlike FeatureUnlockRegistry (level-based), these thresholds are based on
/// concrete engagement metrics: tasks created, tasks completed, level reached,
/// and quests completed.
/// Maps to: docs/features/onboarding/progressive-disclosure.feature
/// — "Features unlock at specific engagement thresholds"
/// </summary>
public sealed record EngagementThreshold
{
    public string ThresholdKey { get; }
    public UnlockableFeature Feature { get; }

    public EngagementThreshold(string thresholdKey, UnlockableFeature feature)
    {
        if (string.IsNullOrWhiteSpace(thresholdKey))
        {
            throw new Exceptions.DomainException("Threshold key cannot be empty.");
        }

        ThresholdKey = thresholdKey;
        Feature = feature;
    }
}

/// <summary>
/// Pure domain service that maps engagement metrics to feature unlocks.
/// Thresholds are based on the progressive-disclosure.feature scenario outline.
/// </summary>
public static class EngagementUnlockRegistry
{
    public const int QuestsTasksCreatedThreshold = 5;
    public const int XpTasksCompletedThreshold = 10;
    public const int SkillTreesLevelThreshold = 3;
    public const int TitlesLevelThreshold = 5;
    public const int EpicsQuestsCompletedThreshold = 3;
    public const int AccountabilityPartnerLevelThreshold = 7;

    /// <summary>
    /// Evaluates which features should be unlocked based on current engagement metrics.
    /// </summary>
    public static IReadOnlyList<UnlockableFeature> EvaluateUnlocks(
        int tasksCreated,
        int tasksCompleted,
        int currentLevel,
        int questsCompleted)
    {
        var unlocked = new List<UnlockableFeature>();

        if (tasksCreated >= QuestsTasksCreatedThreshold)
        {
            unlocked.Add(UnlockableFeature.Quests);
        }

        if (tasksCompleted >= XpTasksCompletedThreshold)
        {
            unlocked.Add(UnlockableFeature.BasicXp);
        }

        if (currentLevel >= SkillTreesLevelThreshold)
        {
            unlocked.Add(UnlockableFeature.SkillTrees);
        }

        if (currentLevel >= TitlesLevelThreshold)
        {
            unlocked.Add(UnlockableFeature.Titles);
        }

        if (questsCompleted >= EpicsQuestsCompletedThreshold)
        {
            unlocked.Add(UnlockableFeature.DailyBrief);
        }

        if (currentLevel >= AccountabilityPartnerLevelThreshold)
        {
            unlocked.Add(UnlockableFeature.AccountabilityPartners);
        }

        return unlocked.AsReadOnly();
    }

    /// <summary>
    /// Returns a human-readable description of the threshold for a given feature.
    /// </summary>
    public static string GetThresholdDescription(UnlockableFeature feature)
    {
        return feature switch
        {
            UnlockableFeature.Quests => $"{QuestsTasksCreatedThreshold} tasks created",
            UnlockableFeature.BasicXp => $"{XpTasksCompletedThreshold} tasks completed",
            UnlockableFeature.SkillTrees => $"Level {SkillTreesLevelThreshold}",
            UnlockableFeature.Titles => $"Level {TitlesLevelThreshold}",
            UnlockableFeature.DailyBrief => $"{EpicsQuestsCompletedThreshold} quests completed",
            UnlockableFeature.AccountabilityPartners => $"Level {AccountabilityPartnerLevelThreshold}",
            _ => throw new ArgumentOutOfRangeException(
                nameof(feature), feature, "No engagement threshold defined for this feature.")
        };
    }
}
