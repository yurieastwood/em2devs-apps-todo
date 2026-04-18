using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Ports;

public interface IEnergyCheckInRepository
{
    Task<IReadOnlyList<EnergyCheckIn>> GetRecentAsync(int days = 60, CancellationToken ct = default);
    Task<EnergyCheckIn?> GetTodayAsync(CancellationToken ct = default);
    Task AddAsync(EnergyCheckIn checkIn, CancellationToken ct = default);
    Task UpdateAsync(EnergyCheckIn checkIn, CancellationToken ct = default);
}
