using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Queries;

public sealed record ListTasksQuery(Domain.TaskStatus? StatusFilter) : IRequest<IReadOnlyList<TodoTask>>;

public sealed class ListTasksQueryHandler : IRequestHandler<ListTasksQuery, IReadOnlyList<TodoTask>>
{
    private readonly ITaskRepository _repository;

    public ListTasksQueryHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<TodoTask>> Handle(ListTasksQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<TodoTask> tasks = await _repository.GetAllAsync(ct).ConfigureAwait(false);

        if (request.StatusFilter.HasValue)
        {
            tasks = tasks.Where(t => t.Status == request.StatusFilter.Value).ToList().AsReadOnly();
        }

        return tasks;
    }
}
