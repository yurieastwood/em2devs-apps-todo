using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Shared-state backing store for <see cref="InMemoryPlayerProfileRepository"/>.
/// Registered as a singleton so multiple scoped repository instances see the same data,
/// while the repository itself is scoped to pick up the scoped <see cref="ICurrentUser"/>.
/// </summary>
public sealed class InMemoryPlayerProfileStore
{
    public ConcurrentDictionary<Guid, PlayerProfile> Profiles { get; } = new();
}

/// <summary>
/// In-memory player profile repository for tests and the no-DB fallback.
/// Slice 3 multi-user isolation: one profile per UserId, keyed by <see cref="ICurrentUser.UserId"/>.
/// </summary>
public sealed class InMemoryPlayerProfileRepository : IPlayerProfileRepository
{
    private readonly object _lock = new();
    private readonly InMemoryPlayerProfileStore _store;
    private readonly ILastXpBreakdownCache _breakdownCache;
    private readonly ICurrentUser _currentUser;

    public InMemoryPlayerProfileRepository(
        InMemoryPlayerProfileStore store,
        ILastXpBreakdownCache breakdownCache,
        ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(currentUser);
        _store = store;
        _breakdownCache = breakdownCache;
        _currentUser = currentUser;
    }

    private PlayerProfile GetOrCreate() =>
        _store.Profiles.GetOrAdd(_currentUser.UserId, uid => PlayerProfile.NewProfile(uid));

    public Task<PlayerProfileReadModel> GetProfileAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            PlayerProfile profile = GetOrCreate();
            return Task.FromResult(PlayerProfileProjection.Project(profile, _breakdownCache.GetCurrent()));
        }
    }

    public Task AwardXpAsync(ExperiencePoints xp, XpBreakdownReadModel? breakdown = null, DateOnly? historyDate = null, string? historySource = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(xp);

        lock (_lock)
        {
            PlayerProfile profile = GetOrCreate();
            profile.AwardXp(xp);
            _breakdownCache.SetCurrent(breakdown);

            if (historyDate is not null && !string.IsNullOrWhiteSpace(historySource))
            {
                profile.RecordXpEarning(historyDate.Value, xp, historySource);
            }
        }

        return Task.CompletedTask;
    }

    public Task RecordCompletionAsync(DateOnly completionDate, CancellationToken ct = default)
    {
        lock (_lock)
        {
            PlayerProfile profile = GetOrCreate();
            profile.RecordCompletion(completionDate);
        }

        return Task.CompletedTask;
    }

    public Task ProcessDayEndAsync(DateOnly evaluationDate, CancellationToken ct = default)
    {
        lock (_lock)
        {
            PlayerProfile profile = GetOrCreate();
            profile.ProcessDayEnd(evaluationDate);
        }

        return Task.CompletedTask;
    }

    public Task FreezeStreakAsync(DateOnly today, int days, CancellationToken ct = default)
    {
        lock (_lock)
        {
            PlayerProfile profile = GetOrCreate();
            profile.FreezeStreak(today, days);
        }

        return Task.CompletedTask;
    }

    public Task StartFocusModeAsync(TaskId taskId, DateTimeOffset startedAt, CancellationToken ct = default)
    {
        lock (_lock)
        {
            PlayerProfile profile = GetOrCreate();
            profile.StartFocusMode(taskId, startedAt);
        }

        return Task.CompletedTask;
    }

    public Task<FocusMode> EndFocusModeAsync(DateTimeOffset endedAt, CancellationToken ct = default)
    {
        lock (_lock)
        {
            PlayerProfile profile = GetOrCreate();
            FocusMode ended = profile.EndFocusMode(endedAt);
            return Task.FromResult(ended);
        }
    }

    public Task DiscoverSkillTreeAsync(SkillTreeType type, CancellationToken ct = default)
    {
        lock (_lock)
        {
            PlayerProfile profile = GetOrCreate();
            profile.DiscoverSkillTree(type);
        }

        return Task.CompletedTask;
    }

    public Task AwardTitleAsync(Title title, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(title);
        lock (_lock)
        {
            PlayerProfile profile = GetOrCreate();
            profile.AwardTitle(title);
        }

        return Task.CompletedTask;
    }

    public Task ImportAsync(PlayerProfile profile, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        lock (_lock)
        {
            _store.Profiles[_currentUser.UserId] = profile;
            _breakdownCache.SetCurrent(null);
        }

        return Task.CompletedTask;
    }
}
