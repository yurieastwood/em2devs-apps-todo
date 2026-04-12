namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Transparent breakdown of XP awarded for a task completion.
/// Maps to: experience-points.feature — "View XP breakdown after task completion"
/// </summary>
public sealed record XpBreakdown
{
    public int BaseXp { get; }
    public double DeadlineModifier { get; }
    public double StreakMultiplier { get; }
    public double DiminishingReturnsFactor { get; }
    public int FinalXp { get; }

    public XpBreakdown(int baseXp, double deadlineModifier, double streakMultiplier, double diminishingReturnsFactor = 1.0)
    {
        BaseXp = baseXp;
        DeadlineModifier = deadlineModifier;
        StreakMultiplier = streakMultiplier;
        DiminishingReturnsFactor = diminishingReturnsFactor;
        FinalXp = Math.Max(1, (int)Math.Round(baseXp * deadlineModifier * streakMultiplier * diminishingReturnsFactor));
    }

    public ExperiencePoints ToExperiencePoints() => new(FinalXp);
}

/// <summary>
/// Calculates XP for task completion with deadline and streak modifiers.
/// Maps to: experience-points.feature — XP weighted by difficulty, timeliness, and consistency.
/// </summary>
public static class XpCalculator
{
    private const double EarlyCompletionBonus = 1.2;
    private const double LateCompletionPenalty = 0.8;
    private const double NoDeadlineModifier = 1.0;
    private const double BaseStreakMultiplier = 1.0;
    private const double StreakBonusPerDay = 0.02;
    private const int MaxStreakBonusDays = 30;

    /// <summary>
    /// Maximum number of trivial task completions per day before diminishing returns apply.
    /// </summary>
    internal const int TrivialDailyThreshold = 5;

    /// <summary>
    /// Each subsequent trivial task beyond the threshold earns half the XP of the previous one.
    /// </summary>
    private const double DiminishingReturnsFraction = 0.5;

    public static XpBreakdown Calculate(
        TaskDifficulty? difficulty,
        DateTimeOffset? deadline,
        DateTimeOffset completedAt,
        int currentStreakDays,
        int dailyTrivialCompletionCount = 0)
    {
        TaskDifficulty effectiveDifficulty = difficulty ?? TaskDifficulty.Normal;
        int baseXp = ExperiencePoints.BaseForDifficulty(effectiveDifficulty).Value;

        double deadlineModifier = CalculateDeadlineModifier(deadline, completedAt);
        double streakMultiplier = CalculateStreakMultiplier(currentStreakDays);
        double diminishingFactor = CalculateDiminishingReturnsFactor(effectiveDifficulty, dailyTrivialCompletionCount);

        return new XpBreakdown(baseXp, deadlineModifier, streakMultiplier, diminishingFactor);
    }

    /// <summary>
    /// Detects whether the daily trivial task completion count indicates a burst of trivial tasks
    /// (potential XP inflation/gaming). Burst is detected when count exceeds the threshold.
    /// </summary>
    public static bool IsTrivialBurst(int dailyTrivialCompletionCount)
    {
        return dailyTrivialCompletionCount > TrivialDailyThreshold;
    }

    private static double CalculateDeadlineModifier(DateTimeOffset? deadline, DateTimeOffset completedAt)
    {
        if (deadline is null)
        {
            return NoDeadlineModifier;
        }

        if (completedAt < deadline.Value)
        {
            return EarlyCompletionBonus;
        }

        if (completedAt > deadline.Value)
        {
            return LateCompletionPenalty;
        }

        return NoDeadlineModifier;
    }

    private static double CalculateStreakMultiplier(int currentStreakDays)
    {
        int effectiveDays = Math.Clamp(currentStreakDays, 0, MaxStreakBonusDays);
        return BaseStreakMultiplier + (effectiveDays * StreakBonusPerDay);
    }

    /// <summary>
    /// Calculates diminishing returns factor for trivial tasks beyond the daily threshold.
    /// First 5 trivial tasks: full XP (factor = 1.0).
    /// 6th trivial task: 50% (factor = 0.5).
    /// 7th trivial task: 25% (factor = 0.25).
    /// Each subsequent: halved again.
    /// Non-trivial tasks are never affected (factor = 1.0).
    /// </summary>
    private static double CalculateDiminishingReturnsFactor(TaskDifficulty difficulty, int dailyTrivialCompletionCount)
    {
        if (difficulty != TaskDifficulty.Trivial)
        {
            return 1.0;
        }

        if (dailyTrivialCompletionCount < TrivialDailyThreshold)
        {
            return 1.0;
        }

        int tasksOverThreshold = dailyTrivialCompletionCount - TrivialDailyThreshold;
        return Math.Pow(DiminishingReturnsFraction, tasksOverThreshold + 1);
    }
}
