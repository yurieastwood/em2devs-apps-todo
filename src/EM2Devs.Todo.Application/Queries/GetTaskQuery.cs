using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetTaskQuery(Guid TaskId) : IRequest<Result<TodoTask>>;

public sealed class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, Result<TodoTask>>
{
    private readonly ITaskRepository _repository;

    public GetTaskQueryHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TodoTask>> Handle(GetTaskQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        TodoTask? task = await _repository.GetByIdAsync(new TaskId(request.TaskId), ct).ConfigureAwait(false);

        if (task is null)
        {
            return new NotFoundError($"Task with id '{request.TaskId}' was not found.");
        }

        return task;
    }
}
