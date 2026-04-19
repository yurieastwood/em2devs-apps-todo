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
[Route("api/capacity")]
[Route("api/v{version:apiVersion}/capacity")]
public sealed class CapacityController : ControllerBase
{
    private readonly IMediator _mediator;

    public CapacityController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetCapacityOverview(CancellationToken ct)
    {
        Result<CapacityOverviewReadModel> result = await _mediator
            .Send(new GetCapacityOverviewQuery(), ct)
            .ConfigureAwait(false);

        return result.Match<IActionResult>(
            overview => Ok(overview),
            error => Problem(error.Message, statusCode: 500));
    }
}
