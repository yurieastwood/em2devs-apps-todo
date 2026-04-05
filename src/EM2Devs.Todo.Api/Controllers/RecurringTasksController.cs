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
[Route("api/recurring-tasks")]
[Route("api/v{version:apiVersion}/recurring-tasks")]
public sealed class RecurringTasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public RecurringTasksController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> ListRecurringTasks(CancellationToken ct)
    {
        Result<IReadOnlyList<RecurringTask>> result = await _mediator
            .Send(new ListRecurringTasksQuery(), ct).ConfigureAwait(false);
        return result.ToHttpResult(tasks => Ok(tasks.Select(MapToResponse)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecurringTask(
        [FromBody] CreateRecurringTaskRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<RecurringTask> result = await _mediator
            .Send(new CreateRecurringTaskCommand(request.Title, request.Pattern), ct).ConfigureAwait(false);
        return result.ToHttpResult(task =>
            CreatedAtAction(nameof(GetRecurringTask), new { recurringTaskId = task.Id.Value }, MapToResponse(task)));
    }

    [HttpGet("{recurringTaskId:guid}")]
    public async Task<IActionResult> GetRecurringTask(Guid recurringTaskId, CancellationToken ct)
    {
        Result<RecurringTask> result = await _mediator
            .Send(new GetRecurringTaskQuery(recurringTaskId), ct).ConfigureAwait(false);
        return result.ToHttpResult(task => Ok(MapToResponse(task)));
    }

    [HttpPut("{recurringTaskId:guid}")]
    public async Task<IActionResult> UpdateRecurringTask(
        Guid recurringTaskId, [FromBody] UpdateRecurringTaskRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<RecurringTask> result = await _mediator
            .Send(new UpdateRecurringTaskCommand(recurringTaskId, request.Title, request.Pattern,
                request.ApplyToFutureInstances), ct).ConfigureAwait(false);
        return result.ToHttpResult(task => Ok(MapToResponse(task)));
    }

    [HttpPatch("{recurringTaskId:guid}/pause")]
    public async Task<IActionResult> PauseRecurringTask(Guid recurringTaskId, CancellationToken ct)
    {
        Result<RecurringTask> result = await _mediator
            .Send(new PauseRecurringTaskCommand(recurringTaskId), ct).ConfigureAwait(false);
        return result.ToHttpResult(task => Ok(MapToResponse(task)));
    }

    [HttpPatch("{recurringTaskId:guid}/resume")]
    public async Task<IActionResult> ResumeRecurringTask(Guid recurringTaskId, CancellationToken ct)
    {
        Result<RecurringTask> result = await _mediator
            .Send(new ResumeRecurringTaskCommand(recurringTaskId), ct).ConfigureAwait(false);
        return result.ToHttpResult(task => Ok(MapToResponse(task)));
    }

    [HttpDelete("{recurringTaskId:guid}")]
    public async Task<IActionResult> DeleteRecurringTask(Guid recurringTaskId, CancellationToken ct)
    {
        Result<bool> result = await _mediator
            .Send(new DeleteRecurringTaskCommand(recurringTaskId), ct).ConfigureAwait(false);
        return result.ToHttpResult(_ => NoContent());
    }

    [HttpPatch("{recurringTaskId:guid}/instances/{instanceId:guid}/skip")]
    public async Task<IActionResult> SkipInstance(Guid recurringTaskId, Guid instanceId, CancellationToken ct)
    {
        Result<TodoTask> getResult = await _mediator
            .Send(new Application.Queries.GetTaskQuery(instanceId), ct).ConfigureAwait(false);

        IActionResult? ownershipError = getResult.Match<IActionResult?>(
            task => task.SourceRecurringTaskId?.Value != recurringTaskId
                ? NotFound(new { error = $"Instance '{instanceId}' does not belong to recurring task '{recurringTaskId}'." })
                : null,
            _ => null);

        if (!getResult.IsSuccess)
        {
            return getResult.ToHttpResult(_ => Ok());
        }

        if (ownershipError is not null)
        {
            return ownershipError;
        }

        Result<TodoTask> result = await _mediator
            .Send(new UpdateTaskStatusCommand(instanceId, "Skipped"), ct).ConfigureAwait(false);
        return result.ToHttpResult(task =>
            Ok(new TaskInstanceResponse(task.Id.Value, task.Title.Value, task.Description, task.Status.ToString(),
                task.Difficulty.ToString(), task.DueDate, task.CompletedAt, task.ScheduledDate,
                task.SourceRecurringTaskId?.Value)));
    }

    [HttpGet("{recurringTaskId:guid}/instances")]
    public async Task<IActionResult> ListInstances(Guid recurringTaskId, CancellationToken ct)
    {
        Result<IReadOnlyList<TodoTask>> result = await _mediator
            .Send(new ListRecurringTaskInstancesQuery(recurringTaskId), ct).ConfigureAwait(false);
        return result.ToHttpResult(instances => Ok(instances.Select(t =>
            new TaskInstanceResponse(t.Id.Value, t.Title.Value, t.Description, t.Status.ToString(),
                t.Difficulty.ToString(), t.DueDate, t.CompletedAt, t.ScheduledDate,
                t.SourceRecurringTaskId?.Value))));
    }

    [HttpPost("{recurringTaskId:guid}/generate")]
    public async Task<IActionResult> GenerateInstance(
        Guid recurringTaskId, [FromQuery] DateOnly? scheduledDate, CancellationToken ct)
    {
        if (Request.Query.ContainsKey("scheduledDate") && !scheduledDate.HasValue)
        {
            ModelState.AddModelError("scheduledDate", "Invalid scheduledDate format. Expected: yyyy-MM-dd");
            return ValidationProblem(ModelState);
        }

        Result<TodoTask> result = await _mediator
            .Send(new GenerateInstancesCommand(recurringTaskId, scheduledDate), ct).ConfigureAwait(false);
        return result.ToHttpResult(task =>
            Ok(new TaskInstanceResponse(task.Id.Value, task.Title.Value, task.Description, task.Status.ToString(),
                task.Difficulty.ToString(), task.DueDate, task.CompletedAt, task.ScheduledDate,
                task.SourceRecurringTaskId?.Value)));
    }

    private static RecurringTaskResponse MapToResponse(RecurringTask task) =>
        new(task.Id.Value, task.Title.Value, task.Pattern.ToString(), task.IsActive);
}

public sealed record CreateRecurringTaskRequest(string Title, string Pattern);
public sealed record UpdateRecurringTaskRequest(
    string? Title = null, string? Pattern = null, bool ApplyToFutureInstances = false);
public sealed record RecurringTaskResponse(Guid Id, string Title, string Pattern, bool IsActive);
public sealed record TaskInstanceResponse(
    Guid Id, string Title, string? Description, string Status, string Difficulty,
    DateTimeOffset? DueDate, DateTimeOffset? CompletedAt, DateOnly? ScheduledDate,
    Guid? SourceRecurringTaskId);
