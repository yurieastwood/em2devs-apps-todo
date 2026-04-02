using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record CreateRecurringTaskCommand(string Title, string Pattern)
    : IRequest<Result<RecurringTask>>;

public sealed class CreateRecurringTaskCommandHandler
    : IRequestHandler<CreateRecurringTaskCommand, Result<RecurringTask>>
{
    private readonly IRecurringTaskRepository _repository;

    public CreateRecurringTaskCommandHandler(IRecurringTaskRepository repository) =>
        _repository = repository;

    public async Task<Result<RecurringTask>> Handle(CreateRecurringTaskCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        TaskTitle title;
        try
        {
            title = new TaskTitle(request.Title);
        }
        catch (DomainException ex)
        {
            return new ValidationError(ex.Message);
        }

        if (!Enum.TryParse<RecurrencePattern>(request.Pattern, ignoreCase: true, out RecurrencePattern pattern))
        {
            return new ValidationError($"Invalid recurrence pattern: '{request.Pattern}'. Valid values: Daily, Weekly, Monthly.");
        }

        RecurringTask recurringTask = RecurringTask.Create(title, pattern);
        await _repository.SaveAsync(recurringTask, ct).ConfigureAwait(false);
        return recurringTask;
    }
}

public sealed record GenerateInstancesCommand(Guid RecurringTaskId, DateOnly? ScheduledDate = null) : IRequest<Result<TodoTask>>;

public sealed class GenerateInstancesCommandHandler
    : IRequestHandler<GenerateInstancesCommand, Result<TodoTask>>
{
    private readonly IRecurringTaskRepository _recurringRepository;
    private readonly ITaskRepository _taskRepository;

    public GenerateInstancesCommandHandler(
        IRecurringTaskRepository recurringRepository, ITaskRepository taskRepository)
    {
        _recurringRepository = recurringRepository;
        _taskRepository = taskRepository;
    }

    public async Task<Result<TodoTask>> Handle(GenerateInstancesCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        RecurringTask? recurring = await _recurringRepository
            .GetByIdAsync(new RecurringTaskId(request.RecurringTaskId), ct).ConfigureAwait(false);
        if (recurring is null)
        {
            return new NotFoundError($"Recurring task with id '{request.RecurringTaskId}' was not found.");
        }

        DateOnly scheduledDate = request.ScheduledDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        TodoTask instance;
        try
        {
            instance = recurring.GenerateNextInstance(scheduledDate);
        }
        catch (DomainException ex)
        {
            return new ConflictError(ex.Message);
        }

        await _taskRepository.SaveAsync(instance, ct).ConfigureAwait(false);
        return instance;
    }
}

public sealed record PauseRecurringTaskCommand(Guid RecurringTaskId) : IRequest<Result<RecurringTask>>;

public sealed class PauseRecurringTaskCommandHandler
    : IRequestHandler<PauseRecurringTaskCommand, Result<RecurringTask>>
{
    private readonly IRecurringTaskRepository _repository;

    public PauseRecurringTaskCommandHandler(IRecurringTaskRepository repository) =>
        _repository = repository;

    public async Task<Result<RecurringTask>> Handle(PauseRecurringTaskCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        RecurringTask? recurring = await _repository
            .GetByIdAsync(new RecurringTaskId(request.RecurringTaskId), ct).ConfigureAwait(false);
        if (recurring is null)
        {
            return new NotFoundError($"Recurring task with id '{request.RecurringTaskId}' was not found.");
        }

        try
        {
            recurring.Pause();
        }
        catch (DomainException ex)
        {
            return new ConflictError(ex.Message);
        }

        await _repository.SaveAsync(recurring, ct).ConfigureAwait(false);
        return recurring;
    }
}

public sealed record ResumeRecurringTaskCommand(Guid RecurringTaskId) : IRequest<Result<RecurringTask>>;

public sealed class ResumeRecurringTaskCommandHandler
    : IRequestHandler<ResumeRecurringTaskCommand, Result<RecurringTask>>
{
    private readonly IRecurringTaskRepository _repository;

    public ResumeRecurringTaskCommandHandler(IRecurringTaskRepository repository) =>
        _repository = repository;

    public async Task<Result<RecurringTask>> Handle(ResumeRecurringTaskCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        RecurringTask? recurring = await _repository
            .GetByIdAsync(new RecurringTaskId(request.RecurringTaskId), ct).ConfigureAwait(false);
        if (recurring is null)
        {
            return new NotFoundError($"Recurring task with id '{request.RecurringTaskId}' was not found.");
        }

        try
        {
            recurring.Resume();
        }
        catch (DomainException ex)
        {
            return new ConflictError(ex.Message);
        }

        await _repository.SaveAsync(recurring, ct).ConfigureAwait(false);
        return recurring;
    }
}

public sealed record UpdateRecurringTaskCommand(
    Guid RecurringTaskId,
    string? Title = null,
    string? Pattern = null,
    bool ApplyToFutureInstances = false) : IRequest<Result<RecurringTask>>;

public sealed class UpdateRecurringTaskCommandHandler
    : IRequestHandler<UpdateRecurringTaskCommand, Result<RecurringTask>>
{
    private readonly IRecurringTaskRepository _recurringRepository;
    private readonly ITaskRepository _taskRepository;

    public UpdateRecurringTaskCommandHandler(
        IRecurringTaskRepository recurringRepository, ITaskRepository taskRepository)
    {
        _recurringRepository = recurringRepository;
        _taskRepository = taskRepository;
    }

    public async Task<Result<RecurringTask>> Handle(UpdateRecurringTaskCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        RecurringTask? recurring = await _recurringRepository
            .GetByIdAsync(new RecurringTaskId(request.RecurringTaskId), ct).ConfigureAwait(false);
        if (recurring is null)
        {
            return new NotFoundError($"Recurring task with id '{request.RecurringTaskId}' was not found.");
        }

        if (request.Title is not null)
        {
            TaskTitle title;
            try
            {
                title = new TaskTitle(request.Title);
            }
            catch (DomainException ex)
            {
                return new ValidationError(ex.Message);
            }

            recurring.UpdateTitle(title);
        }

        if (request.Pattern is not null)
        {
            if (!Enum.TryParse<RecurrencePattern>(request.Pattern, ignoreCase: true, out RecurrencePattern pattern))
            {
                return new ValidationError($"Invalid recurrence pattern: '{request.Pattern}'. Valid values: Daily, Weekly, Monthly.");
            }

            try
            {
                recurring.UpdatePattern(pattern);
            }
            catch (DomainException ex)
            {
                return new ValidationError(ex.Message);
            }
        }

        await _recurringRepository.SaveAsync(recurring, ct).ConfigureAwait(false);

        if (request.ApplyToFutureInstances && request.Title is not null)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            IReadOnlyList<TodoTask> instances = await _taskRepository
                .GetByRecurringTaskIdAsync(recurring.Id, ct).ConfigureAwait(false);

            TaskTitle newTitle = new TaskTitle(request.Title);
            List<TodoTask> modified = [];

            foreach (TodoTask instance in instances)
            {
                if (instance.ScheduledDate.HasValue
                    && instance.ScheduledDate.Value >= today
                    && instance.Status == Domain.TaskStatus.Todo)
                {
                    instance.UpdateTitle(newTitle);
                    modified.Add(instance);
                }
            }

            foreach (TodoTask instance in modified)
            {
                await _taskRepository.SaveAsync(instance, ct).ConfigureAwait(false);
            }
        }

        return recurring;
    }
}

public sealed record DeleteRecurringTaskCommand(Guid RecurringTaskId) : IRequest<Result<bool>>;

public sealed class DeleteRecurringTaskCommandHandler
    : IRequestHandler<DeleteRecurringTaskCommand, Result<bool>>
{
    private readonly IRecurringTaskRepository _repository;

    public DeleteRecurringTaskCommandHandler(IRecurringTaskRepository repository) =>
        _repository = repository;

    public async Task<Result<bool>> Handle(DeleteRecurringTaskCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        bool deleted = await _repository
            .DeleteAsync(new RecurringTaskId(request.RecurringTaskId), ct).ConfigureAwait(false);
        if (!deleted)
        {
            return new NotFoundError($"Recurring task with id '{request.RecurringTaskId}' was not found.");
        }

        return true;
    }
}
