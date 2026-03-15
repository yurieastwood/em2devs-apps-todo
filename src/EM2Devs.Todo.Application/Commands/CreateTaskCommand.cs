using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record CreateTaskCommand(string Title) : IRequest<TodoTask>;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TodoTask>
{
    private readonly ITaskRepository _repository;

    public CreateTaskCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoTask> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        var title = new TaskTitle(request.Title);
        TodoTask task = TodoTask.Create(title);
        await _repository.SaveAsync(task, ct).ConfigureAwait(false);
        return task;
    }
}
