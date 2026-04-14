using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Postgres-backed <see cref="IUserRepository"/> using EF Core via <see cref="TodoDbContext"/>.
/// Mirrors the change-tracking pattern used by <see cref="PostgresTaskRepository"/>.
/// </summary>
public sealed class PostgresUserRepository : IUserRepository
{
    private readonly TodoDbContext _dbContext;

    public PostgresUserRepository(TodoDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return await _dbContext.Users.FindAsync([id], ct).ConfigureAwait(false);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, email), ct)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        await _dbContext.Users.AddAsync(user, ct).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task SaveAsync(User user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (_dbContext.Entry(user).State == EntityState.Detached)
        {
            _dbContext.Users.Attach(user);
            _dbContext.Entry(user).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
