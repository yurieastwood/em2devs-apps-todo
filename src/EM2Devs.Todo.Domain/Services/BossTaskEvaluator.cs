using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Pure domain service that evaluates whether a task should be promoted to or demoted from Boss Task status.
/// No infrastructure dependencies — all decisions are based on task state.
/// </summary>
public static class BossTaskEvaluator
{
    private const int RescheduleThreshold = 3;
    private const int AgeDaysThreshold = 14;
    private const int AvoidanceViewThreshold = 5;

    /// <summary>
    /// Evaluates a task and promotes or demotes it based on promotion rules.
    /// Returns true if the task's boss status changed.
    /// </summary>
    public static bool Evaluate(TodoTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.Status == TaskStatus.Done || task.Status == TaskStatus.Skipped)
        {
            return false;
        }

        bool shouldBeBoss = ShouldPromote(task);

        if (shouldBeBoss && !task.IsBossTask)
        {
            task.PromoteToBossTask();
            return true;
        }

        if (!shouldBeBoss && task.IsBossTask)
        {
            task.DemoteFromBossTask();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a task meets any of the promotion criteria.
    /// </summary>
    public static bool ShouldPromote(TodoTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return IsPromotableByRescheduling(task)
            || IsPromotableByAgeAndPriority(task)
            || IsPromotableByDifficultyAndAvoidance(task);
    }

    /// <summary>
    /// Rule: A task rescheduled 3 or more times should be promoted.
    /// </summary>
    private static bool IsPromotableByRescheduling(TodoTask task)
    {
        return task.RescheduleCount >= RescheduleThreshold;
    }

    /// <summary>
    /// Rule: A high or critical priority task open for 14+ days should be promoted.
    /// Low and medium priority tasks do NOT qualify for age-based promotion.
    /// </summary>
    private static bool IsPromotableByAgeAndPriority(TodoTask task)
    {
        bool isHighOrCritical = task.Priority is TaskPriority.High or TaskPriority.Critical;
        if (!isHighOrCritical)
        {
            return false;
        }

        int ageDays = (int)(DateTimeOffset.UtcNow - task.CreatedAt).TotalDays;
        return ageDays >= AgeDaysThreshold;
    }

    /// <summary>
    /// Rule: A hard or epic difficulty task viewed 5+ times without progress should be promoted.
    /// "Without progress" means the task is still in Todo status (not InProgress or Done).
    /// </summary>
    private static bool IsPromotableByDifficultyAndAvoidance(TodoTask task)
    {
        bool isHardOrEpic = task.Difficulty is TaskDifficulty.Hard or TaskDifficulty.Epic;
        if (!isHardOrEpic)
        {
            return false;
        }

        return task.ViewCount >= AvoidanceViewThreshold && task.Status == TaskStatus.Todo;
    }
}
