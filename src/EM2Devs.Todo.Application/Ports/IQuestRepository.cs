using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Ports;

public interface IQuestRepository
{
    Task<Quest?> GetByIdAsync(QuestId id, CancellationToken ct = default);
    Task<IReadOnlyList<Quest>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Quest>> GetByTaskIdAsync(TaskId taskId, CancellationToken ct = default);
    Task SaveAsync(Quest quest, CancellationToken ct = default);
    Task<bool> DeleteAsync(QuestId id, CancellationToken ct = default);
}
