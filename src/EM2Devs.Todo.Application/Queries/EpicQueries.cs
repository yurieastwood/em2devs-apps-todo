using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetEpicQuery(Guid EpicId) : IRequest<Result<Epic>>;

public sealed class GetEpicQueryHandler : IRequestHandler<GetEpicQuery, Result<Epic>>
{
    private readonly IEpicRepository _repository;

    public GetEpicQueryHandler(IEpicRepository repository) => _repository = repository;

    public async Task<Result<Epic>> Handle(GetEpicQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Epic? epic = await _repository.GetByIdAsync(new EpicId(request.EpicId), ct).ConfigureAwait(false);
        if (epic is null)
        {
            return new NotFoundError($"Epic with id '{request.EpicId}' was not found.");
        }

        return epic;
    }
}

public sealed record ListEpicsQuery : IRequest<Result<IReadOnlyList<Epic>>>;

public sealed class ListEpicsQueryHandler : IRequestHandler<ListEpicsQuery, Result<IReadOnlyList<Epic>>>
{
    private readonly IEpicRepository _repository;

    public ListEpicsQueryHandler(IEpicRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<Epic>>> Handle(ListEpicsQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<Epic> epics = await _repository.GetAllAsync(ct).ConfigureAwait(false);
        return Result<IReadOnlyList<Epic>>.Success(epics);
    }
}
