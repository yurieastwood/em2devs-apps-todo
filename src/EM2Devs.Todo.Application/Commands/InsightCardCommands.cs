using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record MarkInsightReadCommand(Guid InsightId) : IRequest<Result<bool>>;
public sealed record SaveInsightCommand(Guid InsightId) : IRequest<Result<bool>>;
public sealed record DismissInsightCommand(Guid InsightId) : IRequest<Result<bool>>;

public sealed class MarkInsightReadCommandHandler : IRequestHandler<MarkInsightReadCommand, Result<bool>>
{
    private readonly IInsightCardRepository _repository;

    public MarkInsightReadCommandHandler(IInsightCardRepository repository) => _repository = repository;

    public async Task<Result<bool>> Handle(MarkInsightReadCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        InsightCard? card = await _repository.GetByIdAsync(new InsightCardId(request.InsightId), ct).ConfigureAwait(false);
        if (card is null)
        {
            return new NotFoundError("Insight card not found.");
        }

        card.MarkAsRead();
        await _repository.SaveAsync(card, ct).ConfigureAwait(false);
        return true;
    }
}

public sealed class SaveInsightCommandHandler : IRequestHandler<SaveInsightCommand, Result<bool>>
{
    private readonly IInsightCardRepository _repository;

    public SaveInsightCommandHandler(IInsightCardRepository repository) => _repository = repository;

    public async Task<Result<bool>> Handle(SaveInsightCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        InsightCard? card = await _repository.GetByIdAsync(new InsightCardId(request.InsightId), ct).ConfigureAwait(false);
        if (card is null)
        {
            return new NotFoundError("Insight card not found.");
        }

        card.Save();
        await _repository.SaveAsync(card, ct).ConfigureAwait(false);
        return true;
    }
}

public sealed class DismissInsightCommandHandler : IRequestHandler<DismissInsightCommand, Result<bool>>
{
    private readonly IInsightCardRepository _repository;

    public DismissInsightCommandHandler(IInsightCardRepository repository) => _repository = repository;

    public async Task<Result<bool>> Handle(DismissInsightCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        InsightCard? card = await _repository.GetByIdAsync(new InsightCardId(request.InsightId), ct).ConfigureAwait(false);
        if (card is null)
        {
            return new NotFoundError("Insight card not found.");
        }

        card.Dismiss();
        await _repository.SaveAsync(card, ct).ConfigureAwait(false);
        return true;
    }
}
