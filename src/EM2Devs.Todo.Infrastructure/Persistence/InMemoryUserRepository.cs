using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Thread-safe in-memory <see cref="IUserRepository"/> for dev and test paths.
/// Seeds two demo users at construction time, hashing the shared dev password
/// via the provided <see cref="IPasswordHasher"/>.
/// </summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    /// <summary>Stable UserId for the primary demo account (matches existing seeded data).</summary>
    public static readonly UserId DemoUserId = new(new Guid("00000000-0000-0000-0000-000000000001"));

    /// <summary>Stable UserId for the secondary demo account.</summary>
    public static readonly UserId Demo2UserId = new(new Guid("00000000-0000-0000-0000-000000000002"));

    /// <summary>Shared password for both seeded demo accounts (dev only).</summary>
    public const string SeedPassword = "demo1234";

    private readonly ConcurrentDictionary<Guid, User> _store = new();

    public InMemoryUserRepository(IPasswordHasher passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(passwordHasher);

        string hash = passwordHasher.Hash(SeedPassword);
        DateTimeOffset createdAt = DateTimeOffset.UnixEpoch;

        User demo = User.Create("demo@waypoint.dev", hash, "Demo User", createdAt, DemoUserId);
        User demo2 = User.Create("demo2@waypoint.dev", hash, "Demo User 2", createdAt, Demo2UserId);

        _store[demo.Id.Value] = demo;
        _store[demo2.Id.Value] = demo2;
    }

    public Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        _store.TryGetValue(id.Value, out User? user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        User? user = _store.Values.FirstOrDefault(
            u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (!_store.TryAdd(user.Id.Value, user))
        {
            throw new InvalidOperationException($"User with id {user.Id.Value} already exists.");
        }
        return Task.CompletedTask;
    }

    public Task SaveAsync(User user, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        _store[user.Id.Value] = user;
        return Task.CompletedTask;
    }
}
