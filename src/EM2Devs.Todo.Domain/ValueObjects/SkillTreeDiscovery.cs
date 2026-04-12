namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Maps task categories to skill tree types and discovery thresholds.
/// Categories are case-insensitive, multiple categories can map to the same tree.
/// </summary>
public static class SkillTreeDiscovery
{
    private static readonly Dictionary<string, SkillTreeType> _categoryMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["creative"] = SkillTreeType.Creator,
            ["health"] = SkillTreeType.Guardian,
            ["fitness"] = SkillTreeType.Guardian,
            ["learning"] = SkillTreeType.Scholar,
            ["study"] = SkillTreeType.Scholar,
            ["work"] = SkillTreeType.Architect,
            ["career"] = SkillTreeType.Architect,
            ["social"] = SkillTreeType.Connector,
            ["people"] = SkillTreeType.Connector,
            ["home"] = SkillTreeType.Steward,
            ["organising"] = SkillTreeType.Steward,
            ["side-project"] = SkillTreeType.Builder
        };

    public static bool TryGetTreeType(string category, out SkillTreeType treeType) =>
        _categoryMap.TryGetValue(category, out treeType);

    public static int DiscoveryThreshold(SkillTreeType treeType) =>
        treeType switch
        {
            SkillTreeType.Creator => 15,
            SkillTreeType.Guardian => 15,
            SkillTreeType.Scholar => 15,
            SkillTreeType.Architect => 20,
            SkillTreeType.Connector => 15,
            SkillTreeType.Steward => 15,
            SkillTreeType.Builder => 10,
            _ => throw new ArgumentOutOfRangeException(
                nameof(treeType), treeType, "Unknown skill tree type.")
        };
}
