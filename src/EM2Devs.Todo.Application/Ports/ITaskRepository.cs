using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Ports;

public interface ITaskRepository
{
    Task<TodoTask?> GetByIdAsync(TaskId id, CancellationToken ct = default);
    Task<IReadOnlyList<TodoTask>> GetAllAsync(CancellationToken ct = default);
    Task SaveAsync(TodoTask task, CancellationToken ct = default);
}
