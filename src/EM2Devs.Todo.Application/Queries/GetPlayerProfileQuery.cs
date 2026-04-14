using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;

namespace EM2Devs.Todo.Application.Queries;

/// <summary>
/// Query returning the current user's player profile read model, including XP history,
/// titles, and skill trees for display on the progression dashboard.
/// </summary>
public sealed record GetPlayerProfileQuery : IRequest<Result<PlayerProfileReadModel>>;

public sealed class GetPlayerProfileQueryHandler
    : IRequestHandler<GetPlayerProfileQuery, Result<PlayerProfileReadModel>>
{
    private readonly IPlayerProfileRepository _repository;

    public GetPlayerProfileQueryHandler(IPlayerProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PlayerProfileReadModel>> Handle(GetPlayerProfileQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        PlayerProfileReadModel profile = await _repository.GetProfileAsync(ct).ConfigureAwait(false);
        return profile;
    }
}
