using EM2Devs.Todo.Application.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EM2Devs.Todo.Api.Hubs;

/// <summary>
/// SignalR hub that streams freshly created notifications to the authenticated
/// user's browser. On connect, the caller is added to a group keyed by their
/// user id so <see cref="INotificationPublisher"/> can target per-user fan-out.
/// The hub itself exposes no invokable server methods — it is a push-only channel.
/// </summary>
[Authorize]
public sealed class NotificationsHub : Hub
{
    private readonly ICurrentUser _currentUser;

    public NotificationsHub(ICurrentUser currentUser) => _currentUser = currentUser;

    public override async Task OnConnectedAsync()
    {
        if (_currentUser.IsAuthenticated && _currentUser.UserId != Guid.Empty)
        {
            await Groups
                .AddToGroupAsync(Context.ConnectionId, _currentUser.UserId.ToString())
                .ConfigureAwait(false);
        }

        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_currentUser.IsAuthenticated && _currentUser.UserId != Guid.Empty)
        {
            await Groups
                .RemoveFromGroupAsync(Context.ConnectionId, _currentUser.UserId.ToString())
                .ConfigureAwait(false);
        }

        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }
}
