using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Published when a task is deleted.
/// Triggers quest progress recalculation for any quest containing this task.
/// Maps to: task-management.feature - "Delete a task that belongs to a quest"
/// </summary>
public sealed record TaskDeletedEvent(TaskId TaskId) : INotification;
