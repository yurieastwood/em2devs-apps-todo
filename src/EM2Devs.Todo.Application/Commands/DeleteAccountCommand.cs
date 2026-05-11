using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record DeleteAccountCommand(string? Confirmation) : IRequest<Result<bool>>;

public sealed class DeleteAccountCommandHandler
    : IRequestHandler<DeleteAccountCommand, Result<bool>>
{
    /// <summary>
    /// Literal phrase required to confirm account deletion. Distinct from the data-only
    /// purge phrase so the bigger action gets its own typed confirmation.
    /// </summary>
    public const string RequiredConfirmation = "DELETE MY ACCOUNT";

    private readonly IUserDataPurger _purger;
    private readonly IUserRepository _users;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _clock;

    public DeleteAccountCommandHandler(
        IUserDataPurger purger,
        IUserRepository users,
        ICurrentUser currentUser,
        TimeProvider clock)
    {
        _purger = purger;
        _users = users;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<bool>> Handle(DeleteAccountCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.Equals(request.Confirmation, RequiredConfirmation, StringComparison.Ordinal))
        {
            return Result<bool>.Failure(new ValidationError(
                $"Confirmation phrase must be exactly '{RequiredConfirmation}'."));
        }

        User? user = await _users.GetByIdAsync(new UserId(_currentUser.UserId), ct).ConfigureAwait(false);
        if (user is null)
        {
            return Result<bool>.Failure(new NotFoundError("Authenticated user not found."));
        }

        // Idempotent — repeated calls (e.g. a client retrying with a still-valid JWT)
        // succeed without re-running the purge or violating User.Deactivate's invariant.
        if (user.IsDeactivated)
        {
            return true;
        }

        await _purger.PurgeAllForCurrentUserAsync(ct).ConfigureAwait(false);

        user.Deactivate(_clock.GetUtcNow());
        await _users.SaveAsync(user, ct).ConfigureAwait(false);

        return true;
    }
}
