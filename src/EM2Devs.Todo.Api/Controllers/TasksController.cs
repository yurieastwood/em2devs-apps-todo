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
[Route("api/tasks")]
[Route("api/v{version:apiVersion}/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> ListTasks([FromQuery] string? status, CancellationToken ct)
    {
        // ASP.NET binds ?status= as null for string?; use Request.Query to detect presence with empty value
        string? statusFilter = Request.Query.ContainsKey("status") ? (status ?? string.Empty) : null;

        Result<IReadOnlyList<TodoTask>> result = await _mediator.Send(new ListTasksQuery(statusFilter), ct).ConfigureAwait(false);
        return result.ToHttpResult(tasks => Ok(tasks.Select(MapToResponse)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<TodoTask> result = await _mediator.Send(new CreateTaskCommand(request.Title), ct).ConfigureAwait(false);
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
            new UpdateTaskCommand(taskId, request.Title, request.Description, request.Difficulty, request.DueDate, request.ClearDueDate),
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

    private static TaskResponse MapToResponse(TodoTask task) =>
        new(task.Id.Value, task.Title.Value, task.Description, task.Status.ToString(),
            task.Difficulty.ToString(), task.DueDate, task.CompletedAt);
}

public sealed record CreateTaskRequest(string Title);
public sealed record UpdateTaskRequest(
    string? Title = null,
    string? Description = null,
    string? Difficulty = null,
    DateTimeOffset? DueDate = null,
    bool ClearDueDate = false);
public sealed record UpdateTaskStatusRequest(string Status);
public sealed record TaskResponse(
    Guid Id, string Title, string? Description, string Status,
    string Difficulty, DateTimeOffset? DueDate, DateTimeOffset? CompletedAt);
