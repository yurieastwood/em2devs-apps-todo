using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
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
[Authorize]
[Route("api/quests")]
[Route("api/v{version:apiVersion}/quests")]
public sealed class QuestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuestsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> ListQuests(CancellationToken ct)
    {
        Result<IReadOnlyList<Quest>> result = await _mediator.Send(new ListQuestsQuery(), ct).ConfigureAwait(false);
        return result.ToHttpResult(quests => Ok(quests.Select(MapToResponse)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuest([FromBody] CreateQuestRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<Quest> result = await _mediator.Send(
            new CreateQuestCommand(request.Title, request.Description, request.DueDate), ct).ConfigureAwait(false);
        return result.ToHttpResult(quest =>
            CreatedAtAction(nameof(GetQuest), new { questId = quest.Id.Value }, MapToResponse(quest)));
    }

    [HttpGet("{questId:guid}")]
    public async Task<IActionResult> GetQuest(Guid questId, CancellationToken ct)
    {
        Result<Quest> result = await _mediator.Send(new GetQuestQuery(questId), ct).ConfigureAwait(false);
        return result.ToHttpResult(quest => Ok(MapToResponse(quest)));
    }

    [HttpPost("{questId:guid}/tasks")]
    public async Task<IActionResult> AddTask(Guid questId, [FromBody] AddTaskToQuestRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<Quest> result = await _mediator.Send(
            new AddTaskToQuestCommand(questId, request.TaskId), ct).ConfigureAwait(false);
        return result.ToHttpResult(quest => Ok(MapToResponse(quest)));
    }

    [HttpDelete("{questId:guid}/tasks/{taskId:guid}")]
    public async Task<IActionResult> RemoveTask(Guid questId, Guid taskId, CancellationToken ct)
    {
        Result<Quest> result = await _mediator.Send(
            new RemoveTaskFromQuestCommand(questId, taskId), ct).ConfigureAwait(false);
        return result.ToHttpResult(quest => Ok(MapToResponse(quest)));
    }

    [HttpPost("{questId:guid}/complete")]
    public async Task<IActionResult> CompleteQuest(Guid questId, CancellationToken ct)
    {
        Result<Quest> result = await _mediator.Send(new CompleteQuestCommand(questId), ct).ConfigureAwait(false);
        return result.ToHttpResult(quest => Ok(MapToResponse(quest)));
    }

    [HttpDelete("{questId:guid}")]
    public async Task<IActionResult> DeleteQuest(Guid questId, CancellationToken ct)
    {
        Result<bool> result = await _mediator.Send(new DeleteQuestCommand(questId), ct).ConfigureAwait(false);
        return result.ToHttpResult(_ => NoContent());
    }

    private static QuestResponse MapToResponse(Quest quest) =>
        new(quest.Id.Value,
            quest.Title.Value,
            quest.Description,
            quest.DueDate,
            quest.Progress,
            quest.IsCompleted,
            quest.Tasks.Select(t => new QuestTaskResponse(t.Id.Value, t.Title.Value, t.Status.ToString())).ToList());
}

public sealed record CreateQuestRequest(string Title, string Description, DateOnly? DueDate = null);
public sealed record AddTaskToQuestRequest(Guid TaskId);
public sealed record QuestTaskResponse(Guid Id, string Title, string Status);
public sealed record QuestResponse(
    Guid Id, string Title, string Description, DateOnly? DueDate, int Progress, bool IsCompleted, List<QuestTaskResponse> Tasks);
