using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Published when a task transitions to Done status.
/// Triggers the gamification chain: XP award → achievement check (ADR-010).
/// </summary>
public sealed record TaskCompletedEvent(
    TaskId TaskId,
    TaskTitle Title,
    TaskDifficulty? Difficulty = null,
    DateTimeOffset? Deadline = null,
    DateTimeOffset? CompletedAt = null) : INotification;

/// <summary>
/// Published when XP gain causes a level up.
/// Maps to: levelling.feature — "Level milestones are celebrated"
/// </summary>
public sealed record LevelUpEvent(int PreviousLevel, int NewLevel) : INotification;
