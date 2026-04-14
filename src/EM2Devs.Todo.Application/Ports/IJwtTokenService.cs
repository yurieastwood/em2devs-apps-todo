using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Ports;

/// <summary>
/// Issued JWT token pair: raw token string and its absolute expiry.
/// </summary>
public sealed record JwtToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>
/// JWT issuance port. Implementations are responsible for signing and claims shape.
/// </summary>
public interface IJwtTokenService
{
    JwtToken Issue(User user);
}
