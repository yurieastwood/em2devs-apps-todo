using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace EM2Devs.Todo.Api.Hubs;

/// <summary>
/// <see cref="INotificationPublisher"/> adapter that fans out new notifications over
/// the <see cref="NotificationsHub"/> to the target user's group. The payload shape
/// mirrors the REST inbox response so the frontend can merge without translation.
/// </summary>
public sealed class SignalRNotificationPublisher : INotificationPublisher
{
    internal const string EventName = "notificationCreated";

    private readonly IHubContext<NotificationsHub> _hub;

    public SignalRNotificationPublisher(IHubContext<NotificationsHub> hub) => _hub = hub;

    public Task PublishAsync(Guid userId, Notification notification, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var payload = new
        {
            id = notification.Id.Value,
            type = notification.Type.ToString(),
            message = notification.Message,
            createdAt = notification.CreatedAt,
            status = notification.Status.ToString(),
            readAt = notification.ReadAt
        };

        return _hub.Clients
            .Group(userId.ToString())
            .SendAsync(EventName, payload, ct);
    }
}
