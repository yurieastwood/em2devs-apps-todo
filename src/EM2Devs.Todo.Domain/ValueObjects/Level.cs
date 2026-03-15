namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Validated level value object with logarithmic XP thresholds (ADR-0002).
/// Enforces level bounds [1, MaxLevel] and tracks current XP within level.
/// </summary>
public sealed record Level
{
    public const int MaxLevel = 100;

    public int Value { get; }
    public ExperiencePoints CurrentXp { get; }

    public Level(int value, ExperiencePoints currentXp)
    {
        ArgumentNullException.ThrowIfNull(currentXp);

        if (value < 1)
        {
            throw new Exceptions.DomainException("Level must be at least 1.");
        }

        if (value > MaxLevel)
        {
            throw new Exceptions.DomainException($"Level cannot exceed {MaxLevel}.");
        }

        Value = value;
        CurrentXp = currentXp;
    }

    public static Level StartingLevel() => new(1, new ExperiencePoints(0));

    public Level AddXp(ExperiencePoints earned)
    {
        ArgumentNullException.ThrowIfNull(earned);

        if (Value >= MaxLevel)
        {
            return new Level(MaxLevel, CurrentXp.Add(earned));
        }

        int totalXp = CurrentXp.Value + earned.Value;
        int currentLevel = Value;

        while (currentLevel < MaxLevel)
        {
            int xpNeeded = XpForNextLevel(currentLevel);
            if (totalXp < xpNeeded)
            {
                break;
            }

            totalXp -= xpNeeded;
            currentLevel++;
        }

        return new Level(currentLevel, new ExperiencePoints(totalXp));
    }

    public int XpToNextLevel()
    {
        if (Value >= MaxLevel)
        {
            return 0;
        }

        return XpForNextLevel(Value) - CurrentXp.Value;
    }

    /// <summary>
    /// Returns the cumulative XP required to reach the given level from level 1.
    /// Matches the logarithmic curve from levelling.feature:
    /// Level 2=50, 5=300, 10=1000, 20=4000, 50=25000.
    /// Formula: round(12.5 * level^1.93) rounded to nearest clean boundary.
    /// </summary>
    public static int CumulativeXpRequired(int level)
    {
        if (level < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(level), level, "Cumulative XP is only defined for level 2 and above.");
        }

        return level switch
        {
            2 => 50,
            3 => 100,
            4 => 200,
            5 => 300,
            6 => 450,
            7 => 600,
            8 => 800,
            9 => 900,
            10 => 1_000,
            <= 20 => CalculateThreshold(level, 1_000, 4_000, 10, 20),
            <= 50 => CalculateThreshold(level, 4_000, 25_000, 20, 50),
            _ => CalculateThreshold(level, 25_000, 200_000, 50, MaxLevel)
        };
    }

    private static int XpForNextLevel(int currentLevel)
    {
        int cumulativeNext = CumulativeXpRequired(currentLevel + 1);
        int cumulativeCurrent = currentLevel >= 2 ? CumulativeXpRequired(currentLevel) : 0;
        return cumulativeNext - cumulativeCurrent;
    }

    private static int CalculateThreshold(int level, int startXp, int endXp, int startLevel, int endLevel)
    {
        double fraction = (double)(level - startLevel) / (endLevel - startLevel);
        double logStart = Math.Log(startXp);
        double logEnd = Math.Log(endXp);
        double interpolated = Math.Exp(logStart + fraction * (logEnd - logStart));

        int rounding = interpolated switch
        {
            < 500 => 50,
            < 5_000 => 100,
            _ => 1_000
        };

        return (int)(Math.Round(interpolated / rounding) * rounding);
    }
}
