using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskRepository _repository;

    public TasksController(ITaskRepository repository) => _repository = repository;

    [HttpGet]
    public async Task<IActionResult> ListTasks([FromQuery] string? status, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(status) && !Enum.TryParse<Domain.TaskStatus>(status, ignoreCase: false, out _))
        {
            return BadRequest(new { error = $"Invalid status filter '{status}'. Valid values: Todo, InProgress, Done." });
        }

        var tasks = await _repository.GetAllAsync(ct).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<Domain.TaskStatus>(status, ignoreCase: false, out Domain.TaskStatus parsed))
        {
            tasks = tasks.Where(t => t.Status == parsed).ToList().AsReadOnly();
        }

        return Ok(tasks.Select(MapToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        TaskTitle title;
        try
        {
            title = new TaskTitle(request.Title);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        var task = TodoTask.Create(title);
        await _repository.SaveAsync(task, ct).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetTask), new { taskId = task.Id.Value }, MapToResponse(task));
    }

    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetTask(Guid taskId, CancellationToken ct)
    {
        var guid = taskId;

        var task = await _repository.GetByIdAsync(new TaskId(guid), ct).ConfigureAwait(false);
        return task is null ? NotFound() : Ok(MapToResponse(task));
    }

    [HttpPatch("{taskId:guid}/status")]
    public async Task<IActionResult> UpdateTaskStatus(
        Guid taskId,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Enum.TryParse<Domain.TaskStatus>(request.Status, out var targetStatus))
        {
            return BadRequest(new { error = $"Invalid status value '{request.Status}'." });
        }

        var task = await _repository.GetByIdAsync(new TaskId(taskId), ct).ConfigureAwait(false);
        if (task is null)
        {
            return NotFound();
        }

        if (task.Status == targetStatus)
        {
            return Conflict(new { error = $"Task is already in status '{targetStatus}'." });
        }

        try
        {
            ApplyStatusTransition(task, targetStatus);
        }
        catch (DomainException ex)
        {
            return Conflict(new { error = ex.Message });
        }

        await _repository.SaveAsync(task, ct).ConfigureAwait(false);
        return Ok(MapToResponse(task));
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> DeleteTask(Guid taskId, CancellationToken ct)
    {
        bool deleted = await _repository.DeleteAsync(new TaskId(taskId), ct).ConfigureAwait(false);
        return deleted ? NoContent() : NotFound();
    }

    private static void ApplyStatusTransition(TodoTask task, Domain.TaskStatus targetStatus)
    {
        switch (targetStatus)
        {
            case Domain.TaskStatus.InProgress:
                task.MoveToInProgress();
                break;
            case Domain.TaskStatus.Done:
                task.MarkAsDone();
                break;
            default:
                throw new DomainException($"Transition to '{targetStatus}' is not supported.");
        }
    }

    private static TaskResponse MapToResponse(TodoTask task) =>
        new(task.Id.Value, task.Title.Value, task.Status.ToString());
}

public sealed record CreateTaskRequest(string Title);
public sealed record UpdateTaskStatusRequest(string Status);
public sealed record TaskResponse(Guid Id, string Title, string Status);
