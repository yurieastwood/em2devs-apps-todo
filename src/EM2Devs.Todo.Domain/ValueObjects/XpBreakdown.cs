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
    public int FinalXp { get; }

    public XpBreakdown(int baseXp, double deadlineModifier, double streakMultiplier)
    {
        BaseXp = baseXp;
        DeadlineModifier = deadlineModifier;
        StreakMultiplier = streakMultiplier;
        FinalXp = Math.Max(1, (int)Math.Round(baseXp * deadlineModifier * streakMultiplier));
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

    public static XpBreakdown Calculate(
        TaskDifficulty? difficulty,
        DateTimeOffset? deadline,
        DateTimeOffset completedAt,
        int currentStreakDays)
    {
        TaskDifficulty effectiveDifficulty = difficulty ?? TaskDifficulty.Normal;
        int baseXp = ExperiencePoints.BaseForDifficulty(effectiveDifficulty).Value;

        double deadlineModifier = CalculateDeadlineModifier(deadline, completedAt);
        double streakMultiplier = CalculateStreakMultiplier(currentStreakDays);

        return new XpBreakdown(baseXp, deadlineModifier, streakMultiplier);
    }

    private static double CalculateDeadlineModifier(DateTimeOffset? deadline, DateTimeOffset completedAt)
    {
        if (deadline is null)
        {
            return NoDeadlineModifier;
        }

        return completedAt <= deadline.Value ? EarlyCompletionBonus : LateCompletionPenalty;
    }

    private static double CalculateStreakMultiplier(int currentStreakDays)
    {
        int effectiveDays = Math.Clamp(currentStreakDays, 0, MaxStreakBonusDays);
        return BaseStreakMultiplier + (effectiveDays * StreakBonusPerDay);
    }
}
