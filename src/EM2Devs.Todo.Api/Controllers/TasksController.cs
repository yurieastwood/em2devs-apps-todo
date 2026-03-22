using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Api.Extensions;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    private static readonly HashSet<string> _validStatusNames =
        new(Enum.GetNames<Domain.TaskStatus>(), StringComparer.Ordinal);

    public TasksController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> ListTasks([FromQuery] string? status, CancellationToken ct)
    {
        bool statusParamPresent = Request.Query.ContainsKey("status");

        if (statusParamPresent && !_validStatusNames.Contains(status ?? string.Empty))
        {
            return new ObjectResult(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9457",
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = $"Invalid status filter '{status}'. Valid values: Todo, InProgress, Done."
            })
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ContentTypes = { "application/problem+json" }
            };
        }

        Domain.TaskStatus? filter = null;
        if (statusParamPresent && Enum.TryParse<Domain.TaskStatus>(status, ignoreCase: false, out Domain.TaskStatus parsed))
        {
            filter = parsed;
        }

        Result<IReadOnlyList<TodoTask>> result = await _mediator.Send(new ListTasksQuery(filter), ct).ConfigureAwait(false);
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

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> DeleteTask(Guid taskId, CancellationToken ct)
    {
        Result<bool> result = await _mediator.Send(new DeleteTaskCommand(taskId), ct).ConfigureAwait(false);
        return result.ToHttpResult(_ => NoContent());
    }

    private static TaskResponse MapToResponse(TodoTask task) =>
        new(task.Id.Value, task.Title.Value, task.Status.ToString());
}

public sealed record CreateTaskRequest(string Title);
public sealed record UpdateTaskStatusRequest(string Status);
public sealed record TaskResponse(Guid Id, string Title, string Status);
