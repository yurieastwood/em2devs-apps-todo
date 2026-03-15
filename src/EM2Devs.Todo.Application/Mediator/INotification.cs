namespace EM2Devs.Todo.Application.Mediator;

/// <summary>
/// Marker interface for a domain event notification.
/// Published after state changes; fans out to multiple handlers (ADR-010).
/// </summary>
public interface INotification;

/// <summary>
/// Handles a notification (domain event).
/// Multiple handlers can subscribe to the same notification type.
/// </summary>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken ct);
}
