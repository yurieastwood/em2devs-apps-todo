using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Pure domain service that sorts tasks by energy match.
/// High-energy tasks surface during peak/high energy; low-energy tasks during low energy.
/// Reordering never removes tasks from the list — it only changes sort order.
/// </summary>
public static class EnergyTaskSorter
{
    /// <summary>
    /// Sorts tasks so that tasks matching the current energy level appear first.
    /// The distance between the task's difficulty and the energy level determines order:
    /// closer matches come first, further matches come last.
    /// When two tasks have the same distance, higher-energy levels prefer harder tasks
    /// and lower-energy levels prefer easier tasks.
    /// All tasks are preserved — none are removed.
    /// </summary>
    public static List<TodoTask> SortByEnergyMatch(IReadOnlyList<TodoTask> tasks, EnergyLevel currentEnergy)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        int targetRank = GetTargetDifficultyRank(currentEnergy);

        return tasks
            .OrderBy(task => Math.Abs(GetDifficultyRank(task.Difficulty) - targetRank))
            .ThenBy(task => currentEnergy >= EnergyLevel.High
                ? -GetDifficultyRank(task.Difficulty)
                : GetDifficultyRank(task.Difficulty))
            .ToList();
    }

    /// <summary>
    /// Maps energy level to a target difficulty rank.
    /// Peak/High energy -> prefer Hard/Epic tasks (high rank)
    /// Medium energy -> prefer Normal tasks (middle rank)
    /// Low energy -> prefer Trivial/Easy tasks (low rank)
    /// </summary>
    private static int GetTargetDifficultyRank(EnergyLevel energy)
    {
        return energy switch
        {
            EnergyLevel.Peak => 4,
            EnergyLevel.High => 3,
            EnergyLevel.Medium => 2,
            EnergyLevel.Low => 0,
            _ => 2
        };
    }

    /// <summary>
    /// Maps task difficulty to a numeric rank for comparison.
    /// </summary>
    private static int GetDifficultyRank(TaskDifficulty difficulty)
    {
        return difficulty switch
        {
            TaskDifficulty.Trivial => 0,
            TaskDifficulty.Easy => 1,
            TaskDifficulty.Normal => 2,
            TaskDifficulty.Hard => 3,
            TaskDifficulty.Epic => 4,
            _ => 2
        };
    }
}
