using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object containing the summary metrics for a weekly review.
/// Captures tasks completed, tasks created, quests completed,
/// current streak days, and XP earned during the week.
/// </summary>
public sealed record WeeklyReviewSummary
{
    public int TasksCompleted { get; }
    public int TasksCreated { get; }
    public int QuestsCompleted { get; }
    public int CurrentStreak { get; }
    public ExperiencePoints XpEarned { get; }

    public WeeklyReviewSummary(
        int tasksCompleted,
        int tasksCreated,
        int questsCompleted,
        int currentStreak,
        ExperiencePoints xpEarned)
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

        if (currentStreak < 0)
        {
            throw new DomainException("Current streak cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(xpEarned);

        TasksCompleted = tasksCompleted;
        TasksCreated = tasksCreated;
        QuestsCompleted = questsCompleted;
        CurrentStreak = currentStreak;
        XpEarned = xpEarned;
    }
}
