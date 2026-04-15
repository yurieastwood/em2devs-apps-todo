using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Infrastructure.Notifications;

/// <summary>
/// Null-object <see cref="INotificationPublisher"/> used by hosts that do not
/// run a SignalR hub (e.g. the Worker process). Keeps the Application-layer
/// handler wiring uniform without forcing every host to serve real-time push.
/// </summary>
public sealed class NoOpNotificationPublisher : INotificationPublisher
{
    public Task PublishAsync(Guid userId, Notification notification, CancellationToken ct = default)
        => Task.CompletedTask;
}
