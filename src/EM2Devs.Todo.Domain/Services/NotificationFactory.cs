using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Pure domain service that creates notifications based on task state, achievement events,
/// and user notification settings. Enforces rules around quiet hours, category toggles,
/// completed-task suppression, and overdue reminder frequency.
/// </summary>
public static class NotificationFactory
{
    private const int AchievementAutoDismissSeconds = 5;

    /// <summary>
    /// Creates a reminder notification for a task that is due today.
    /// Returns null if the task is completed/skipped, if TaskReminders are disabled,
    /// or if the notification time falls within quiet hours.
    /// </summary>
    public static Notification? CreateDueTodayReminder(
        TodoTask task,
        NotificationSettings settings,
        DateTimeOffset currentUtcTime)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(settings);

        if (IsCompletedOrSkipped(task))
        {
            return null;
        }

        if (!settings.IsCategoryEnabled(NotificationCategory.TaskReminders))
        {
            return null;
        }

        if (settings.IsInQuietHours(currentUtcTime))
        {
            return null;
        }

        return Notification.Create(
            NotificationType.TaskReminder,
            $"{task.Title.Value} is due today");
    }

    /// <summary>
    /// Creates a reminder notification for a task with an upcoming deadline.
    /// Returns null if the task is completed/skipped, if TaskReminders are disabled,
    /// or if the notification time falls within quiet hours.
    /// </summary>
    public static Notification? CreateUpcomingDeadlineReminder(
        TodoTask task,
        NotificationSettings settings,
        DateTimeOffset currentUtcTime,
        TimeSpan reminderWindow)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(settings);

        if (IsCompletedOrSkipped(task))
        {
            return null;
        }

        if (!settings.IsCategoryEnabled(NotificationCategory.TaskReminders))
        {
            return null;
        }

        if (!task.DueDate.HasValue)
        {
            return null;
        }

        TimeSpan timeUntilDue = task.DueDate.Value - currentUtcTime;
        if (timeUntilDue < TimeSpan.Zero || timeUntilDue > reminderWindow)
        {
            return null;
        }

        if (settings.IsInQuietHours(currentUtcTime))
        {
            return null;
        }

        return Notification.Create(
            NotificationType.TaskReminder,
            $"{task.Title.Value} is due soon");
    }

    /// <summary>
    /// Creates a reminder for an overdue task. Returns null if the task is completed/skipped,
    /// if TaskReminders are disabled, if a reminder was already sent today,
    /// or if the notification time falls within quiet hours.
    /// </summary>
    public static Notification? CreateOverdueReminder(
        TodoTask task,
        NotificationSettings settings,
        DateTimeOffset currentUtcTime,
        DateTimeOffset? lastReminderUtcTime)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(settings);

        if (IsCompletedOrSkipped(task))
        {
            return null;
        }

        if (!settings.IsCategoryEnabled(NotificationCategory.TaskReminders))
        {
            return null;
        }

        if (!task.DueDate.HasValue)
        {
            return null;
        }

        if (task.DueDate.Value > currentUtcTime)
        {
            return null;
        }

        // No more than one reminder per day
        if (lastReminderUtcTime.HasValue &&
            (currentUtcTime - lastReminderUtcTime.Value).TotalHours < 24)
        {
            return null;
        }

        if (settings.IsInQuietHours(currentUtcTime))
        {
            return null;
        }

        return Notification.Create(
            NotificationType.TaskReminder,
            $"{task.Title.Value} is overdue");
    }

    /// <summary>
    /// Creates a celebratory notification for an achievement.
    /// Auto-dismisses after 5 seconds. Returns null if AchievementAlerts are disabled
    /// or if the notification time falls within quiet hours.
    /// </summary>
    public static Notification? CreateAchievementNotification(
        string achievement,
        NotificationSettings settings,
        DateTimeOffset currentUtcTime)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(achievement))
        {
            throw new Exceptions.DomainException("Achievement name cannot be empty.");
        }

        if (!settings.IsCategoryEnabled(NotificationCategory.AchievementAlerts))
        {
            return null;
        }

        if (settings.IsInQuietHours(currentUtcTime))
        {
            return null;
        }

        return Notification.Create(
            NotificationType.AchievementAlert,
            $"Achievement unlocked: {achievement}!",
            AchievementAutoDismissSeconds);
    }

    private static bool IsCompletedOrSkipped(TodoTask task)
    {
        return task.Status == TaskStatus.Done || task.Status == TaskStatus.Skipped;
    }
}
