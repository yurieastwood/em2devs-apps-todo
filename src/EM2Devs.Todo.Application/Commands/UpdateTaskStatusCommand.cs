using EM2Devs.Todo.Application.Events;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using TaskStatus = EM2Devs.Todo.Domain.TaskStatus;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record UpdateTaskStatusCommand(Guid TaskId, string Status) : IRequest<TodoTask?>;

public sealed class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, TodoTask?>
{
    private readonly ITaskRepository _repository;
    private readonly IMediator _mediator;

    public UpdateTaskStatusCommandHandler(ITaskRepository repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<TodoTask?> Handle(UpdateTaskStatusCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Enum.TryParse<TaskStatus>(request.Status, out TaskStatus targetStatus))
        {
            throw new DomainException($"Invalid status value '{request.Status}'.");
        }

        TodoTask? task = await _repository.GetByIdAsync(new TaskId(request.TaskId), ct).ConfigureAwait(false);
        if (task is null)
        {
            return null;
        }

        if (task.Status == targetStatus)
        {
            throw new DomainException($"Task is already in status '{targetStatus}'.");
        }

        switch (targetStatus)
        {
            case TaskStatus.InProgress:
                task.MoveToInProgress();
                break;
            case TaskStatus.Done:
                task.MarkAsDone();
                break;
            default:
                throw new DomainException($"Transition to '{targetStatus}' is not supported.");
        }

        await _repository.SaveAsync(task, ct).ConfigureAwait(false);

        if (targetStatus == TaskStatus.Done)
        {
            await _mediator.Publish(new TaskCompletedEvent(task.Id, task.Title), ct).ConfigureAwait(false);
        }

        return task;
    }
}
