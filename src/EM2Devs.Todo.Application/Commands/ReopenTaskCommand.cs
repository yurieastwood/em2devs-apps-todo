using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record ReopenTaskCommand(Guid TaskId) : IRequest<Result<TodoTask>>;

public sealed class ReopenTaskCommandHandler : IRequestHandler<ReopenTaskCommand, Result<TodoTask>>
{
    private readonly ITaskRepository _repository;

    public ReopenTaskCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TodoTask>> Handle(ReopenTaskCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        TodoTask? task = await _repository.GetByIdAsync(new TaskId(request.TaskId), ct).ConfigureAwait(false);
        if (task is null)
        {
            return new NotFoundError($"Task with id '{request.TaskId}' was not found.");
        }

        try
        {
            task.Reopen();
        }
        catch (DomainException ex)
        {
            return new ConflictError(ex.Message);
        }

        await _repository.SaveAsync(task, ct).ConfigureAwait(false);
        return task;
    }
}
