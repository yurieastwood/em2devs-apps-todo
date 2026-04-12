namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Validated experience points value object (ADR-0002).
/// Enforces non-negative XP on construction.
/// </summary>
public sealed record ExperiencePoints
{
    public int Value { get; }

    public ExperiencePoints(int value)
    {
        if (value < 0)
        {
            throw new Exceptions.DomainException("Experience points cannot be negative.");
        }

        Value = value;
    }

    public ExperiencePoints Add(ExperiencePoints other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new ExperiencePoints(Value + other.Value);
    }

    public static ExperiencePoints BaseForDifficulty(TaskDifficulty difficulty)
    {
        int baseXp = difficulty switch
        {
            TaskDifficulty.Trivial => 8,
            TaskDifficulty.Easy => 15,
            TaskDifficulty.Normal => 30,
            TaskDifficulty.Hard => 60,
            TaskDifficulty.Epic => 115,
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Unknown task difficulty.")
        };

        return new ExperiencePoints(baseXp);
    }
}
