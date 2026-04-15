using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class Notification
{
    public NotificationId Id { get; }

    /// <summary>
    /// Identifies the user the notification belongs to. Ephemeral notifications
    /// created through <see cref="Create"/> (e.g. by <c>NotificationFactory</c>
    /// pure-domain helpers) use <see cref="Guid.Empty"/>. Persisted notifications
    /// must be created via <see cref="CreateForUser"/>, which enforces a non-empty
    /// <see cref="Guid"/>.
    /// </summary>
    public Guid UserId { get; private set; }

    public NotificationType Type { get; }
    public string Message { get; }
    public NotificationStatus Status { get; private set; }
    public bool IsRead => Status == NotificationStatus.Read;
    public bool IsDismissed => Status == NotificationStatus.Dismissed;
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? ReadAt { get; private set; }
    public int? AutoDismissAfterSeconds { get; }
    public DeliveryChannel Channel { get; }
    public DeepLink? DeepLink { get; }

    private Notification(NotificationId id, Guid userId, NotificationType type, string message,
        DateTimeOffset createdAt, int? autoDismissAfterSeconds,
        DeliveryChannel channel, DeepLink? deepLink)
    {
        Id = id;
        UserId = userId;
        Type = type;
        Message = message;
        CreatedAt = createdAt;
        AutoDismissAfterSeconds = autoDismissAfterSeconds;
        Channel = channel;
        DeepLink = deepLink;
        Status = NotificationStatus.Unread;
    }

    // Stryker disable all : EF Core materialization constructor — not reachable from domain tests.
    // EF binds these parameters to mapped properties by name+type; keep it alongside the
    // "rich" ctor so the status/read_at columns can be hydrated from the database.
    private Notification(NotificationId id, Guid userId, NotificationType type, string message,
        NotificationStatus status, DateTimeOffset createdAt, DateTimeOffset? readAt)
    {
        Id = id;
        UserId = userId;
        Type = type;
        Message = message;
        Status = status;
        CreatedAt = createdAt;
        ReadAt = readAt;
        Channel = DeliveryChannel.InApp;
    }
    // Stryker restore all

    /// <summary>
    /// Creates a transient (non-user-scoped) notification — suitable for
    /// pure-domain services that do not own the persistence concern
    /// (e.g. <c>NotificationFactory</c>). The returned instance has
    /// <see cref="UserId"/> equal to <see cref="Guid.Empty"/> and is not
    /// acceptable by the repository.
    /// </summary>
    public static Notification Create(NotificationType type, string message,
        int? autoDismissAfterSeconds = null,
        DeliveryChannel channel = DeliveryChannel.InApp,
        DeepLink? deepLink = null)
    {
        ValidateInputs(message, autoDismissAfterSeconds);
        return new Notification(NotificationId.New(), Guid.Empty, type, message,
            DateTimeOffset.UtcNow, autoDismissAfterSeconds, channel, deepLink);
    }

    /// <summary>
    /// Creates a user-owned notification. Used by event handlers that generate
    /// notifications in response to application events so they can be persisted
    /// and returned by <c>GET /api/notifications</c>.
    /// </summary>
    public static Notification CreateForUser(Guid userId, NotificationType type, string message,
        int? autoDismissAfterSeconds = null,
        DeliveryChannel channel = DeliveryChannel.InApp,
        DeepLink? deepLink = null)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId cannot be empty.");
        }
        ValidateInputs(message, autoDismissAfterSeconds);
        return new Notification(NotificationId.New(), userId, type, message,
            DateTimeOffset.UtcNow, autoDismissAfterSeconds, channel, deepLink);
    }

    public void MarkAsRead()
    {
        if (Status == NotificationStatus.Dismissed)
        {
            throw new DomainException("Cannot mark a dismissed notification as read.");
        }

        if (Status == NotificationStatus.Read)
        {
            return;
        }

        Status = NotificationStatus.Read;
        ReadAt = DateTimeOffset.UtcNow;
    }

    public void Dismiss()
    {
        Status = NotificationStatus.Dismissed;
    }

    private static void ValidateInputs(string message, int? autoDismissAfterSeconds)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new DomainException("Notification message cannot be empty.");
        }

        if (autoDismissAfterSeconds.HasValue && autoDismissAfterSeconds.Value <= 0)
        {
            throw new DomainException("Auto-dismiss duration must be positive.");
        }
    }
}
