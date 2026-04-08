using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// Persistent player progression aggregate.
/// Owns the player's Level (XP) and Streak. Tracks longest streak as historical state.
/// Replaces the previous in-memory state held inside InMemoryPlayerProfileRepository.
/// Single-user demo mode: no UserId yet — added when auth lands.
/// </summary>
public sealed class PlayerProfile
{
    public int Id { get; private set; }
    public Level Level { get; private set; }
    public Streak Streak { get; private set; }
    public int LongestStreak { get; private set; }

    // EF Core requires a parameterless constructor for reconstruction.
    private PlayerProfile()
    {
        Level = Level.StartingLevel();
        Streak = Streak.NewStreak();
    }

    private PlayerProfile(Level level, Streak streak, int longestStreak)
    {
        Level = level;
        Streak = streak;
        LongestStreak = longestStreak;
    }

    public static PlayerProfile NewProfile() =>
        new(Level.StartingLevel(), Streak.NewStreak(), longestStreak: 0);

    public static PlayerProfile Reconstitute(Level level, Streak streak, int longestStreak) =>
        new(level, streak, longestStreak);

    public void AwardXp(ExperiencePoints xp)
    {
        ArgumentNullException.ThrowIfNull(xp);
        Level = Level.AddXp(xp);
    }

    public void RecordCompletion(DateOnly today)
    {
        Streak = Streak.RecordCompletion(today);
        if (Streak.CurrentDays > LongestStreak)
        {
            LongestStreak = Streak.CurrentDays;
        }
    }

    public void ProcessDayEnd(DateOnly endOfDay)
    {
        Streak = Streak.ProcessDayEnd(endOfDay);
    }
}
