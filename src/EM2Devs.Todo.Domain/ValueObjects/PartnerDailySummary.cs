namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Daily summary visible to an accountability partner.
/// Shows aggregate stats (tasks completed, streak, active quests) without
/// revealing individual task titles or descriptions — privacy by design.
/// </summary>
public sealed record PartnerDailySummary
{
    public int TasksCompleted { get; }
    public int CurrentStreak { get; }
    public int XpEarnedToday { get; }
    public int ActiveQuestCount { get; }
    public DateOnly Date { get; }

    public PartnerDailySummary(
        int tasksCompleted,
        int currentStreak,
        int xpEarnedToday,
        int activeQuestCount,
        DateOnly date)
    {
        if (tasksCompleted < 0)
        {
            throw new Exceptions.DomainException("Tasks completed cannot be negative.");
        }

        if (currentStreak < 0)
        {
            throw new Exceptions.DomainException("Current streak cannot be negative.");
        }

        if (xpEarnedToday < 0)
        {
            throw new Exceptions.DomainException("XP earned today cannot be negative.");
        }

        if (activeQuestCount < 0)
        {
            throw new Exceptions.DomainException("Active quest count cannot be negative.");
        }

        TasksCompleted = tasksCompleted;
        CurrentStreak = currentStreak;
        XpEarnedToday = xpEarnedToday;
        ActiveQuestCount = activeQuestCount;
        Date = date;
    }
}
