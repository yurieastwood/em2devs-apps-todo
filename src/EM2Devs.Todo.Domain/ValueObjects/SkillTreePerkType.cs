namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// The type of reward unlocked by a skill tree tier.
/// Tier 1 unlocks personalised Tips, Tier 2 unlocks suggested Workflows,
/// Tier 3 unlocks Cosmetic rewards.
/// </summary>
public enum SkillTreePerkType
{
    Tips,
    Workflow,
    Cosmetic
}
