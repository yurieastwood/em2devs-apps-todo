using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Ports;

/// <summary>
/// Persistence port for the <see cref="User"/> aggregate (Phase 0 multi-user auth).
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task SaveAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Permanently removes the user row. Used after the deactivation holding period
    /// elapses to release the email/displayName back to the pool.
    /// </summary>
    Task DeleteAsync(UserId id, CancellationToken ct = default);
}
