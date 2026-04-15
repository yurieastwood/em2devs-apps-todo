using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Ports;

/// <summary>
/// Port for pushing freshly created notifications out-of-band to connected clients
/// (e.g. via SignalR). Adapters MUST scope delivery to the supplied
/// <paramref name="userId"/> and MUST NOT throw for transient transport failures —
/// real-time delivery is best-effort; the inbox endpoint remains the source of truth.
/// </summary>
public interface INotificationPublisher
{
    Task PublishAsync(Guid userId, Notification notification, CancellationToken ct = default);
}
