namespace EM2Devs.Todo.Domain;

/// <summary>
/// Types of events that can appear on a user's journey timeline.
/// Maps to: docs/features/reflection/journey-timeline.feature
/// </summary>
public enum TimelineEventType
{
    LevelUp,
    QuestCompleted,
    EpicCompleted,
    SagaCompleted,
    BossTaskDefeated,
    TitleEarned,
    SkillTreeUnlocked,
    SkillTreeTierAdvanced,
    StreakMilestone,
    SeasonalQuestLineCompleted,
    GuildJoined,
    GuildQuestCompleted,
    ChallengeWon,
    WeeklyReviewStreakMilestone
}
