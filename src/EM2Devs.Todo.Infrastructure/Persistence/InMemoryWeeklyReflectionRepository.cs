using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Shared-state backing store for <see cref="InMemoryWeeklyReflectionRepository"/>.
/// Registered as a singleton so multiple scoped repository instances see the same data,
/// while the repository itself is scoped to pick up the scoped <see cref="ICurrentUser"/>.
/// </summary>
public sealed class InMemoryWeeklyReflectionStore
{
    public ConcurrentDictionary<(Guid UserId, DateOnly WeekOf), WeeklyReflectionReadModel> Reflections { get; } = new();
}

public sealed class InMemoryWeeklyReflectionRepository : IWeeklyReflectionRepository
{
    private readonly InMemoryWeeklyReflectionStore _store;
    private readonly ICurrentUser _currentUser;

    public InMemoryWeeklyReflectionRepository(
        InMemoryWeeklyReflectionStore store,
        ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(currentUser);
        _store = store;
        _currentUser = currentUser;
    }

    public Task<WeeklyReflectionReadModel?> GetAsync(DateOnly weekOf, CancellationToken ct = default)
    {
        _store.Reflections.TryGetValue((_currentUser.UserId, weekOf), out WeeklyReflectionReadModel? reflection);
        return Task.FromResult(reflection);
    }

    public Task SaveAsync(
        DateOnly weekOf,
        string whatWentWell,
        string whatDragged,
        string adjustment,
        DateTimeOffset savedAt,
        CancellationToken ct = default)
    {
        WeeklyReflectionReadModel reflection = new(whatWentWell, whatDragged, adjustment, savedAt);
        _store.Reflections[(_currentUser.UserId, weekOf)] = reflection;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<WeeklyReflectionSnapshot>> ListAllForCurrentUserAsync(CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        IReadOnlyList<WeeklyReflectionSnapshot> snapshots = _store.Reflections
            .Where(kvp => kvp.Key.UserId == userId)
            .OrderBy(kvp => kvp.Key.WeekOf)
            .Select(kvp => new WeeklyReflectionSnapshot(
                kvp.Key.WeekOf,
                kvp.Value.WhatWentWell,
                kvp.Value.WhatDragged,
                kvp.Value.Adjustment,
                kvp.Value.SavedAt))
            .ToList();
        return Task.FromResult(snapshots);
    }
}
