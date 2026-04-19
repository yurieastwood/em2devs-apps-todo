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
[Route("api/wrapped")]
[Route("api/v{version:apiVersion}/wrapped")]
public sealed class WrappedController : ControllerBase
{
    private readonly IMediator _mediator;

    public WrappedController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetWrapped(
        [FromQuery] int? year = null,
        CancellationToken ct = default)
    {
        Result<AnnualWrappedReadModel> result = await _mediator
            .Send(new GetAnnualWrappedQuery(year), ct)
            .ConfigureAwait(false);

        return result.Match<IActionResult>(
            wrapped => Ok(wrapped),
            error => Problem(error.Message, statusCode: 500));
    }
}
