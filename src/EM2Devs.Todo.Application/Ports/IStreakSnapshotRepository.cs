using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Ports;

public interface IStreakSnapshotRepository
{
    Task SaveAsync(StreakSnapshot snapshot, CancellationToken ct = default);
    Task<StreakSnapshot?> GetByDateAsync(DateOnly snapshotDate, CancellationToken ct = default);
    Task<IReadOnlyList<StreakSnapshot>> GetRangeAsync(DateOnly fromInclusive, DateOnly toInclusive, CancellationToken ct = default);
}
