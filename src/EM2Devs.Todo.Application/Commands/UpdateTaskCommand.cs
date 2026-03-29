using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record UpdateTaskCommand(
    Guid TaskId,
    string? Title = null,
    string? Description = null,
    string? Difficulty = null,
    DateTimeOffset? DueDate = null,
    bool ClearDueDate = false) : IRequest<Result<TodoTask>>;

public sealed class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result<TodoTask>>
{
    private readonly ITaskRepository _repository;

    public UpdateTaskCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TodoTask>> Handle(UpdateTaskCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        TodoTask? task = await _repository.GetByIdAsync(new TaskId(request.TaskId), ct).ConfigureAwait(false);
        if (task is null)
        {
            return new NotFoundError($"Task with id '{request.TaskId}' was not found.");
        }

        if (request.Title is not null)
        {
            TaskTitle newTitle;
            try
            {
                newTitle = new TaskTitle(request.Title);
            }
            catch (DomainException ex)
            {
                return new ValidationError(ex.Message);
            }

            task.UpdateTitle(newTitle);
        }

        if (request.Description is not null)
        {
            task.UpdateDescription(request.Description);
        }

        if (request.Difficulty is not null)
        {
            if (!Enum.TryParse<TaskDifficulty>(request.Difficulty, out TaskDifficulty difficulty))
            {
                return new ValidationError($"Invalid difficulty '{request.Difficulty}'.");
            }

            task.UpdateDifficulty(difficulty);
        }

        if (request.ClearDueDate)
        {
            task.UpdateDueDate(null);
        }
        else if (request.DueDate.HasValue)
        {
            task.UpdateDueDate(request.DueDate.Value);
        }

        await _repository.SaveAsync(task, ct).ConfigureAwait(false);
        return task;
    }
}
