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
[Route("api/subscription")]
[Route("api/v{version:apiVersion}/subscription")]
public sealed class SubscriptionController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetSubscription(CancellationToken ct)
    {
        Result<SubscriptionReadModel> result = await _mediator
            .Send(new GetSubscriptionQuery(), ct)
            .ConfigureAwait(false);

        return result.Match<IActionResult>(
            sub => Ok(sub),
            error => Problem(error.Message, statusCode: 500));
    }
}
