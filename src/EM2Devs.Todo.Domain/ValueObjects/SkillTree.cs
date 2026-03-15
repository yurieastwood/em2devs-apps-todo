namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Skill tree entity tracking tier progression through task completions.
/// Progress is permanent and does not decay on inactivity.
/// </summary>
public sealed record SkillTree
{
    public SkillTreeType Type { get; }
    public SkillTier CurrentTier { get; }
    public int TasksCompletedInTier { get; }

    public SkillTree(SkillTreeType type, SkillTier currentTier, int tasksCompletedInTier)
    {
        ArgumentNullException.ThrowIfNull(currentTier);

        if (tasksCompletedInTier < 0)
        {
            throw new Exceptions.DomainException(
                "Tasks completed in tier cannot be negative.");
        }

        Type = type;
        CurrentTier = currentTier;
        TasksCompletedInTier = tasksCompletedInTier;
    }

    public static SkillTree Discover(SkillTreeType type) =>
        new(type, new SkillTier(1), 0);

    public SkillTree RecordTaskCompletion()
    {
        if (CurrentTier.Value >= SkillTier.MaxTier)
        {
            return new SkillTree(Type, CurrentTier, TasksCompletedInTier + 1);
        }

        int newCount = TasksCompletedInTier + 1;
        int required = TasksRequiredForTier(CurrentTier.Value + 1);

        if (newCount >= required)
        {
            return new SkillTree(Type, new SkillTier(CurrentTier.Value + 1), 0);
        }

        return new SkillTree(Type, CurrentTier, newCount);
    }

    public int TasksToNextTier()
    {
        if (CurrentTier.Value >= SkillTier.MaxTier)
        {
            return 0;
        }

        return TasksRequiredForTier(CurrentTier.Value + 1) - TasksCompletedInTier;
    }

    /// <summary>
    /// Returns the number of tasks required to advance to the given tier.
    /// Tier 2 requires 30 tasks, Tier 3 requires 60 tasks.
    /// </summary>
    public static int TasksRequiredForTier(int tier)
    {
        if (tier < 2 || tier > SkillTier.MaxTier)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tier), tier,
                $"Task requirements are only defined for tiers 2 through {SkillTier.MaxTier}.");
        }

        return tier switch
        {
            2 => 30,
            _ => 60
        };
    }
}
