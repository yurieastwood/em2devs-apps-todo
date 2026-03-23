using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Ports;

public interface IEpicRepository
{
    Task<Epic?> GetByIdAsync(EpicId id, CancellationToken ct = default);
    Task<IReadOnlyList<Epic>> GetAllAsync(CancellationToken ct = default);
    Task SaveAsync(Epic epic, CancellationToken ct = default);
    Task<bool> DeleteAsync(EpicId id, CancellationToken ct = default);
}
