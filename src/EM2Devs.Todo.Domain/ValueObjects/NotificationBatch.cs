using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a batched summary of multiple notifications delivered close together.
/// Used when many notifications arrive within a short window to avoid overwhelming the user.
/// </summary>
public sealed record NotificationBatch
{
    /// <summary>
    /// Default window within which notifications are considered simultaneous.
    /// </summary>
    public static readonly TimeSpan DefaultBatchWindow = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Minimum number of notifications required before they are batched.
    /// </summary>
    public const int MinimumBatchSize = 2;

    private readonly IReadOnlyList<Notification> _notifications;

    public NotificationType Type { get; }
    public IReadOnlyList<Notification> Notifications => _notifications;
    public int Count => _notifications.Count;
    public string Summary { get; }

    private NotificationBatch(NotificationType type, IReadOnlyList<Notification> notifications, string summary)
    {
        Type = type;
        _notifications = notifications;
        Summary = summary;
    }

    /// <summary>
    /// Creates a batch when multiple notifications of the same type arrived within the window.
    /// Throws when fewer than the minimum batch size is provided.
    /// </summary>
    public static NotificationBatch Create(IReadOnlyList<Notification> notifications)
    {
        ArgumentNullException.ThrowIfNull(notifications);

        if (notifications.Count < MinimumBatchSize)
        {
            throw new DomainException($"A batch requires at least {MinimumBatchSize} notifications.");
        }

        NotificationType type = notifications[0].Type;
        foreach (Notification notification in notifications)
        {
            if (notification.Type != type)
            {
                throw new DomainException("All notifications in a batch must share the same type.");
            }
        }

        string summary = $"{notifications.Count} {type} notifications";
        return new NotificationBatch(type, notifications, summary);
    }

    /// <summary>
    /// Attempts to batch a collection of notifications that arrived within <paramref name="window"/>.
    /// Returns null when fewer than the minimum batch size fall within the window.
    /// </summary>
    public static NotificationBatch? TryCreate(IReadOnlyList<Notification> notifications, TimeSpan window)
    {
        ArgumentNullException.ThrowIfNull(notifications);

        if (notifications.Count < MinimumBatchSize)
        {
            return null;
        }

        DateTimeOffset earliest = notifications.Min(n => n.CreatedAt);
        DateTimeOffset latest = notifications.Max(n => n.CreatedAt);
        if (latest - earliest > window)
        {
            return null;
        }

        return Create(notifications);
    }
}
