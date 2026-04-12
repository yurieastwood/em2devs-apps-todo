namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Enumeration of unlockable features that become available at specific levels.
/// Maps to: docs/features/progression/levelling.feature — "Progressive feature unlocks by level"
/// </summary>
public enum UnlockableFeature
{
    Tasks,
    Quests,
    BasicXp,
    SkillTrees,
    Titles,
    DailyBrief,
    AccountabilityPartners,
    Leaderboards,
    ChallengeMode,
    InsightCards,
    AdvancedAnalytics,
}
