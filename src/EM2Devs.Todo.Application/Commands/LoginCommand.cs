using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Commands;

/// <summary>
/// Result of a successful login or registration: issued JWT plus minimal user identity
/// needed by the client to render the authenticated shell.
/// </summary>
public sealed record LoginResult(string Token, Guid UserId, string DisplayName, DateTimeOffset ExpiresAt);

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<LoginResult>>;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _tokens;

    public LoginCommandHandler(IUserRepository users, IPasswordHasher hasher, IJwtTokenService tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        User? user = await _users.GetByEmailAsync(request.Email, ct).ConfigureAwait(false);
        if (user is null)
        {
            return new UnauthorizedError("Invalid email or password.");
        }

        if (!_hasher.Verify(request.Password, user.PasswordHash))
        {
            return new UnauthorizedError("Invalid email or password.");
        }

        JwtToken token = _tokens.Issue(user);
        return new LoginResult(token.Token, user.Id.Value, user.DisplayName, token.ExpiresAt);
    }
}
