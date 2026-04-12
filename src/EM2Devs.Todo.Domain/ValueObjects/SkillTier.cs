namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Validated skill tree tier (1–3).
/// </summary>
public sealed record SkillTier
{
    public const int MaxTier = 3;

    public int Value { get; }

    public SkillTier(int value)
    {
        if (value < 1 || value > MaxTier)
        {
            throw new Exceptions.DomainException(
                $"Skill tier must be between 1 and {MaxTier}.");
        }

        Value = value;
    }
}
