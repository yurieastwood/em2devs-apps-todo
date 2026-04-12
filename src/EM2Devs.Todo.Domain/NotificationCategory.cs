namespace EM2Devs.Todo.Domain;

/// <summary>
/// Notification categories that users can individually enable or disable.
/// Maps to the categories defined in notifications.feature.
/// </summary>
public enum NotificationCategory
{
    TaskReminders,
    AchievementAlerts,
    DailyBriefReady,
    WeeklyReviewPrompt,
    GuildActivity,
    PartnerMessages,
    InsightCards,
    CapacityWarnings,
    UpgradePrompts
}
