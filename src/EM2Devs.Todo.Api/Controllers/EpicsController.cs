using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Api.Extensions;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/epics")]
[Route("api/v{version:apiVersion}/epics")]
public sealed class EpicsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EpicsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> ListEpics(CancellationToken ct)
    {
        Result<IReadOnlyList<Epic>> result = await _mediator.Send(new ListEpicsQuery(), ct).ConfigureAwait(false);
        return result.ToHttpResult(epics => Ok(epics.Select(MapToResponse)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateEpic([FromBody] CreateEpicRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<Epic> result = await _mediator.Send(
            new CreateEpicCommand(request.Title, request.Description, request.TargetDate), ct).ConfigureAwait(false);
        return result.ToHttpResult(epic =>
            CreatedAtAction(nameof(GetEpic), new { epicId = epic.Id.Value }, MapToResponse(epic)));
    }

    [HttpGet("{epicId:guid}")]
    public async Task<IActionResult> GetEpic(Guid epicId, CancellationToken ct)
    {
        Result<Epic> result = await _mediator.Send(new GetEpicQuery(epicId), ct).ConfigureAwait(false);
        return result.ToHttpResult(epic => Ok(MapToResponse(epic)));
    }

    [HttpPost("{epicId:guid}/quests")]
    public async Task<IActionResult> AssignQuest(Guid epicId, [FromBody] AssignQuestRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<Epic> result = await _mediator.Send(
            new AssignQuestToEpicCommand(epicId, request.QuestId), ct).ConfigureAwait(false);
        return result.ToHttpResult(epic => Ok(MapToResponse(epic)));
    }

    [HttpDelete("{epicId:guid}/quests/{questId:guid}")]
    public async Task<IActionResult> RemoveQuest(Guid epicId, Guid questId, CancellationToken ct)
    {
        Result<Epic> result = await _mediator.Send(
            new RemoveQuestFromEpicCommand(epicId, questId), ct).ConfigureAwait(false);
        return result.ToHttpResult(epic => Ok(MapToResponse(epic)));
    }

    [HttpPost("{epicId:guid}/complete")]
    public async Task<IActionResult> CompleteEpic(Guid epicId, CancellationToken ct)
    {
        Result<Epic> result = await _mediator.Send(new CompleteEpicCommand(epicId), ct).ConfigureAwait(false);
        return result.ToHttpResult(epic => Ok(MapToResponse(epic)));
    }

    [HttpDelete("{epicId:guid}")]
    public async Task<IActionResult> DeleteEpic(Guid epicId, CancellationToken ct)
    {
        Result<bool> result = await _mediator.Send(new DeleteEpicCommand(epicId), ct).ConfigureAwait(false);
        return result.ToHttpResult(_ => NoContent());
    }

    private static EpicResponse MapToResponse(Epic epic) =>
        new(epic.Id.Value,
            epic.Title.Value,
            epic.Description,
            epic.TargetDate,
            epic.Progress,
            epic.IsCompleted,
            epic.Quests.Select(q => new EpicQuestResponse(q.Id.Value, q.Title.Value, q.Progress)).ToList());
}

public sealed record CreateEpicRequest(string Title, string Description, DateOnly? TargetDate = null);
public sealed record AssignQuestRequest(Guid QuestId);
public sealed record EpicQuestResponse(Guid Id, string Title, int Progress);
public sealed record EpicResponse(
    Guid Id, string Title, string Description, DateOnly? TargetDate, decimal Progress, bool IsCompleted, List<EpicQuestResponse> Quests);
