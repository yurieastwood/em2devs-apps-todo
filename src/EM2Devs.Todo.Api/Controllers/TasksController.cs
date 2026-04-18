using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Api.Extensions;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/tasks")]
[Route("api/v{version:apiVersion}/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Lists tasks optionally filtered by status or by a named view. The <c>status</c> and
    /// <c>view</c> query parameters are mutually exclusive; supplying both returns a 400.
    /// Valid views: <c>inbox</c>, <c>today</c>, <c>upcoming</c>, <c>completed</c>.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListTasks(
        [FromQuery] string? status,
        [FromQuery] string? view,
        CancellationToken ct)
    {
        // ASP.NET binds ?status= as null for string?; use Request.Query to detect presence with empty value
        string? statusFilter = Request.Query.ContainsKey("status") ? (status ?? string.Empty) : null;
        string? viewFilter = Request.Query.ContainsKey("view") ? (view ?? string.Empty) : null;

        Result<IReadOnlyList<TodoTask>> result = await _mediator.Send(new ListTasksQuery(statusFilter, viewFilter), ct).ConfigureAwait(false);
        return result.ToHttpResult(tasks => Ok(tasks.Select(MapToResponse)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<TodoTask> result = await _mediator.Send(
            new CreateTaskCommand(request.Title, request.ScheduledDate, request.Tags),
            ct).ConfigureAwait(false);
        return result.ToHttpResult(task =>
            CreatedAtAction(nameof(GetTask), new { taskId = task.Id.Value }, MapToResponse(task)));
    }

    [HttpPost("quick-add")]
    public async Task<IActionResult> QuickAddTask([FromBody] QuickAddTaskRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<TodoTask> result = await _mediator.Send(new QuickAddTaskCommand(request.Input), ct).ConfigureAwait(false);
        return result.ToHttpResult(task =>
            CreatedAtAction(nameof(GetTask), new { taskId = task.Id.Value }, MapToResponse(task)));
    }

    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetTask(Guid taskId, CancellationToken ct)
    {
        Result<TodoTask> result = await _mediator.Send(new GetTaskQuery(taskId), ct).ConfigureAwait(false);
        return result.ToHttpResult(task => Ok(MapToResponse(task)));
    }

    [HttpPatch("{taskId:guid}")]
    public async Task<IActionResult> UpdateTask(
        Guid taskId,
        [FromBody] UpdateTaskRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<TodoTask> result = await _mediator.Send(
            new UpdateTaskCommand(taskId, request.Title, request.Description, request.Difficulty, request.Priority,
                request.EstimatedMinutes, request.ClearEstimatedTime, request.DueDate, request.ClearDueDate),
            ct).ConfigureAwait(false);
        return result.ToHttpResult(task => Ok(MapToResponse(task)));
    }

    [HttpPatch("{taskId:guid}/status")]
    public async Task<IActionResult> UpdateTaskStatus(
        Guid taskId,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<TodoTask> result = await _mediator.Send(
            new UpdateTaskStatusCommand(taskId, request.Status), ct).ConfigureAwait(false);
        return result.ToHttpResult(task => Ok(MapToResponse(task)));
    }

    [HttpPatch("{taskId:guid}/reopen")]
    public async Task<IActionResult> ReopenTask(Guid taskId, CancellationToken ct)
    {
        Result<TodoTask> result = await _mediator.Send(new ReopenTaskCommand(taskId), ct).ConfigureAwait(false);
        return result.ToHttpResult(task => Ok(MapToResponse(task)));
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> DeleteTask(Guid taskId, CancellationToken ct)
    {
        Result<bool> result = await _mediator.Send(new DeleteTaskCommand(taskId), ct).ConfigureAwait(false);
        return result.ToHttpResult(_ => NoContent());
    }

    [HttpPatch("{taskId:guid}/actual-time")]
    public async Task<IActionResult> RecordActualTime(
        Guid taskId,
        [FromBody] RecordActualTimeRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<TodoTask> result = await _mediator.Send(
            new RecordActualTimeCommand(taskId, request.ActualMinutes), ct).ConfigureAwait(false);
        return result.ToHttpResult(task => Ok(MapToResponse(task)));
    }

    [HttpPost("{taskId:guid}/focus-mode/start")]
    public async Task<IActionResult> StartFocusMode(Guid taskId, CancellationToken ct)
    {
        Result<bool> result = await _mediator
            .Send(new StartFocusModeCommand(taskId), ct).ConfigureAwait(false);
        return result.ToHttpResult(_ => Ok(new { started = true }));
    }

    [HttpPost("focus-mode/end")]
    public async Task<IActionResult> EndFocusMode(CancellationToken ct)
    {
        Result<FocusModeResult> result = await _mediator
            .Send(new EndFocusModeCommand(), ct).ConfigureAwait(false);
        return result.ToHttpResult(r => Ok(new { taskId = r.TaskId, durationMinutes = r.DurationMinutes }));
    }

    [HttpGet("procrastination-candidates")]
    public async Task<IActionResult> GetProcrastinationCandidates(CancellationToken ct)
    {
        Result<IReadOnlyList<ProcrastinationCandidateReadModel>> result = await _mediator
            .Send(new GetProcrastinationCandidatesQuery(), ct).ConfigureAwait(false);
        return result.Match<IActionResult>(
            candidates => Ok(candidates),
            error => Problem(error.Message, statusCode: 500));
    }

    private static TaskResponse MapToResponse(TodoTask task)
    {
        DifficultyAdjustSuggestion? suggestion = task.ActualTimeRecord is not null && task.EstimatedTime is not null
            ? DifficultyAdjustSuggestion.Evaluate(task.Difficulty, task.EstimatedTime, task.ActualTimeRecord.Actual)
            : null;

        return new TaskResponse(
            task.Id.Value, task.Title.Value, task.Description, task.Status.ToString(),
            task.Difficulty.ToString(), task.Priority.ToString(), task.EstimatedTime?.Minutes,
            task.DueDate, task.CompletedAt, task.ScheduledDate,
            task.ActualTimeRecord?.Actual.Minutes,
            task.ActualTimeRecord is null ? null : (int)Math.Round(task.ActualTimeRecord.VariancePercent),
            task.Tags.Select(t => t.Value).ToArray(),
            suggestion?.SuggestedDifficulty.ToString(),
            suggestion?.Reason);
    }
}

public sealed record CreateTaskRequest(
    string Title,
    DateOnly? ScheduledDate = null,
    string[]? Tags = null);
public sealed record QuickAddTaskRequest(string Input);
public sealed record UpdateTaskRequest(
    string? Title = null,
    string? Description = null,
    string? Difficulty = null,
    string? Priority = null,
    int? EstimatedMinutes = null,
    bool ClearEstimatedTime = false,
    DateTimeOffset? DueDate = null,
    bool ClearDueDate = false);
public sealed record UpdateTaskStatusRequest(string Status);
public sealed record RecordActualTimeRequest(int ActualMinutes);
public sealed record TaskResponse(
    Guid Id, string Title, string? Description, string Status,
    string Difficulty, string Priority, int? EstimatedMinutes,
    DateTimeOffset? DueDate, DateTimeOffset? CompletedAt,
    DateOnly? ScheduledDate,
    int? ActualMinutes, int? VariancePercent,
    string[] Tags,
    string? DifficultySuggestion = null,
    string? DifficultySuggestionReason = null);
