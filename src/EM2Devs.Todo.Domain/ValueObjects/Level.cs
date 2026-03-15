namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Validated level value object with logarithmic XP thresholds (ADR-0002).
/// Enforces level bounds [1, MaxLevel] and tracks current XP within level.
/// </summary>
public sealed record Level
{
    public const int MaxLevel = 100;

    /// <summary>
    /// Pre-computed cumulative XP thresholds for levels 2–100.
    /// Index 0 = level 2, index 98 = level 100.
    /// Matches feature-specified anchors: L2=50, L5=300, L10=1000, L20=4000, L50=25000.
    /// Intermediate values follow a logarithmic interpolation curve.
    /// </summary>
    private static readonly int[] _thresholds = BuildThresholds();

    public int Value { get; }
    public ExperiencePoints CurrentXp { get; }

    /// <remarks>
    /// CurrentXp is not validated against the level threshold to allow
    /// reconstitution from persistence without coupling to threshold logic.
    /// </remarks>
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
    /// </summary>
    public static int CumulativeXpRequired(int level)
    {
        if (level < 2 || level - 2 >= _thresholds.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(level), level, "Cumulative XP is only defined for levels 2 through 100.");
        }

        return _thresholds[level - 2];
    }

    private static int XpForNextLevel(int currentLevel)
    {
        int cumulativeNext = CumulativeXpRequired(currentLevel + 1);
        int cumulativeCurrent = currentLevel >= 2 ? CumulativeXpRequired(currentLevel) : 0;
        return cumulativeNext - cumulativeCurrent;
    }

    private static int[] BuildThresholds()
    {
        // Anchor points from levelling.feature
        int[] anchors = [50, 100, 200, 300, 450, 600, 800, 900, 1_000];

        int[] result = new int[MaxLevel - 1]; // levels 2 through MaxLevel
        anchors.CopyTo(result, 0);

        // Interpolate ranges: 11–20 (1000→4000), 21–50 (4000→25000), 51–100 (25000→200000)
        int[][] ranges =
        [
            [10, 20, 1_000, 4_000],
            [20, 50, 4_000, 25_000],
            [50, MaxLevel, 25_000, 200_000]
        ];

        foreach (int[] range in ranges)
        {
            int startLevel = range[0];
            int endLevel = range[1];
            int startXp = range[2];
            int endXp = range[3];
            double logStart = Math.Log(startXp);
            double logEnd = Math.Log(endXp);

            for (int level = startLevel + 1; level <= endLevel; level++)
            {
                double fraction = (double)(level - startLevel) / (endLevel - startLevel);
                result[level - 2] = (int)Math.Round(Math.Exp(logStart + fraction * (logEnd - logStart)));
            }
        }

        return result;
    }
}
