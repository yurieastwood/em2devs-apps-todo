using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Queries;

public sealed record ListTasksQuery(string? StatusFilter) : IRequest<Result<IReadOnlyList<TodoTask>>>;

public sealed class ListTasksQueryHandler : IRequestHandler<ListTasksQuery, Result<IReadOnlyList<TodoTask>>>
{
    private readonly ITaskRepository _repository;

    public ListTasksQueryHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<TodoTask>>> Handle(ListTasksQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<TodoTask> tasks = await _repository.GetAllAsync(ct).ConfigureAwait(false);

        if (request.StatusFilter is not null &&
            Enum.TryParse<Domain.TaskStatus>(request.StatusFilter, ignoreCase: false, out Domain.TaskStatus parsed))
        {
            tasks = tasks.Where(t => t.Status == parsed).ToList().AsReadOnly();
        }

        return Result<IReadOnlyList<TodoTask>>.Success(tasks);
    }
}
