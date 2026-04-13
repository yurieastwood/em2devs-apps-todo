namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A seasonal quest line with sequential stages.
/// Each stage requires completing a number of tasks at a minimum difficulty.
/// </summary>
public sealed record SeasonalQuestLine
{
    public const int MaxStages = 8;

    public int TotalStages { get; }
    public int CurrentStage { get; }
    public int TasksCompletedInStage { get; }
    public bool IsLocked { get; }

    public SeasonalQuestLine(int totalStages, int currentStage, int tasksCompletedInStage, bool isLocked = false)
    {
        if (totalStages < 1 || totalStages > MaxStages)
        {
            throw new Exceptions.DomainException(
                $"Total stages must be between 1 and {MaxStages}.");
        }

        if (currentStage < 1 || currentStage > totalStages + 1)
        {
            throw new Exceptions.DomainException(
                "Current stage is out of range.");
        }

        if (tasksCompletedInStage < 0)
        {
            throw new Exceptions.DomainException(
                "Tasks completed in stage cannot be negative.");
        }

        TotalStages = totalStages;
        CurrentStage = currentStage;
        TasksCompletedInStage = tasksCompletedInStage;
        IsLocked = isLocked;
    }

    public static SeasonalQuestLine Start(int totalStages) =>
        new(totalStages, 1, 0);

    public bool IsCompleted => CurrentStage > TotalStages && !IsLocked;

    public bool IsStageAvailable(int stage) =>
        stage == CurrentStage && !IsCompleted && !IsLocked;

    public SeasonalQuestLine RecordTaskCompletion(int tasksRequiredForCurrentStage)
    {
        if (IsCompleted || IsLocked)
        {
            return this;
        }

        int newCount = TasksCompletedInStage + 1;

        if (newCount >= tasksRequiredForCurrentStage)
        {
            return new SeasonalQuestLine(TotalStages, CurrentStage + 1, 0);
        }

        return new SeasonalQuestLine(TotalStages, CurrentStage, newCount);
    }

    public int TasksRemainingInStage(int tasksRequiredForCurrentStage)
    {
        if (IsCompleted || IsLocked)
        {
            return 0;
        }

        return tasksRequiredForCurrentStage - TasksCompletedInStage;
    }

    /// <summary>
    /// Locks the quest line when the season ends, preventing further progression.
    /// </summary>
    public SeasonalQuestLine Lock() =>
        new(TotalStages, CurrentStage, TasksCompletedInStage, true);
}
