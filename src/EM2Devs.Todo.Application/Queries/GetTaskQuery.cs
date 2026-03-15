using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetTaskQuery(Guid TaskId) : IRequest<TodoTask?>;

public sealed class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, TodoTask?>
{
    private readonly ITaskRepository _repository;

    public GetTaskQueryHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoTask?> Handle(GetTaskQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _repository.GetByIdAsync(new TaskId(request.TaskId), ct).ConfigureAwait(false);
    }
}
