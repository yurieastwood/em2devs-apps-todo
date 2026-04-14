using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning;
using EM2Devs.Todo.Api.Extensions;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/auth")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Register a new user account and issue a JWT.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<LoginResult> result = await _mediator.Send(
            new RegisterUserCommand(request.Email, request.Password, request.DisplayName), ct)
            .ConfigureAwait(false);

        return result.ToHttpResult(ok => Ok(new AuthResponse(
            ok.Token, ok.UserId, ok.DisplayName, ok.ExpiresAt)));
    }

    /// <summary>
    /// Authenticate with email and password and issue a JWT.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<LoginResult> result = await _mediator.Send(
            new LoginCommand(request.Email, request.Password), ct)
            .ConfigureAwait(false);

        return result.ToHttpResult(ok => Ok(new AuthResponse(
            ok.Token, ok.UserId, ok.DisplayName, ok.ExpiresAt)));
    }

    /// <summary>
    /// Return the authenticated user's identity as parsed from the bearer token claims.
    /// </summary>
    [HttpGet("me")]
    public IActionResult Me()
    {
        ClaimsPrincipal user = HttpContext.User;
        if (user.Identity is not { IsAuthenticated: true })
        {
            return Unauthorized();
        }

        string? sub = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(sub, out Guid userId))
        {
            return Unauthorized();
        }

        string displayName = user.FindFirst(JwtRegisteredClaimNames.Name)?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value
            ?? string.Empty;

        string email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? user.FindFirst(ClaimTypes.Email)?.Value
            ?? string.Empty;

        return Ok(new MeResponse(userId, displayName, email));
    }

    /// <summary>
    /// Logout is a no-op on the server for stateless JWT auth; the client discards its token.
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout() => NoContent();
}

public sealed record RegisterRequest(string Email, string Password, string DisplayName);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string Token, Guid UserId, string DisplayName, DateTimeOffset ExpiresAt);
public sealed record MeResponse(Guid UserId, string DisplayName, string Email);
