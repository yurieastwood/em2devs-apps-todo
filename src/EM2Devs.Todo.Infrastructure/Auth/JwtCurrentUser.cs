using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EM2Devs.Todo.Application.Ports;
using Microsoft.AspNetCore.Http;

namespace EM2Devs.Todo.Infrastructure.Auth;

/// <summary>
/// <see cref="ICurrentUser"/> implementation that reads the authenticated principal
/// from <see cref="HttpContext.User"/> via <see cref="IHttpContextAccessor"/>.
/// Falls back to an anonymous user when no HTTP context or the principal is unauthenticated.
/// </summary>
public sealed class JwtCurrentUser : ICurrentUser
{
    private const string AnonymousDisplayName = "Anonymous";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            ClaimsPrincipal? principal = _httpContextAccessor.HttpContext?.User;
            if (principal?.Identity is not { IsAuthenticated: true })
            {
                return Guid.Empty;
            }

            string? sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(sub, out Guid id) ? id : Guid.Empty;
        }
    }

    public string DisplayName
    {
        get
        {
            ClaimsPrincipal? principal = _httpContextAccessor.HttpContext?.User;
            if (principal?.Identity is not { IsAuthenticated: true })
            {
                return AnonymousDisplayName;
            }

            string? name = principal.FindFirst(JwtRegisteredClaimNames.Name)?.Value
                ?? principal.FindFirst(ClaimTypes.Name)?.Value;

            return string.IsNullOrWhiteSpace(name) ? AnonymousDisplayName : name;
        }
    }

    public bool IsAuthenticated
        => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
