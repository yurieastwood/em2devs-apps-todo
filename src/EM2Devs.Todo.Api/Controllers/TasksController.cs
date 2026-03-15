using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator) => _mediator = mediator;

    private static readonly HashSet<string> _validStatusValues =
        new(Enum.GetNames<Domain.TaskStatus>(), StringComparer.Ordinal);

    [HttpGet]
    public async Task<IActionResult> ListTasks([FromQuery] string? status, CancellationToken ct)
    {
        bool statusParamPresent = Request.Query.ContainsKey("status");

        if (statusParamPresent && !_validStatusValues.Contains(status ?? string.Empty))
        {
            return BadRequest(new { error = $"Invalid status filter '{status}'. Valid values: Todo, InProgress, Done." });
        }

        Domain.TaskStatus? filter = null;
        if (statusParamPresent && Enum.TryParse<Domain.TaskStatus>(status, ignoreCase: false, out Domain.TaskStatus parsed))
        {
            filter = parsed;
        }

        IReadOnlyList<TodoTask> tasks = await _mediator.Send(new ListTasksQuery(filter), ct).ConfigureAwait(false);
        return Ok(tasks.Select(MapToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        TodoTask task;
        try
        {
            task = await _mediator.Send(new CreateTaskCommand(request.Title), ct).ConfigureAwait(false);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return CreatedAtAction(nameof(GetTask), new { taskId = task.Id.Value }, MapToResponse(task));
    }

    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetTask(Guid taskId, CancellationToken ct)
    {
        TodoTask? task = await _mediator.Send(new GetTaskQuery(taskId), ct).ConfigureAwait(false);
        return task is null ? NotFound() : Ok(MapToResponse(task));
    }

    [HttpPatch("{taskId:guid}/status")]
    public async Task<IActionResult> UpdateTaskStatus(
        Guid taskId,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            TodoTask? task = await _mediator.Send(
                new UpdateTaskStatusCommand(taskId, request.Status), ct).ConfigureAwait(false);

            if (task is null)
            {
                return NotFound();
            }

            return Ok(MapToResponse(task));
        }
        catch (DomainException ex) when (ex.Message.Contains("Invalid status"))
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> DeleteTask(Guid taskId, CancellationToken ct)
    {
        bool deleted = await _mediator.Send(new DeleteTaskCommand(taskId), ct).ConfigureAwait(false);
        return deleted ? NoContent() : NotFound();
    }

    private static TaskResponse MapToResponse(TodoTask task) =>
        new(task.Id.Value, task.Title.Value, task.Status.ToString());
}

public sealed record CreateTaskRequest(string Title);
public sealed record UpdateTaskStatusRequest(string Status);
public sealed record TaskResponse(Guid Id, string Title, string Status);
