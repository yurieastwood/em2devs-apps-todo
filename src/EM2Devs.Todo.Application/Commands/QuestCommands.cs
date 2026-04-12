using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record CreateQuestCommand(string Title, string Description, DateOnly? DueDate = null)
    : IRequest<Result<Quest>>;

public sealed class CreateQuestCommandHandler : IRequestHandler<CreateQuestCommand, Result<Quest>>
{
    private readonly IQuestRepository _repository;

    public CreateQuestCommandHandler(IQuestRepository repository) => _repository = repository;

    public async Task<Result<Quest>> Handle(CreateQuestCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        QuestTitle title;
        try
        {
            title = new QuestTitle(request.Title);
        }
        catch (DomainException ex)
        {
            return new ValidationError(ex.Message);
        }

        Quest quest = Quest.Create(title, request.Description, request.DueDate);
        await _repository.SaveAsync(quest, ct).ConfigureAwait(false);
        return quest;
    }
}

public sealed record AddTaskToQuestCommand(Guid QuestId, Guid TaskId) : IRequest<Result<Quest>>;

public sealed class AddTaskToQuestCommandHandler : IRequestHandler<AddTaskToQuestCommand, Result<Quest>>
{
    private readonly IQuestRepository _questRepository;
    private readonly ITaskRepository _taskRepository;

    public AddTaskToQuestCommandHandler(IQuestRepository questRepository, ITaskRepository taskRepository)
    {
        _questRepository = questRepository;
        _taskRepository = taskRepository;
    }

    public async Task<Result<Quest>> Handle(AddTaskToQuestCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Quest? quest = await _questRepository.GetByIdAsync(new QuestId(request.QuestId), ct).ConfigureAwait(false);
        if (quest is null)
        {
            return new NotFoundError($"Quest with id '{request.QuestId}' was not found.");
        }

        TodoTask? task = await _taskRepository.GetByIdAsync(new TaskId(request.TaskId), ct).ConfigureAwait(false);
        if (task is null)
        {
            return new NotFoundError($"Task with id '{request.TaskId}' was not found.");
        }

        try
        {
            quest.AddTask(task);
        }
        catch (DomainException ex)
        {
            return new ConflictError(ex.Message);
        }

        await _questRepository.SaveAsync(quest, ct).ConfigureAwait(false);
        return quest;
    }
}

public sealed record RemoveTaskFromQuestCommand(Guid QuestId, Guid TaskId) : IRequest<Result<Quest>>;

public sealed class RemoveTaskFromQuestCommandHandler : IRequestHandler<RemoveTaskFromQuestCommand, Result<Quest>>
{
    private readonly IQuestRepository _repository;

    public RemoveTaskFromQuestCommandHandler(IQuestRepository repository) => _repository = repository;

    public async Task<Result<Quest>> Handle(RemoveTaskFromQuestCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Quest? quest = await _repository.GetByIdAsync(new QuestId(request.QuestId), ct).ConfigureAwait(false);
        if (quest is null)
        {
            return new NotFoundError($"Quest with id '{request.QuestId}' was not found.");
        }

        try
        {
            quest.RemoveTask(new TaskId(request.TaskId));
        }
        catch (DomainException ex)
        {
            return new NotFoundError(ex.Message);
        }

        await _repository.SaveAsync(quest, ct).ConfigureAwait(false);
        return quest;
    }
}

public sealed record CompleteQuestCommand(Guid QuestId) : IRequest<Result<Quest>>;

public sealed class CompleteQuestCommandHandler : IRequestHandler<CompleteQuestCommand, Result<Quest>>
{
    private readonly IQuestRepository _repository;

    public CompleteQuestCommandHandler(IQuestRepository repository) => _repository = repository;

    public async Task<Result<Quest>> Handle(CompleteQuestCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Quest? quest = await _repository.GetByIdAsync(new QuestId(request.QuestId), ct).ConfigureAwait(false);
        if (quest is null)
        {
            return new NotFoundError($"Quest with id '{request.QuestId}' was not found.");
        }

        if (quest.IsCompleted)
        {
            return quest;
        }

        try
        {
            quest.Complete();
        }
        catch (DomainException ex)
        {
            return new ConflictError(ex.Message);
        }

        await _repository.SaveAsync(quest, ct).ConfigureAwait(false);
        return quest;
    }
}

public sealed record DeleteQuestCommand(Guid QuestId) : IRequest<Result<bool>>;

public sealed class DeleteQuestCommandHandler : IRequestHandler<DeleteQuestCommand, Result<bool>>
{
    private readonly IQuestRepository _repository;

    public DeleteQuestCommandHandler(IQuestRepository repository) => _repository = repository;

    public async Task<Result<bool>> Handle(DeleteQuestCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        bool deleted = await _repository.DeleteAsync(new QuestId(request.QuestId), ct).ConfigureAwait(false);
        if (!deleted)
        {
            return new NotFoundError($"Quest with id '{request.QuestId}' was not found.");
        }

        return true;
    }
}
