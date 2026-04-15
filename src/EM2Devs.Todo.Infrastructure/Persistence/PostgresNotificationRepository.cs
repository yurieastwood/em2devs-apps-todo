using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class PostgresNotificationRepository : INotificationRepository
{
    private readonly TodoDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PostgresNotificationRepository(TodoDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<Notification>> GetForCurrentUserAsync(bool includeRead, CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        IQueryable<Notification> query = _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .Where(n => n.Status != NotificationStatus.Dismissed);
        if (!includeRead)
        {
            query = query.Where(n => n.Status != NotificationStatus.Read);
        }
        return await query.ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<Notification?> GetByIdAsync(NotificationId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        Guid userId = _currentUser.UserId;
        return await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (notification.UserId != _currentUser.UserId)
        {
            throw new InvalidOperationException(
                "Notification UserId does not match the current user. Cross-user writes are forbidden.");
        }

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task SaveAsync(Notification notification, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (notification.UserId != _currentUser.UserId)
        {
            throw new InvalidOperationException(
                "Notification UserId does not match the current user. Cross-user writes are forbidden.");
        }

        if (_dbContext.Entry(notification).State == EntityState.Detached)
        {
            _dbContext.Notifications.Attach(notification);
            _dbContext.Entry(notification).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
