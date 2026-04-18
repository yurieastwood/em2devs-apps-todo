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
[Route("api/insights")]
[Route("api/v{version:apiVersion}/insights")]
public sealed class InsightsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InsightsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> ListInsights(
        [FromQuery] bool includeRead = false,
        CancellationToken ct = default)
    {
        Result<IReadOnlyList<InsightCardReadModel>> result = await _mediator
            .Send(new ListInsightCardsQuery(includeRead), ct)
            .ConfigureAwait(false);

        return result.Match<IActionResult>(
            cards => Ok(cards),
            error => Problem(error.Message, statusCode: 500));
    }

    [HttpPost("{insightId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid insightId, CancellationToken ct)
    {
        Result<bool> result = await _mediator
            .Send(new MarkInsightReadCommand(insightId), ct)
            .ConfigureAwait(false);
        return result.ToHttpResult(_ => NoContent());
    }

    [HttpPost("{insightId:guid}/save")]
    public async Task<IActionResult> SaveInsight(Guid insightId, CancellationToken ct)
    {
        Result<bool> result = await _mediator
            .Send(new SaveInsightCommand(insightId), ct)
            .ConfigureAwait(false);
        return result.ToHttpResult(_ => NoContent());
    }

    [HttpPost("{insightId:guid}/dismiss")]
    public async Task<IActionResult> DismissInsight(Guid insightId, CancellationToken ct)
    {
        Result<bool> result = await _mediator
            .Send(new DismissInsightCommand(insightId), ct)
            .ConfigureAwait(false);
        return result.ToHttpResult(_ => NoContent());
    }
}
