using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Api.Extensions;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/timeline")]
[Route("api/v{version:apiVersion}/timeline")]
public sealed class TimelineController : ControllerBase
{
    private readonly IMediator _mediator;

    public TimelineController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetTimeline(
        [FromQuery] string? eventType = null,
        [FromQuery] Guid? cursor = null,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (pageSize < 1 || pageSize > 100)
        {
            ModelState.AddModelError("pageSize", "pageSize must be between 1 and 100.");
            return ValidationProblem(ModelState);
        }

        Result<TimelineReadModel> result = await _mediator
            .Send(new GetTimelineQuery(eventType, cursor, pageSize), ct)
            .ConfigureAwait(false);

        return result.Match<IActionResult>(
            timeline => Ok(timeline),
            error => Problem(error.Message, statusCode: 500));
    }
}
