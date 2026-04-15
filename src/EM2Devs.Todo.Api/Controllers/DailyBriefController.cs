using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Api.Extensions;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;

namespace EM2Devs.Todo.Api.Controllers;

/// <summary>
/// Returns the authenticated user's "today's brief": a stateless projection of
/// open tasks (core plan + if-time-allows + overdue) plus the current streak.
/// Read-only — no brief entity is persisted.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/daily-brief")]
[Route("api/v{version:apiVersion}/daily-brief")]
public sealed class DailyBriefController : ControllerBase
{
    private readonly IMediator _mediator;

    public DailyBriefController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetDailyBrief(CancellationToken ct)
    {
        Result<DailyBriefReadModel> result =
            await _mediator.Send(new GetDailyBriefQuery(), ct).ConfigureAwait(false);

        return result.ToHttpResult(brief => Ok(Map(brief)));
    }

    private static DailyBriefResponse Map(DailyBriefReadModel brief)
    {
        return new DailyBriefResponse(
            brief.Date,
            brief.Greeting,
            brief.CurrentStreakDays,
            brief.CorePlanCount,
            brief.IfTimeAllowsCount,
            brief.OverdueCount,
            brief.CorePlan.Select(MapTask).ToList(),
            brief.IfTimeAllows.Select(MapTask).ToList(),
            brief.Overdue.Select(MapTask).ToList(),
            brief.Status);
    }

    private static DailyBriefTaskResponse MapTask(DailyBriefTaskReadModel task)
    {
        return new DailyBriefTaskResponse(
            task.Id,
            task.Title,
            task.Difficulty,
            task.Priority,
            task.EstimatedMinutes,
            task.CalibratedMinutes,
            task.ScheduledDate);
    }
}

public sealed record DailyBriefResponse(
    DateOnly Date,
    string Greeting,
    int CurrentStreakDays,
    int CorePlanCount,
    int IfTimeAllowsCount,
    int OverdueCount,
    IReadOnlyList<DailyBriefTaskResponse> CorePlan,
    IReadOnlyList<DailyBriefTaskResponse> IfTimeAllows,
    IReadOnlyList<DailyBriefTaskResponse> Overdue,
    string Status);

public sealed record DailyBriefTaskResponse(
    Guid Id,
    string Title,
    string Difficulty,
    string Priority,
    int? EstimatedMinutes,
    int? CalibratedMinutes,
    DateOnly? ScheduledDate);
