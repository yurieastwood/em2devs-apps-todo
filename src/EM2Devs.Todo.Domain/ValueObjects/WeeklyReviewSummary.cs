using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object containing the summary metrics displayed during a weekly review.
/// Includes tasks completed, tasks created, quests completed, current streak days, and XP earned.
/// </summary>
public sealed record WeeklyReviewSummary
{
    public int TasksCompleted { get; }
    public int TasksCreated { get; }
    public int QuestsCompleted { get; }
    public int CurrentStreakDays { get; }
    public int XpEarned { get; }

    public WeeklyReviewSummary(
        int tasksCompleted,
        int tasksCreated,
        int questsCompleted,
        int currentStreakDays,
        int xpEarned)
    {
        if (tasksCompleted < 0)
        {
            throw new DomainException("Tasks completed cannot be negative.");
        }

        if (tasksCreated < 0)
        {
            throw new DomainException("Tasks created cannot be negative.");
        }

        if (questsCompleted < 0)
        {
            throw new DomainException("Quests completed cannot be negative.");
        }

        if (currentStreakDays < 0)
        {
            throw new DomainException("Current streak days cannot be negative.");
        }

        if (xpEarned < 0)
        {
            throw new DomainException("XP earned cannot be negative.");
        }

        TasksCompleted = tasksCompleted;
        TasksCreated = tasksCreated;
        QuestsCompleted = questsCompleted;
        CurrentStreakDays = currentStreakDays;
        XpEarned = xpEarned;
    }
}
