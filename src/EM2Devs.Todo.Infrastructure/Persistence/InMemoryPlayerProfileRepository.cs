using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// In-memory player profile repository for PoC.
/// Tracks mutable XP and level state across the application lifetime.
/// </summary>
public sealed class InMemoryPlayerProfileRepository : IPlayerProfileRepository
{
    private readonly object _lock = new();
    private Level _level = Level.StartingLevel();

    public Task<PlayerProfile> GetProfileAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(new PlayerProfile(
                TotalXp: _level.CurrentXp.Value,
                Level: _level.Value,
                XpToNextLevel: _level.XpToNextLevel(),
                CurrentStreak: 0,
                LongestStreak: 0));
        }
    }

    public Task AwardXpAsync(ExperiencePoints xp, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(xp);

        lock (_lock)
        {
            _level = _level.AddXp(xp);
        }

        return Task.CompletedTask;
    }
}
