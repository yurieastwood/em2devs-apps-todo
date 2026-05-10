using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class InMemoryEnergyCheckInStore
{
    private readonly Dictionary<Guid, List<EnergyCheckIn>> _checkIns = new();
    private readonly object _lock = new();

    public IReadOnlyList<EnergyCheckIn> GetForUser(Guid userId, int days)
    {
        lock (_lock)
        {
            if (!_checkIns.TryGetValue(userId, out List<EnergyCheckIn>? list))
            {
                return [];
            }

            DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-days);
            return list.Where(c => c.RecordedAt >= cutoff).ToList().AsReadOnly();
        }
    }

    public EnergyCheckIn? GetToday(Guid userId)
    {
        lock (_lock)
        {
            if (!_checkIns.TryGetValue(userId, out List<EnergyCheckIn>? list))
            {
                return null;
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            return list.FirstOrDefault(c => DateOnly.FromDateTime(c.RecordedAt.UtcDateTime) == today);
        }
    }

    public void Add(Guid userId, EnergyCheckIn checkIn)
    {
        lock (_lock)
        {
            if (!_checkIns.TryGetValue(userId, out List<EnergyCheckIn>? list))
            {
                list = [];
                _checkIns[userId] = list;
            }

            list.Add(checkIn);
        }
    }

    public void RemoveAllForUser(Guid userId)
    {
        lock (_lock)
        {
            _checkIns.Remove(userId);
        }
    }
}

public sealed class InMemoryEnergyCheckInRepository : IEnergyCheckInRepository
{
    private readonly InMemoryEnergyCheckInStore _store;
    private readonly ICurrentUser _currentUser;

    public InMemoryEnergyCheckInRepository(InMemoryEnergyCheckInStore store, ICurrentUser currentUser)
    {
        _store = store;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<EnergyCheckIn>> GetRecentAsync(int days = 60, CancellationToken ct = default)
        => Task.FromResult(_store.GetForUser(_currentUser.UserId, days));

    public Task<EnergyCheckIn?> GetTodayAsync(CancellationToken ct = default)
        => Task.FromResult(_store.GetToday(_currentUser.UserId));

    public Task AddAsync(EnergyCheckIn checkIn, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(checkIn);
        _store.Add(_currentUser.UserId, checkIn);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(EnergyCheckIn checkIn, CancellationToken ct = default)
        => Task.CompletedTask;
}
