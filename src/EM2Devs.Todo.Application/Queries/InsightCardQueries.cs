using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record ListInsightCardsQuery(bool IncludeRead = false) : IRequest<Result<IReadOnlyList<InsightCardReadModel>>>;

public sealed class ListInsightCardsQueryHandler
    : IRequestHandler<ListInsightCardsQuery, Result<IReadOnlyList<InsightCardReadModel>>>
{
    private readonly IInsightCardRepository _repository;

    public ListInsightCardsQueryHandler(IInsightCardRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<InsightCardReadModel>>> Handle(ListInsightCardsQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        IReadOnlyList<InsightCard> cards = await _repository.GetForCurrentUserAsync(request.IncludeRead, ct).ConfigureAwait(false);

        var result = cards.Select(c => new InsightCardReadModel(
            c.Id.Value, c.Type.ToString(), c.Message, c.SupportingData, c.Status.ToString(), c.GeneratedAt)).ToList();

        return result;
    }
}
