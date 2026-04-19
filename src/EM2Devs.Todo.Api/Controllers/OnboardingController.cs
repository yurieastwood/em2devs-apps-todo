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
[Route("api/onboarding")]
[Route("api/v{version:apiVersion}/onboarding")]
public sealed class OnboardingController : ControllerBase
{
    private readonly IMediator _mediator;

    public OnboardingController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetOnboardingState(CancellationToken ct)
    {
        Result<OnboardingStateReadModel> result = await _mediator
            .Send(new GetOnboardingStateQuery(), ct)
            .ConfigureAwait(false);

        return result.Match<IActionResult>(
            state => Ok(state),
            error => Problem(error.Message, statusCode: 500));
    }
}
