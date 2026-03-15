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
    public async Task<IActionResult> ListTasks(CancellationToken ct)
    {
        var tasks = await _repository.GetAllAsync(ct).ConfigureAwait(false);
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

    [HttpGet("{taskId}")]
    public async Task<IActionResult> GetTask(string taskId, CancellationToken ct)
    {
        if (!Guid.TryParseExact(taskId, "D", out var guid) || taskId != guid.ToString("D"))
        {
            return NotFound();
        }

        var task = await _repository.GetByIdAsync(new TaskId(guid), ct).ConfigureAwait(false);
        return task is null ? NotFound() : Ok(MapToResponse(task));
    }

    private static TaskResponse MapToResponse(TodoTask task) =>
        new(task.Id.Value, task.Title.Value, task.Status.ToString());
}

public sealed record CreateTaskRequest(string Title);
public sealed record TaskResponse(Guid Id, string Title, string Status);
