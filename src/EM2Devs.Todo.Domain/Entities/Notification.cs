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

    private Notification(NotificationId id, NotificationType type, string message)
    {
        Id = id;
        Type = type;
        Message = message;
    }

    public static Notification Create(NotificationType type, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new DomainException("Notification message cannot be empty.");
        }

        return new Notification(NotificationId.New(), type, message);
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
