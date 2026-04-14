namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A single entry in the skill tree catalog: one of the seven skill trees,
/// either unlocked (with progress) or locked (with an unlock hint).
/// </summary>
public sealed record SkillTreeCatalogEntry
{
    public SkillTreeType Type { get; }
    public SkillTree? UnlockedTree { get; }
    public string? UnlockHint { get; }

    public bool IsUnlocked => UnlockedTree is not null;

    private SkillTreeCatalogEntry(SkillTreeType type, SkillTree? unlockedTree, string? unlockHint)
    {
        Type = type;
        UnlockedTree = unlockedTree;
        UnlockHint = unlockHint;
    }

    public static SkillTreeCatalogEntry Unlocked(SkillTree tree)
    {
        ArgumentNullException.ThrowIfNull(tree);
        return new SkillTreeCatalogEntry(tree.Type, tree, null);
    }

    public static SkillTreeCatalogEntry Locked(SkillTreeType type, string hint)
    {
        if (string.IsNullOrWhiteSpace(hint))
        {
            throw new Exceptions.DomainException("Unlock hint cannot be empty.");
        }

        return new SkillTreeCatalogEntry(type, null, hint);
    }
}

/// <summary>
/// The full catalog of skill trees, combining the player's unlocked trees
/// with locked silhouettes for every tree they have not yet discovered.
/// </summary>
public sealed record SkillTreeCatalog
{
    public IReadOnlyList<SkillTreeCatalogEntry> Entries { get; }

    private SkillTreeCatalog(IReadOnlyList<SkillTreeCatalogEntry> entries)
    {
        Entries = entries;
    }

    /// <summary>
    /// Builds the catalog from the player's currently unlocked skill trees.
    /// Every <see cref="SkillTreeType"/> appears exactly once: unlocked entries
    /// use the player's actual progress; missing types become locked silhouettes
    /// with a hint describing how to unlock them.
    /// </summary>
    public static SkillTreeCatalog Build(IReadOnlyList<SkillTree> unlockedTrees)
    {
        ArgumentNullException.ThrowIfNull(unlockedTrees);

        var entries = new List<SkillTreeCatalogEntry>();
        foreach (SkillTreeType type in Enum.GetValues<SkillTreeType>())
        {
            SkillTree? match = unlockedTrees.FirstOrDefault(t => t.Type == type);
            if (match is not null)
            {
                entries.Add(SkillTreeCatalogEntry.Unlocked(match));
            }
            else
            {
                entries.Add(SkillTreeCatalogEntry.Locked(type, UnlockHintFor(type)));
            }
        }
        return new SkillTreeCatalog(entries);
    }

    private static string UnlockHintFor(SkillTreeType type)
    {
        int threshold = SkillTreeDiscovery.DiscoveryThreshold(type);
        string category = PrimaryCategoryFor(type);
        return $"Complete {threshold} {category} tasks to unlock";
    }

    private static string PrimaryCategoryFor(SkillTreeType type) =>
        type switch
        {
            SkillTreeType.Creator => "creative",
            SkillTreeType.Guardian => "health or fitness",
            SkillTreeType.Scholar => "learning or study",
            SkillTreeType.Architect => "work or career",
            SkillTreeType.Connector => "social",
            SkillTreeType.Steward => "home or organising",
            SkillTreeType.Builder => "side-project",
            _ => throw new ArgumentOutOfRangeException(
                nameof(type), type, "Unknown skill tree type.")
        };
}
