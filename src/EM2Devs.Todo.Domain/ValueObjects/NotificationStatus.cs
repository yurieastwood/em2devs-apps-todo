namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Lifecycle state of a persisted in-app <c>Notification</c> — the inbox groups rows by it.
/// </summary>
public enum NotificationStatus
{
    Unread,
    Read,
    Dismissed,
}
