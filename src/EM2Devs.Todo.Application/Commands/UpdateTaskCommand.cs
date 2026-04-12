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
    string? Priority = null,
    int? EstimatedMinutes = null,
    bool ClearEstimatedTime = false,
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
            if (!Enum.TryParse<TaskDifficulty>(request.Difficulty, out TaskDifficulty difficulty) ||
                !Enum.IsDefined(difficulty))
            {
                return new ValidationError($"Invalid difficulty '{request.Difficulty}'.");
            }

            task.UpdateDifficulty(difficulty);
        }

        if (request.Priority is not null)
        {
            string validPriorities = string.Join(", ", Enum.GetNames<TaskPriority>());

            if (int.TryParse(request.Priority, out _) ||
                !Enum.TryParse<TaskPriority>(request.Priority, out TaskPriority priority) ||
                !Enum.IsDefined(priority))
            {
                return new ValidationError($"Invalid priority '{request.Priority}'. Valid values: {validPriorities}.");
            }

            task.UpdatePriority(priority);
        }

        if (request.ClearEstimatedTime)
        {
            task.UpdateEstimatedTime(null);
        }
        else if (request.EstimatedMinutes.HasValue)
        {
            TimeEstimate estimate;
            try
            {
                estimate = TimeEstimate.FromMinutes(request.EstimatedMinutes.Value);
            }
            catch (DomainException ex)
            {
                return new ValidationError(ex.Message);
            }

            task.UpdateEstimatedTime(estimate);
        }

        if (request.ClearDueDate)
        {
            task.UpdateDueDate(null);
        }
        else if (request.DueDate.HasValue)
        {
            DateTimeOffset dueDate = request.DueDate.Value;
            if (dueDate.Offset < TimeSpan.FromHours(-14) || dueDate.Offset > TimeSpan.FromHours(14))
            {
                return new ValidationError("Due date has an invalid timezone offset.");
            }

            task.UpdateDueDate(dueDate);
        }

        await _repository.SaveAsync(task, ct).ConfigureAwait(false);
        return task;
    }
}
