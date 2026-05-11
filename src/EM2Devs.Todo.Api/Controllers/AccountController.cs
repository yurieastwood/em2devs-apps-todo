using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Api.Extensions;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Domain;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/account")]
[Route("api/v{version:apiVersion}/account")]
public sealed class AccountController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator) => _mediator = mediator;

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteAccountRequest? request,
        CancellationToken ct)
    {
        if (request is null)
        {
            ModelState.AddModelError("body", "Request body is required.");
            return ValidationProblem(ModelState);
        }

        Result<bool> result = await _mediator
            .Send(new DeleteAccountCommand(request.Confirmation), ct)
            .ConfigureAwait(false);

        return result.ToHttpResult(_ => NoContent());
    }

    [HttpPost("recover")]
    [AllowAnonymous]
    public async Task<IActionResult> Recover(
        [FromBody] RecoverAccountRequest? request,
        CancellationToken ct)
    {
        if (request is null)
        {
            ModelState.AddModelError("body", "Request body is required.");
            return ValidationProblem(ModelState);
        }

        Result<LoginResult> result = await _mediator
            .Send(new RecoverAccountCommand(request.Email, request.Password), ct)
            .ConfigureAwait(false);

        return result.ToHttpResult(ok => Ok(new AuthResponse(
            ok.Token, ok.UserId, ok.DisplayName, ok.ExpiresAt)));
    }
}

public sealed record DeleteAccountRequest(string? Confirmation);
public sealed record RecoverAccountRequest(string Email, string Password);
