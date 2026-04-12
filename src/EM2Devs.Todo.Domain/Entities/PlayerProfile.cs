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
    public TitleInventory TitleInventory { get; private set; }
    public XpHistory XpHistory { get; private set; }

    private readonly List<SkillTree> _skillTrees = [];

    /// <summary>
    /// The player's discovered skill trees. Progress is permanent and never decays.
    /// </summary>
    public IReadOnlyList<SkillTree> SkillTrees => _skillTrees.AsReadOnly();

    private PlayerProfile(PlayerProfileId id, Level level, Streak streak, int longestStreak)
    {
        Id = id;
        Level = level;
        Streak = streak;
        LongestStreak = longestStreak;
        TitleInventory = TitleInventory.Empty();
        XpHistory = XpHistory.Empty();
    }

    /// <summary>
    /// EF Core constructor. Owned-type properties (<see cref="Level"/>, <see cref="Streak"/>)
    /// cannot be bound to constructor parameters in EF Core, so they're populated via private
    /// setters after construction. Initialised here to satisfy non-null reference defaults.
    /// </summary>
    private PlayerProfile(PlayerProfileId id, int longestStreak)
    {
        Id = id;
        Level = Level.StartingLevel();
        Streak = Streak.NewStreak();
        LongestStreak = longestStreak;
        TitleInventory = TitleInventory.Empty();
        XpHistory = XpHistory.Empty();
    }

    public static PlayerProfile NewProfile() =>
        new(SingletonId, Level.StartingLevel(), Streak.NewStreak(), longestStreak: 0);

    public static PlayerProfile Reconstitute(PlayerProfileId id, Level level, Streak streak, int longestStreak) =>
        new(id, level, streak, longestStreak);

    public void AwardXp(ExperiencePoints xp)
    {
        ArgumentNullException.ThrowIfNull(xp);
        Level = Level.AddXp(xp);
    }

    /// <summary>
    /// Records an XP earning event in the player's history with date, amount, and source.
    /// Used for displaying XP history over time with daily totals and source breakdowns.
    /// </summary>
    public void RecordXpEarning(DateOnly date, ExperiencePoints xp, string source)
    {
        XpHistory = XpHistory.RecordXpEarning(date, xp, source);
    }

    /// <summary>
    /// Awards a title to the player's inventory. Idempotent — re-awarding
    /// an already-earned title is a no-op. Null validation is enforced by
    /// <see cref="TitleInventory.AwardTitle"/>.
    /// </summary>
    public void AwardTitle(Title title)
    {
        TitleInventory = TitleInventory.AwardTitle(title);
    }

    /// <summary>
    /// Selects the active title displayed on the player's public profile.
    /// The title must already be earned.
    /// </summary>
    public void SelectActiveTitle(TitleType type)
    {
        TitleInventory = TitleInventory.SelectActiveTitle(type);
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

    /// <summary>
    /// Discovers (unlocks) a skill tree for the player. Idempotent — re-discovering
    /// an already-unlocked tree type is a no-op.
    /// </summary>
    public void DiscoverSkillTree(SkillTreeType type)
    {
        if (_skillTrees.Any(t => t.Type == type))
        {
            return;
        }

        _skillTrees.Add(SkillTree.Discover(type));
    }

    /// <summary>
    /// Records a qualifying task completion for the given skill tree type,
    /// advancing tier progress. The tree must already be discovered.
    /// </summary>
    public void RecordSkillTreeProgress(SkillTreeType type)
    {
        int index = _skillTrees.FindIndex(t => t.Type == type);
        if (index < 0)
        {
            throw new Exceptions.DomainException(
                $"Skill tree '{type}' has not been discovered yet.");
        }

        _skillTrees[index] = _skillTrees[index].RecordTaskCompletion();
    }
}
