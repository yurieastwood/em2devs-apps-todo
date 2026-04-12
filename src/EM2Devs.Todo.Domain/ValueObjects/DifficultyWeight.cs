namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Maps task difficulty levels to capacity unit weights.
/// Trivial=1, Easy=2, Normal=3, Hard=5, Epic=8 (Fibonacci-inspired).
/// Used by the capacity model to measure workload in weighted units.
/// </summary>
public static class DifficultyWeight
{
    public const int Trivial = 1;
    public const int Easy = 2;
    public const int Normal = 3;
    public const int Hard = 5;
    public const int Epic = 8;

    public static int For(TaskDifficulty difficulty) => difficulty switch
    {
        TaskDifficulty.Trivial => Trivial,
        TaskDifficulty.Easy => Easy,
        TaskDifficulty.Normal => Normal,
        TaskDifficulty.Hard => Hard,
        TaskDifficulty.Epic => Epic,
        _ => Normal,
    };
}
