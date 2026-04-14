using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

/// <summary>
/// Records the actual time spent on a completed task, producing an <see cref="EstimationRecord"/>
/// stored on the task. Requires the task to be Done and to have an estimate.
/// </summary>
public sealed record RecordActualTimeCommand(Guid TaskId, int ActualMinutes) : IRequest<Result<TodoTask>>;

public sealed class RecordActualTimeCommandHandler : IRequestHandler<RecordActualTimeCommand, Result<TodoTask>>
{
    private readonly ITaskRepository _repository;

    public RecordActualTimeCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TodoTask>> Handle(RecordActualTimeCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        TodoTask? task = await _repository.GetByIdAsync(new TaskId(request.TaskId), ct).ConfigureAwait(false);
        if (task is null)
        {
            return new NotFoundError($"Task with id '{request.TaskId}' was not found.");
        }

        TimeEstimate actual;
        try
        {
            actual = TimeEstimate.FromMinutes(request.ActualMinutes);
        }
        catch (DomainException ex)
        {
            return new ValidationError(ex.Message);
        }

        try
        {
            task.RecordActualTime(actual);
        }
        catch (DomainException ex)
        {
            return new ConflictError(ex.Message);
        }

        await _repository.SaveAsync(task, ct).ConfigureAwait(false);
        return task;
    }
}
