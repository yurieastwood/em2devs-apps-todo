using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// In-memory player profile repository for tests and the no-DB fallback.
/// Holds a single PlayerProfile aggregate instance and delegates state changes to it.
/// </summary>
public sealed class InMemoryPlayerProfileRepository : IPlayerProfileRepository
{
    private readonly object _lock = new();
    private PlayerProfile _profile = PlayerProfile.NewProfile();
    private XpBreakdownReadModel? _lastBreakdown;

    public Task<PlayerProfileReadModel> GetProfileAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(new PlayerProfileReadModel(
                TotalXp: _profile.Level.CurrentXp.Value,
                Level: _profile.Level.Value,
                XpToNextLevel: _profile.Level.XpToNextLevel(),
                CurrentStreak: _profile.Streak.CurrentDays,
                LongestStreak: _profile.LongestStreak,
                LastXpBreakdown: _lastBreakdown));
        }
    }

    public Task AwardXpAsync(ExperiencePoints xp, XpBreakdownReadModel? breakdown = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(xp);

        lock (_lock)
        {
            _profile.AwardXp(xp);
            _lastBreakdown = breakdown;
        }

        return Task.CompletedTask;
    }

    public Task RecordCompletionAsync(DateOnly completionDate, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _profile.RecordCompletion(completionDate);
        }

        return Task.CompletedTask;
    }

    public Task ProcessDayEndAsync(DateOnly evaluationDate, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _profile.ProcessDayEnd(evaluationDate);
        }

        return Task.CompletedTask;
    }
}
