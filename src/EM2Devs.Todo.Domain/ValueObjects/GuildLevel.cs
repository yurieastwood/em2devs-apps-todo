namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Guild level with simple XP thresholds for guild progression.
/// Guilds level up at fixed XP intervals.
/// </summary>
public sealed record GuildLevel
{
    public const int MaxLevel = 50;
    public const int XpPerLevel = 500;

    public int Value { get; }
    public int CurrentXp { get; }

    public GuildLevel(int value, int currentXp)
    {
        if (value < 1)
        {
            throw new Exceptions.DomainException("Guild level must be at least 1.");
        }

        if (value > MaxLevel)
        {
            throw new Exceptions.DomainException($"Guild level cannot exceed {MaxLevel}.");
        }

        if (currentXp < 0)
        {
            throw new Exceptions.DomainException("Guild current XP cannot be negative.");
        }

        Value = value;
        CurrentXp = currentXp;
    }

    public static GuildLevel Starting() => new(1, 0);

    /// <summary>
    /// Add XP and potentially level up. Returns the new guild level and whether a level-up occurred.
    /// </summary>
    public (GuildLevel Level, bool LevelledUp) AddXp(int amount)
    {
        if (amount <= 0)
        {
            throw new Exceptions.DomainException("XP amount must be positive.");
        }

        int totalXp = CurrentXp + amount;
        int currentLevel = Value;
        bool levelledUp = false;

        while (currentLevel < MaxLevel && totalXp >= XpPerLevel)
        {
            totalXp -= XpPerLevel;
            currentLevel++;
            levelledUp = true;
        }

        return (new GuildLevel(currentLevel, totalXp), levelledUp);
    }

    /// <summary>
    /// XP remaining until next level.
    /// </summary>
    public int XpToNextLevel()
    {
        if (Value >= MaxLevel)
        {
            return 0;
        }

        return XpPerLevel - CurrentXp;
    }
}
