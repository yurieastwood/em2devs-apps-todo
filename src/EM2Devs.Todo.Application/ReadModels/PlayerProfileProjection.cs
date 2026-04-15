using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.ReadModels;

/// <summary>
/// Projects a <see cref="PlayerProfile"/> aggregate into the API-facing
/// <see cref="PlayerProfileReadModel"/>, including XP history, titles,
/// and skill tree catalog.
/// </summary>
public static class PlayerProfileProjection
{
    /// <summary>The maximum number of most-recent XP history entries surfaced on the profile.</summary>
    public const int XpHistoryLimit = 20;

    public static PlayerProfileReadModel Project(
        PlayerProfile profile,
        XpBreakdownReadModel? lastBreakdown,
        IReadOnlyList<XpHistoryEntryReadModel>? xpHistoryOverride = null)
    {
        ArgumentNullException.ThrowIfNull(profile);

        IReadOnlyList<XpHistoryEntryReadModel> xpHistory = xpHistoryOverride is not null
            ? TakeLast(xpHistoryOverride, XpHistoryLimit)
            : ProjectXpHistory(profile.XpHistory);

        StreakFreezeReadModel? freeze = profile.Streak.ActiveFreeze is { } f
            ? new StreakFreezeReadModel(
                FrozenAt: f.FrozenAt,
                Days: f.Duration,
                ExpiresAt: f.FrozenAt.AddDays(f.Duration))
            : null;

        return new PlayerProfileReadModel(
            TotalXp: profile.Level.CurrentXp.Value,
            Level: profile.Level.Value,
            XpToNextLevel: profile.Level.XpToNextLevel(),
            CurrentStreak: profile.Streak.CurrentDays,
            LongestStreak: profile.LongestStreak,
            LastXpBreakdown: lastBreakdown,
            XpHistory: xpHistory,
            Titles: ProjectTitles(profile.TitleInventory),
            SkillTrees: ProjectSkillTrees(profile.SkillTrees),
            StreakFreeze: freeze);
    }

    private static IReadOnlyList<XpHistoryEntryReadModel> TakeLast(
        IReadOnlyList<XpHistoryEntryReadModel> entries, int limit)
    {
        int count = entries.Count;
        int start = Math.Max(0, count - limit);
        if (start == 0)
        {
            return entries;
        }
        var result = new List<XpHistoryEntryReadModel>(count - start);
        for (int i = start; i < count; i++)
        {
            result.Add(entries[i]);
        }
        return result;
    }

    private static List<XpHistoryEntryReadModel> ProjectXpHistory(XpHistory history)
    {
        IReadOnlyList<XpHistoryEntry> entries = history.Entries;
        int count = entries.Count;
        int start = Math.Max(0, count - XpHistoryLimit);
        var result = new List<XpHistoryEntryReadModel>(count - start);
        for (int i = start; i < count; i++)
        {
            XpHistoryEntry e = entries[i];
            result.Add(new XpHistoryEntryReadModel(
                Date: e.Date,
                XpEarned: e.XpEarned.Value,
                Source: e.Source,
                CumulativeTotal: e.CumulativeTotal.Value));
        }
        return result;
    }

    private static TitlesReadModel ProjectTitles(TitleInventory inventory)
    {
        var earned = new List<TitleReadModel>(inventory.EarnedTitles.Count);
        foreach (Title t in inventory.EarnedTitles)
        {
            earned.Add(new TitleReadModel(
                Type: t.Type.ToString(),
                DisplayName: Title.DisplayName(t.Type),
                EarnedOn: t.EarnedOn));
        }

        string? active = inventory.ActiveTitle?.ToString();

        // Title progress requires qualifying actions which aren't plumbed yet — return empty.
        // See Phase 3 plan: "return progress = [] if qualifying actions aren't easily available."
        IReadOnlyList<TitleProgressReadModel> progress = [];
        return new TitlesReadModel(earned, active, progress);
    }

    private static List<SkillTreeReadModel> ProjectSkillTrees(IReadOnlyList<SkillTree> unlocked)
    {
        SkillTreeCatalog catalog = SkillTreeCatalog.Build(unlocked);
        var result = new List<SkillTreeReadModel>(catalog.Entries.Count);
        foreach (SkillTreeCatalogEntry entry in catalog.Entries)
        {
            if (entry.IsUnlocked && entry.UnlockedTree is SkillTree tree)
            {
                var perks = new List<SkillTreePerkReadModel>(tree.CurrentTier.Value);
                for (int tier = 1; tier <= tree.CurrentTier.Value; tier++)
                {
                    SkillTreePerk p = SkillTreePerkCatalog.PerkFor(entry.Type, new SkillTier(tier));
                    perks.Add(new SkillTreePerkReadModel(
                        Tier: tier,
                        PerkType: p.Type.ToString(),
                        Description: p.Description));
                }

                result.Add(new SkillTreeReadModel(
                    Type: entry.Type.ToString(),
                    Tier: tree.CurrentTier.Value,
                    TasksCompletedInTier: tree.TasksCompletedInTier,
                    TasksToNextTier: tree.TasksToNextTier(),
                    UnlockHint: null,
                    Perks: perks));
            }
            else
            {
                result.Add(new SkillTreeReadModel(
                    Type: entry.Type.ToString(),
                    Tier: null,
                    TasksCompletedInTier: null,
                    TasksToNextTier: null,
                    UnlockHint: entry.UnlockHint,
                    Perks: []));
            }
        }
        return result;
    }
}
