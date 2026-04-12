using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class Notification
{
    public NotificationId Id { get; }
    public NotificationType Type { get; }
    public string Message { get; }
    public bool IsRead { get; private set; }
    public bool IsDismissed { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public int? AutoDismissAfterSeconds { get; }

    private Notification(NotificationId id, NotificationType type, string message,
        DateTimeOffset createdAt, int? autoDismissAfterSeconds)
    {
        Id = id;
        Type = type;
        Message = message;
        CreatedAt = createdAt;
        AutoDismissAfterSeconds = autoDismissAfterSeconds;
    }

    public static Notification Create(NotificationType type, string message,
        int? autoDismissAfterSeconds = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new DomainException("Notification message cannot be empty.");
        }

        if (autoDismissAfterSeconds.HasValue && autoDismissAfterSeconds.Value <= 0)
        {
            throw new DomainException("Auto-dismiss duration must be positive.");
        }

        return new Notification(NotificationId.New(), type, message,
            DateTimeOffset.UtcNow, autoDismissAfterSeconds);
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }

    public void Dismiss()
    {
        IsDismissed = true;
    }
}
