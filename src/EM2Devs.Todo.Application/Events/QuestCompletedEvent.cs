using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Published when a quest reaches 100% progress and is auto-completed.
/// Triggers quest completion bonus XP award.
/// Maps to: task-management.feature - "Complete the final task in a quest"
/// </summary>
public sealed record QuestCompletedEvent(QuestId QuestId, QuestTitle Title) : INotification;
