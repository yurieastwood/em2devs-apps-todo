using Asp.Versioning;
using EM2Devs.Todo.Api.Extensions;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EM2Devs.Todo.Api.Controllers;

/// <summary>
/// Surfaces the weekly review ritual: an at-a-glance recap of the user's week
/// (completed tasks, XP earned, streak delta) plus a free-text reflection that
/// the user saves. Reflection storage is scoped to the authenticated user
/// and anchored on a Sunday.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/weekly-review")]
[Route("api/v{version:apiVersion}/weekly-review")]
public sealed class WeeklyReviewController : ControllerBase
{
    private readonly IMediator _mediator;

    public WeeklyReviewController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetWeeklyReview(
        [FromQuery] DateOnly? weekOf = null,
        CancellationToken ct = default)
    {
        Result<WeeklyReviewReadModel> result = await _mediator
            .Send(new GetWeeklyReviewQuery(weekOf), ct)
            .ConfigureAwait(false);

        return result.ToHttpResult(model => Ok(Map(model)));
    }

    [HttpPost]
    public async Task<IActionResult> SaveWeeklyReview(
        [FromBody] SaveWeeklyReviewRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<WeeklyReflectionReadModel> result = await _mediator
            .Send(new SaveWeeklyReviewCommand(
                request.WhatWentWell,
                request.WhatDragged,
                request.Adjustment,
                request.WeekOf), ct)
            .ConfigureAwait(false);

        return result.ToHttpResult(reflection => Ok(Map(reflection)));
    }

    private static WeeklyReviewResponse Map(WeeklyReviewReadModel model)
    {
        return new WeeklyReviewResponse(
            model.WeekOf,
            model.TasksCompleted,
            model.XpEarned,
            model.StreakStart,
            model.StreakEnd,
            model.NotableEvents,
            model.Reflection is null ? null : Map(model.Reflection));
    }

    private static WeeklyReflectionResponse Map(WeeklyReflectionReadModel reflection)
    {
        return new WeeklyReflectionResponse(
            reflection.WhatWentWell,
            reflection.WhatDragged,
            reflection.Adjustment,
            reflection.SavedAt);
    }
}

public sealed record SaveWeeklyReviewRequest(
    string WhatWentWell,
    string WhatDragged,
    string Adjustment,
    DateOnly? WeekOf = null);

public sealed record WeeklyReviewResponse(
    DateOnly WeekOf,
    int TasksCompleted,
    int XpEarned,
    int StreakStart,
    int StreakEnd,
    IReadOnlyList<string> NotableEvents,
    WeeklyReflectionResponse? Reflection);

public sealed record WeeklyReflectionResponse(
    string WhatWentWell,
    string WhatDragged,
    string Adjustment,
    DateTimeOffset SavedAt);
