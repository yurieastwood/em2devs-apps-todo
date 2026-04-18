using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Api.Extensions;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/energy")]
[Route("api/v{version:apiVersion}/energy")]
public sealed class EnergyController : ControllerBase
{
    private readonly IMediator _mediator;

    public EnergyController(IMediator mediator) => _mediator = mediator;

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn(
        [FromBody] EnergyCheckInRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<EnergyCheckInResult> result = await _mediator
            .Send(new EnergyCheckInCommand(request.Level), ct)
            .ConfigureAwait(false);

        return result.ToHttpResult(r => Ok(r));
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        Result<EnergyProfileReadModel> result = await _mediator
            .Send(new GetEnergyProfileQuery(), ct)
            .ConfigureAwait(false);

        return result.Match<IActionResult>(
            profile => Ok(profile),
            error => Problem(error.Message, statusCode: 500));
    }
}

public sealed record EnergyCheckInRequest(string Level);
