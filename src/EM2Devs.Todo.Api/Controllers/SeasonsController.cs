using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/seasons")]
[Route("api/v{version:apiVersion}/seasons")]
public sealed class SeasonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SeasonsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentSeason(CancellationToken ct)
    {
        Result<CurrentSeasonReadModel> result = await _mediator
            .Send(new GetCurrentSeasonQuery(), ct)
            .ConfigureAwait(false);

        return result.Match<IActionResult>(
            season => Ok(season),
            error => Problem(error.Message, statusCode: 500));
    }
}
