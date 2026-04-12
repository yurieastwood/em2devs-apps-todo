using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record CreateEpicCommand(string Title, string Description, DateOnly? TargetDate = null)
    : IRequest<Result<Epic>>;

public sealed class CreateEpicCommandHandler : IRequestHandler<CreateEpicCommand, Result<Epic>>
{
    private readonly IEpicRepository _repository;

    public CreateEpicCommandHandler(IEpicRepository repository) => _repository = repository;

    public async Task<Result<Epic>> Handle(CreateEpicCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        EpicTitle title;
        try
        {
            title = new EpicTitle(request.Title);
        }
        catch (DomainException ex)
        {
            return new ValidationError(ex.Message);
        }

        Epic epic = Epic.Create(title, request.Description, request.TargetDate);
        await _repository.SaveAsync(epic, ct).ConfigureAwait(false);
        return epic;
    }
}

public sealed record AssignQuestToEpicCommand(Guid EpicId, Guid QuestId) : IRequest<Result<Epic>>;

public sealed class AssignQuestToEpicCommandHandler : IRequestHandler<AssignQuestToEpicCommand, Result<Epic>>
{
    private readonly IEpicRepository _epicRepository;
    private readonly IQuestRepository _questRepository;

    public AssignQuestToEpicCommandHandler(IEpicRepository epicRepository, IQuestRepository questRepository)
    {
        _epicRepository = epicRepository;
        _questRepository = questRepository;
    }

    public async Task<Result<Epic>> Handle(AssignQuestToEpicCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Epic? epic = await _epicRepository.GetByIdAsync(new EpicId(request.EpicId), ct).ConfigureAwait(false);
        if (epic is null)
        {
            return new NotFoundError($"Epic with id '{request.EpicId}' was not found.");
        }

        Quest? quest = await _questRepository.GetByIdAsync(new QuestId(request.QuestId), ct).ConfigureAwait(false);
        if (quest is null)
        {
            return new NotFoundError($"Quest with id '{request.QuestId}' was not found.");
        }

        try
        {
            quest.AssignToEpic(epic.Id);
            epic.AddQuest(quest);
        }
        catch (DomainException ex)
        {
            return new ConflictError(ex.Message);
        }

        await _questRepository.SaveAsync(quest, ct).ConfigureAwait(false);
        await _epicRepository.SaveAsync(epic, ct).ConfigureAwait(false);
        return epic;
    }
}

public sealed record RemoveQuestFromEpicCommand(Guid EpicId, Guid QuestId) : IRequest<Result<Epic>>;

public sealed class RemoveQuestFromEpicCommandHandler : IRequestHandler<RemoveQuestFromEpicCommand, Result<Epic>>
{
    private readonly IEpicRepository _epicRepository;
    private readonly IQuestRepository _questRepository;

    public RemoveQuestFromEpicCommandHandler(IEpicRepository epicRepository, IQuestRepository questRepository)
    {
        _epicRepository = epicRepository;
        _questRepository = questRepository;
    }

    public async Task<Result<Epic>> Handle(RemoveQuestFromEpicCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Epic? epic = await _epicRepository.GetByIdAsync(new EpicId(request.EpicId), ct).ConfigureAwait(false);
        if (epic is null)
        {
            return new NotFoundError($"Epic with id '{request.EpicId}' was not found.");
        }

        QuestId questId = new(request.QuestId);

        try
        {
            epic.RemoveQuest(questId);
        }
        catch (DomainException ex)
        {
            return new NotFoundError(ex.Message);
        }

        Quest? quest = await _questRepository.GetByIdAsync(questId, ct).ConfigureAwait(false);
        if (quest?.EpicId is not null)
        {
            quest.UnassignFromEpic();
            await _questRepository.SaveAsync(quest, ct).ConfigureAwait(false);
        }

        await _epicRepository.SaveAsync(epic, ct).ConfigureAwait(false);
        return epic;
    }
}

public sealed record CompleteEpicCommand(Guid EpicId) : IRequest<Result<Epic>>;

public sealed class CompleteEpicCommandHandler : IRequestHandler<CompleteEpicCommand, Result<Epic>>
{
    private readonly IEpicRepository _repository;

    public CompleteEpicCommandHandler(IEpicRepository repository) => _repository = repository;

    public async Task<Result<Epic>> Handle(CompleteEpicCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Epic? epic = await _repository.GetByIdAsync(new EpicId(request.EpicId), ct).ConfigureAwait(false);
        if (epic is null)
        {
            return new NotFoundError($"Epic with id '{request.EpicId}' was not found.");
        }

        try
        {
            epic.Complete();
        }
        catch (DomainException ex)
        {
            return new ConflictError(ex.Message);
        }

        await _repository.SaveAsync(epic, ct).ConfigureAwait(false);
        return epic;
    }
}

public sealed record DeleteEpicCommand(Guid EpicId) : IRequest<Result<bool>>;

public sealed class DeleteEpicCommandHandler : IRequestHandler<DeleteEpicCommand, Result<bool>>
{
    private readonly IEpicRepository _repository;

    public DeleteEpicCommandHandler(IEpicRepository repository) => _repository = repository;

    public async Task<Result<bool>> Handle(DeleteEpicCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        bool deleted = await _repository.DeleteAsync(new EpicId(request.EpicId), ct).ConfigureAwait(false);
        if (!deleted)
        {
            return new NotFoundError($"Epic with id '{request.EpicId}' was not found.");
        }

        return true;
    }
}
