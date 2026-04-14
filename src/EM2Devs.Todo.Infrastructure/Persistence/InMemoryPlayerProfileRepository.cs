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
    private readonly ILastXpBreakdownCache _breakdownCache;
    private PlayerProfile _profile = PlayerProfile.NewProfile();

    public InMemoryPlayerProfileRepository(ILastXpBreakdownCache breakdownCache)
    {
        _breakdownCache = breakdownCache;
    }

    public Task<PlayerProfileReadModel> GetProfileAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(PlayerProfileProjection.Project(_profile, _breakdownCache.GetCurrent()));
        }
    }

    public Task AwardXpAsync(ExperiencePoints xp, XpBreakdownReadModel? breakdown = null, DateOnly? historyDate = null, string? historySource = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(xp);

        lock (_lock)
        {
            _profile.AwardXp(xp);
            _breakdownCache.SetCurrent(breakdown);

            if (historyDate is not null && !string.IsNullOrWhiteSpace(historySource))
            {
                _profile.RecordXpEarning(historyDate.Value, xp, historySource);
            }
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
