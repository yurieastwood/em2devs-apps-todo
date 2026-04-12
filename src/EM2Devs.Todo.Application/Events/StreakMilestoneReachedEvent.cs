using EM2Devs.Todo.Application.Mediator;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Published when a streak reaches a milestone threshold (7, 14, 30, 60, 100, 365 days).
/// Maps to: docs/features/progression/streaks.feature — "Streak milestone celebration"
/// </summary>
public sealed record StreakMilestoneReachedEvent(int StreakDays, string Label) : INotification;
