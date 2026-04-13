namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Pure domain service that checks feature access by subscription tier.
/// Determines which features are available at each tier level.
/// </summary>
public static class PremiumFeatureGate
{
    /// <summary>
    /// Features available to all free-tier users.
    /// </summary>
    public static readonly IReadOnlyList<string> FreeFeatures =
    [
        "Unlimited tasks",
        "Unlimited quests",
        "Unlimited epics",
        "Full XP and levelling engine",
        "Skill trees",
        "Titles and ranks",
        "Basic daily brief",
        "Energy-aware scheduling",
        "One accountability partner",
        "Basic weekly review",
        "Journey timeline",
        "Local data storage",
        "Manual data export"
    ];

    /// <summary>
    /// Additional features available to Pro-tier users (beyond free features).
    /// </summary>
    public static readonly IReadOnlyList<string> ProFeatures =
    [
        "Sagas and long-arc goal tracking",
        "Capacity modelling",
        "Time estimation learning",
        "Insight cards",
        "Guilds (create and join up to 5)",
        "Challenge mode",
        "Seasonal leaderboards",
        "Cross-device sync",
        "Priority themes and cosmetics",
        "Advanced weekly review",
        "Annual Wrapped",
        "Calendar integration"
    ];

    /// <summary>
    /// Additional features available to Team-tier users (beyond Pro features).
    /// </summary>
    public static readonly IReadOnlyList<string> TeamFeatures =
    [
        "Everything in Pro",
        "Shared quest boards with roles",
        "Team analytics and velocity tracking",
        "Admin controls and onboarding flows",
        "Dedicated team leaderboards"
    ];

    /// <summary>
    /// Returns all features accessible at the given tier.
    /// </summary>
    public static IReadOnlyList<string> GetAccessibleFeatures(SubscriptionTier tier)
    {
        return tier switch
        {
            SubscriptionTier.Free => FreeFeatures,
            SubscriptionTier.Pro => [.. FreeFeatures, .. ProFeatures],
            SubscriptionTier.Team => [.. FreeFeatures, .. ProFeatures, .. TeamFeatures],
            _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown subscription tier.")
        };
    }

    /// <summary>
    /// Checks whether a specific feature is accessible at the given tier.
    /// </summary>
    public static bool HasAccess(SubscriptionTier tier, string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return false;
        }

        IReadOnlyList<string> accessible = GetAccessibleFeatures(tier);
        return accessible.Contains(featureName);
    }

    /// <summary>
    /// Checks whether a feature is premium-only (not available in the free tier).
    /// </summary>
    public static bool IsPremiumFeature(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return false;
        }

        return ProFeatures.Contains(featureName) || TeamFeatures.Contains(featureName);
    }

    /// <summary>
    /// Returns the minimum tier required to access a feature.
    /// </summary>
    public static SubscriptionTier GetRequiredTier(string featureName)
    {
        ArgumentNullException.ThrowIfNull(featureName);

        if (FreeFeatures.Contains(featureName))
        {
            return SubscriptionTier.Free;
        }

        if (ProFeatures.Contains(featureName))
        {
            return SubscriptionTier.Pro;
        }

        if (TeamFeatures.Contains(featureName))
        {
            return SubscriptionTier.Team;
        }

        throw new Exceptions.DomainException($"Unknown feature: {featureName}.");
    }
}
