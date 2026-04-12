namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Pure domain service that maps levels to unlockable features.
/// Maps to: docs/features/progression/levelling.feature — "Progressive feature unlocks by level"
/// </summary>
public static class FeatureUnlockRegistry
{
    private static readonly Dictionary<int, IReadOnlyList<UnlockableFeature>> _unlocksByLevel =
        new()
        {
            { 1, [UnlockableFeature.Tasks, UnlockableFeature.Quests, UnlockableFeature.BasicXp] },
            { 3, [UnlockableFeature.SkillTrees] },
            { 5, [UnlockableFeature.Titles, UnlockableFeature.DailyBrief] },
            { 7, [UnlockableFeature.AccountabilityPartners] },
            { 10, [UnlockableFeature.Leaderboards, UnlockableFeature.ChallengeMode] },
            { 15, [UnlockableFeature.InsightCards] },
            { 20, [UnlockableFeature.AdvancedAnalytics] },
        };

    /// <summary>
    /// Returns all features unlocked at or below the given level.
    /// </summary>
    public static IReadOnlyList<UnlockableFeature> GetUnlockedFeatures(int level)
    {
        var result = new List<UnlockableFeature>();
        foreach (var (unlockLevel, features) in _unlocksByLevel)
        {
            if (unlockLevel <= level)
            {
                result.AddRange(features);
            }
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// Returns only the features newly unlocked at the given level (not earlier).
    /// </summary>
    public static IReadOnlyList<UnlockableFeature> GetNewlyUnlockedFeatures(int level)
    {
        return _unlocksByLevel.TryGetValue(level, out var features)
            ? features
            : Array.Empty<UnlockableFeature>();
    }
}
