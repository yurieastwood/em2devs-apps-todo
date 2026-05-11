using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Commands;

public sealed record RecoverAccountCommand(string Email, string Password) : IRequest<Result<LoginResult>>;

public sealed class RecoverAccountCommandHandler
    : IRequestHandler<RecoverAccountCommand, Result<LoginResult>>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _tokens;
    private readonly TimeProvider _clock;

    public RecoverAccountCommandHandler(
        IUserRepository users,
        IPasswordHasher hasher,
        IJwtTokenService tokens,
        TimeProvider clock)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
        _clock = clock;
    }

    public async Task<Result<LoginResult>> Handle(RecoverAccountCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        User? user = await _users.GetByEmailAsync(request.Email, ct).ConfigureAwait(false);
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            return new UnauthorizedError("Invalid email or password.");
        }

        if (!user.IsDeactivated)
        {
            return new ConflictError("Account is not deactivated. Use the standard login instead.");
        }

        if (user.HoldingPeriodElapsed(_clock.GetUtcNow()))
        {
            return new ConflictError("Holding period has elapsed. The account can no longer be recovered.");
        }

        user.Reactivate();
        await _users.SaveAsync(user, ct).ConfigureAwait(false);

        JwtToken token = _tokens.Issue(user);
        return new LoginResult(token.Token, user.Id.Value, user.DisplayName, token.ExpiresAt);
    }
}
