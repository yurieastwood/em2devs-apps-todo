using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// Persistent player progression aggregate.
/// Owns the player's Level (XP) and Streak. Tracks longest streak as historical state.
/// Replaces the previous in-memory state held inside InMemoryPlayerProfileRepository.
/// Slice 3 multi-user isolation: each authenticated user owns exactly one PlayerProfile
/// identified by <see cref="UserId"/>. The singleton-row pattern used before Slice 3
/// is gone; concurrent create-on-first-request races are now arbitrated by the unique
/// index on <c>user_id</c> in <c>player_profiles</c>.
/// </summary>
public sealed class PlayerProfile
{
    public PlayerProfileId Id { get; }

    /// <summary>
    /// The authenticated user who owns this profile. Required and immutable.
    /// A unique index on this column enforces exactly one profile per user.
    /// </summary>
    public Guid UserId { get; private set; }

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

    private PlayerProfile(PlayerProfileId id, Guid userId, Level level, Streak streak, int longestStreak)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId cannot be empty.");
        }

        Id = id;
        UserId = userId;
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

    public static PlayerProfile NewProfile(Guid userId) =>
        new(PlayerProfileId.New(), userId, Level.StartingLevel(), Streak.NewStreak(), longestStreak: 0);

    public static PlayerProfile Reconstitute(
        PlayerProfileId id, Guid userId, Level level, Streak streak, int longestStreak) =>
        new(id, userId, level, streak, longestStreak);

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
    /// Activates a streak freeze for the specified duration starting today.
    /// Delegates to <see cref="Streak.Freeze"/>; throws <see cref="DomainException"/>
    /// if the streak is already frozen.
    /// </summary>
    public void FreezeStreak(DateOnly today, int days)
    {
        Streak = Streak.Freeze(today, days);
    }

    /// <summary>
    /// Manually ends an active streak freeze. No-op if not frozen.
    /// </summary>
    public void UnfreezeStreak(DateOnly today)
    {
        Streak = Streak.Unfreeze(today);
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
            throw new DomainException(
                $"Skill tree '{type}' has not been discovered yet.");
        }

        _skillTrees[index] = _skillTrees[index].RecordTaskCompletion();
    }
}
