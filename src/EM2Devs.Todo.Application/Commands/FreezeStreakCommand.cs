using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Application.Commands;

/// <summary>
/// Activates a streak freeze for the current user for the given number of days.
/// The freeze starts on today's local date resolved via the injected
/// <see cref="TimeProvider"/>, and protects the streak from breaking while active.
/// </summary>
public sealed record FreezeStreakCommand(int Days) : IRequest<Result<PlayerProfileReadModel>>;

public sealed class FreezeStreakCommandHandler
    : IRequestHandler<FreezeStreakCommand, Result<PlayerProfileReadModel>>
{
    private readonly IPlayerProfileRepository _repository;
    private readonly TimeProvider _timeProvider;

    public FreezeStreakCommandHandler(IPlayerProfileRepository repository, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(timeProvider);
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<PlayerProfileReadModel>> Handle(
        FreezeStreakCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        try
        {
            await _repository.FreezeStreakAsync(today, request.Days, ct).ConfigureAwait(false);
        }
        catch (DomainException ex)
        {
            return new ConflictError(ex.Message);
        }

        PlayerProfileReadModel profile = await _repository.GetProfileAsync(ct).ConfigureAwait(false);
        return profile;
    }
}
