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
    /// <summary>
    /// Fixed identifier for the single-user demo singleton profile row. Using a deterministic
    /// Id lets <c>PostgresPlayerProfileRepository.GetOrCreateAsync</c> rely on the primary-key
    /// constraint to arbitrate concurrent create-on-first-request races: the loser gets a
    /// <c>DbUpdateException</c>, the winner's row survives, and a retry read returns the same
    /// row. When auth lands and profiles become per-user, this constant goes away.
    /// </summary>
    public static readonly PlayerProfileId SingletonId =
        new(new Guid("01010101-0101-0101-0101-010101010101"));

    public PlayerProfileId Id { get; }
    public Level Level { get; private set; }
    public Streak Streak { get; private set; }
    public int LongestStreak { get; private set; }

    private PlayerProfile(PlayerProfileId id, Level level, Streak streak, int longestStreak)
    {
        Id = id;
        Level = level;
        Streak = streak;
        LongestStreak = longestStreak;
    }

    /// <summary>
    /// EF Core constructor. Owned-type properties (<see cref="Level"/>, <see cref="Streak"/>)
    /// cannot be bound to constructor parameters in EF Core, so they're populated via private
    /// setters after construction. Initialised here to satisfy non-null reference defaults.
    /// </summary>
    // Stryker disable all : EF Core materialisation constructor — not reachable from domain tests
    private PlayerProfile(PlayerProfileId id, int longestStreak)
    {
        Id = id;
        Level = Level.StartingLevel();
        Streak = Streak.NewStreak();
        LongestStreak = longestStreak;
    }
    // Stryker restore all

    public static PlayerProfile NewProfile() =>
        new(SingletonId, Level.StartingLevel(), Streak.NewStreak(), longestStreak: 0);

    public static PlayerProfile Reconstitute(PlayerProfileId id, Level level, Streak streak, int longestStreak) =>
        new(id, level, streak, longestStreak);

    public void AwardXp(ExperiencePoints xp)
    {
        ArgumentNullException.ThrowIfNull(xp);
        Level = Level.AddXp(xp);
    }

    public void RecordCompletion(DateOnly completionDate)
    {
        Streak = Streak.RecordCompletion(completionDate);
        LongestStreak = Math.Max(LongestStreak, Streak.CurrentDays);
    }

    public void ProcessDayEnd(DateOnly evaluationDate)
    {
        Streak = Streak.ProcessDayEnd(evaluationDate);
    }
}
