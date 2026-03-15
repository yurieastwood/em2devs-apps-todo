using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Published when a task transitions to Done status.
/// Triggers the gamification chain: XP award → achievement check (ADR-010).
/// </summary>
public sealed record TaskCompletedEvent(TaskId TaskId, TaskTitle Title) : INotification;
