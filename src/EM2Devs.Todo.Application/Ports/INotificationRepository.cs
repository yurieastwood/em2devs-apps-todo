using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Ports;

/// <summary>
/// Repository for persisted in-app notifications. Every operation is scoped to the
/// current authenticated user — cross-user reads return null/empty and cross-user
/// writes throw. Saves are used for state transitions (MarkAsRead / Dismiss).
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Returns notifications belonging to the current user. Dismissed notifications
    /// are always excluded. When <paramref name="includeRead"/> is false, Read
    /// notifications are also excluded.
    /// </summary>
    Task<IReadOnlyList<Notification>> GetForCurrentUserAsync(bool includeRead, CancellationToken ct = default);

    /// <summary>
    /// Fetches a single notification by id, scoped to the current user. Returns null
    /// when the id is unknown or owned by a different user.
    /// </summary>
    Task<Notification?> GetByIdAsync(NotificationId id, CancellationToken ct = default);

    /// <summary>
    /// Adds a new notification. The <see cref="Notification.UserId"/> must match the
    /// current user; cross-user inserts throw <see cref="InvalidOperationException"/>.
    /// </summary>
    Task AddAsync(Notification notification, CancellationToken ct = default);

    /// <summary>
    /// Persists updates to an existing notification (e.g. after MarkAsRead / Dismiss).
    /// </summary>
    Task SaveAsync(Notification notification, CancellationToken ct = default);
}
