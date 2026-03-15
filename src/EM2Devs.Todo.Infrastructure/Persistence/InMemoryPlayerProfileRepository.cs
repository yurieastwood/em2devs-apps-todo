using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// In-memory player profile repository for PoC.
/// Returns a default profile until real persistence is wired.
/// </summary>
public sealed class InMemoryPlayerProfileRepository : IPlayerProfileRepository
{
    public Task<PlayerProfile> GetProfileAsync(CancellationToken ct = default) =>
        Task.FromResult(new PlayerProfile(
            TotalXp: 0,
            Level: 1,
            XpToNextLevel: 50,
            CurrentStreak: 0,
            LongestStreak: 0));
}
