using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;

namespace EM2Devs.Todo.Application.Commands;

public sealed record DeleteAllUserDataCommand(string? Confirmation) : IRequest<Result<bool>>;

public sealed class DeleteAllUserDataCommandHandler
    : IRequestHandler<DeleteAllUserDataCommand, Result<bool>>
{
    /// <summary>
    /// Literal phrase the caller must include in the confirmation field. Case-sensitive
    /// to add friction beyond a one-character typo.
    /// </summary>
    public const string RequiredConfirmation = "DELETE MY DATA";

    private readonly IUserDataPurger _purger;

    public DeleteAllUserDataCommandHandler(IUserDataPurger purger)
    {
        _purger = purger;
    }

    public async Task<Result<bool>> Handle(DeleteAllUserDataCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.Equals(request.Confirmation, RequiredConfirmation, StringComparison.Ordinal))
        {
            return Result<bool>.Failure(new ValidationError(
                $"Confirmation phrase must be exactly '{RequiredConfirmation}'."));
        }

        await _purger.PurgeAllForCurrentUserAsync(ct).ConfigureAwait(false);
        return true;
    }
}
