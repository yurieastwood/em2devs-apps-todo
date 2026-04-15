using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Shared-state backing store for <see cref="InMemoryNotificationRepository"/>.
/// Registered as a singleton so multiple scoped repository instances see the same data,
/// while the repository itself is scoped to pick up the scoped <see cref="ICurrentUser"/>.
/// </summary>
public sealed class InMemoryNotificationStore
{
    public ConcurrentDictionary<Guid, Notification> Notifications { get; } = new();
}

public sealed class InMemoryNotificationRepository : INotificationRepository
{
    private readonly InMemoryNotificationStore _store;
    private readonly ICurrentUser _currentUser;

    public InMemoryNotificationRepository(InMemoryNotificationStore store, ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(currentUser);
        _store = store;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<Notification>> GetForCurrentUserAsync(bool includeRead, CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        IReadOnlyList<Notification> items = _store.Notifications.Values
            .Where(n => n.UserId == userId)
            .Where(n => n.Status != NotificationStatus.Dismissed)
            .Where(n => includeRead || n.Status != NotificationStatus.Read)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(items);
    }

    public Task<Notification?> GetByIdAsync(NotificationId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        _store.Notifications.TryGetValue(id.Value, out Notification? notification);
        if (notification is null || notification.UserId != _currentUser.UserId)
        {
            return Task.FromResult<Notification?>(null);
        }
        return Task.FromResult<Notification?>(notification);
    }

    public Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (notification.UserId != _currentUser.UserId)
        {
            throw new InvalidOperationException(
                "Notification UserId does not match the current user. Cross-user writes are forbidden.");
        }

        _store.Notifications[notification.Id.Value] = notification;
        return Task.CompletedTask;
    }

    public Task SaveAsync(Notification notification, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (notification.UserId != _currentUser.UserId)
        {
            throw new InvalidOperationException(
                "Notification UserId does not match the current user. Cross-user writes are forbidden.");
        }

        _store.Notifications[notification.Id.Value] = notification;
        return Task.CompletedTask;
    }
}
