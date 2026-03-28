using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Domain.ValueObjects;
using TaskStatus = EM2Devs.Todo.Domain.TaskStatus;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Published when a task's status changes.
/// Triggers quest progress recalculation for any quest containing this task.
/// </summary>
public sealed record TaskStatusChangedEvent(
    TaskId TaskId,
    TaskStatus NewStatus) : INotification;
