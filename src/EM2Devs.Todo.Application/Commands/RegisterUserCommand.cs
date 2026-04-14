using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Application.Commands;

public sealed record RegisterUserCommand(string Email, string Password, string DisplayName)
    : IRequest<Result<LoginResult>>;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<LoginResult>>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _tokens;
    private readonly TimeProvider _clock;

    public RegisterUserCommandHandler(
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

    public async Task<Result<LoginResult>> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        User? existing = await _users.GetByEmailAsync(request.Email, ct).ConfigureAwait(false);
        if (existing is not null)
        {
            return new ConflictError($"A user with email '{request.Email}' is already registered.");
        }

        string passwordHash = _hasher.Hash(request.Password);

        User user;
        try
        {
            user = User.Create(request.Email, passwordHash, request.DisplayName, _clock.GetUtcNow());
        }
        catch (DomainException ex)
        {
            return new ValidationError(ex.Message);
        }

        await _users.AddAsync(user, ct).ConfigureAwait(false);

        JwtToken token = _tokens.Issue(user);
        return new LoginResult(token.Token, user.Id.Value, user.DisplayName, token.ExpiresAt);
    }
}
