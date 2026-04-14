namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Maps (skill tree, tier) to the perk unlocked at that tier.
/// Tier 1 unlocks personalised Tips tailored to the tree's domain.
/// Tier 2 unlocks suggested Workflows (quest templates).
/// Tier 3 unlocks Cosmetic rewards (profile badge + themed palette).
/// </summary>
public static class SkillTreePerkCatalog
{
    /// <summary>
    /// Returns the perk unlocked at the given tier of the given tree.
    /// </summary>
    public static SkillTreePerk PerkFor(SkillTreeType tree, SkillTier tier)
    {
        ArgumentNullException.ThrowIfNull(tier);

        SkillTreePerkType type = PerkTypeForTier(tier.Value);
        string description = DescribePerk(tree, type);
        return new SkillTreePerk(tree, tier, type, description);
    }

    /// <summary>
    /// Returns all perks (tiers 1-3) defined for the given skill tree.
    /// </summary>
    public static IReadOnlyList<SkillTreePerk> AllPerksFor(SkillTreeType tree)
    {
        var perks = new List<SkillTreePerk>(SkillTier.MaxTier);
        for (int tier = 1; tier <= SkillTier.MaxTier; tier++)
        {
            perks.Add(PerkFor(tree, new SkillTier(tier)));
        }
        return perks;
    }

    private static SkillTreePerkType PerkTypeForTier(int tierValue)
    {
        return tierValue switch
        {
            1 => SkillTreePerkType.Tips,
            2 => SkillTreePerkType.Workflow,
            3 => SkillTreePerkType.Cosmetic,
            _ => throw new ArgumentOutOfRangeException(
                nameof(tierValue), tierValue, "Unknown skill tier.")
        };
    }

    private static string DescribePerk(SkillTreeType tree, SkillTreePerkType type)
    {
        string treeName = tree.ToString();
        return type switch
        {
            SkillTreePerkType.Tips => $"Personalised {treeName} tips",
            SkillTreePerkType.Workflow => $"Suggested {treeName} quest templates",
            SkillTreePerkType.Cosmetic => $"{treeName} profile badge and themed colour palette",
            _ => throw new ArgumentOutOfRangeException(
                nameof(type), type, "Unknown perk type.")
        };
    }
}
