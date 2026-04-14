using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A single perk unlocked at a specific tier of a skill tree.
/// Tier 1 -> Tips, Tier 2 -> Workflow, Tier 3 -> Cosmetic.
/// </summary>
public sealed record SkillTreePerk
{
    public SkillTreeType Tree { get; }
    public SkillTier Tier { get; }
    public SkillTreePerkType Type { get; }
    public string Description { get; }

    public SkillTreePerk(SkillTreeType tree, SkillTier tier, SkillTreePerkType type, string description)
    {
        ArgumentNullException.ThrowIfNull(tier);

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Perk description cannot be empty.");
        }

        Tree = tree;
        Tier = tier;
        Type = type;
        Description = description;
    }
}
