using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Api.Middleware;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/auth")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public AuthController(ICurrentUser currentUser) => _currentUser = currentUser;

    [HttpPost("login")]
    public IActionResult Login()
    {
        Response.Cookies.Append(DemoAuthMiddleware.CookieName, "true", new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });

        return Ok(new AuthResponse(_currentUser.UserId, _currentUser.DisplayName));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(DemoAuthMiddleware.CookieName, new CookieOptions
        {
            Path = "/"
        });

        return NoContent();
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

        return Ok(new AuthResponse(_currentUser.UserId, _currentUser.DisplayName));
    }
}

public sealed record AuthResponse(Guid UserId, string DisplayName);
