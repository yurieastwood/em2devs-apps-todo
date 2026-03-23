using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetQuestQuery(Guid QuestId) : IRequest<Result<Quest>>;

public sealed class GetQuestQueryHandler : IRequestHandler<GetQuestQuery, Result<Quest>>
{
    private readonly IQuestRepository _repository;

    public GetQuestQueryHandler(IQuestRepository repository) => _repository = repository;

    public async Task<Result<Quest>> Handle(GetQuestQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Quest? quest = await _repository.GetByIdAsync(new QuestId(request.QuestId), ct).ConfigureAwait(false);
        if (quest is null)
        {
            return new NotFoundError($"Quest with id '{request.QuestId}' was not found.");
        }

        return quest;
    }
}

public sealed record ListQuestsQuery : IRequest<Result<IReadOnlyList<Quest>>>;

public sealed class ListQuestsQueryHandler : IRequestHandler<ListQuestsQuery, Result<IReadOnlyList<Quest>>>
{
    private readonly IQuestRepository _repository;

    public ListQuestsQueryHandler(IQuestRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<Quest>>> Handle(ListQuestsQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<Quest> quests = await _repository.GetAllAsync(ct).ConfigureAwait(false);
        return Result<IReadOnlyList<Quest>>.Success(quests);
    }
}
