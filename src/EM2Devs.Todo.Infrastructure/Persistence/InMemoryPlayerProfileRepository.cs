using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// In-memory player profile repository for tests and the no-DB fallback.
/// Tracks XP, level, current streak, and longest streak across the application lifetime.
/// </summary>
public sealed class InMemoryPlayerProfileRepository : IPlayerProfileRepository
{
    private readonly object _lock = new();
    private Level _level = Level.StartingLevel();
    private Streak _streak = Streak.NewStreak();
    private int _longestStreak;
    private XpBreakdownReadModel? _lastBreakdown;

    public Task<PlayerProfileReadModel> GetProfileAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(new PlayerProfileReadModel(
                TotalXp: _level.CurrentXp.Value,
                Level: _level.Value,
                XpToNextLevel: _level.XpToNextLevel(),
                CurrentStreak: _streak.CurrentDays,
                LongestStreak: _longestStreak,
                LastXpBreakdown: _lastBreakdown));
        }
    }

    public Task AwardXpAsync(ExperiencePoints xp, XpBreakdownReadModel? breakdown = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(xp);

        lock (_lock)
        {
            _level = _level.AddXp(xp);
            _lastBreakdown = breakdown;
        }

        return Task.CompletedTask;
    }

    public Task RecordCompletionAsync(DateOnly today, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _streak = _streak.RecordCompletion(today);
            if (_streak.CurrentDays > _longestStreak)
            {
                _longestStreak = _streak.CurrentDays;
            }
        }

        return Task.CompletedTask;
    }

    public Task ProcessDayEndAsync(DateOnly endOfDay, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _streak = _streak.ProcessDayEnd(endOfDay);
        }

        return Task.CompletedTask;
    }
}
