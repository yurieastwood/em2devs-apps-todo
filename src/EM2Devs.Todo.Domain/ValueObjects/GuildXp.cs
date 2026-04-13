namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Guild XP tracking with per-member contributions.
/// </summary>
public sealed record GuildXp
{
    private readonly Dictionary<Guid, int> _contributions;

    public int TotalXp { get; }
    public IReadOnlyDictionary<Guid, int> MemberContributions => _contributions.AsReadOnly();

    public GuildXp(int totalXp, IReadOnlyDictionary<Guid, int>? contributions = null)
    {
        if (totalXp < 0)
        {
            throw new Exceptions.DomainException("Guild XP cannot be negative.");
        }

        TotalXp = totalXp;
        _contributions = contributions != null
            ? new Dictionary<Guid, int>(contributions)
            : new Dictionary<Guid, int>();
    }

    public static GuildXp Zero() => new(0);

    /// <summary>
    /// Add XP contributed by a specific member.
    /// </summary>
    public GuildXp AddXp(int amount, Guid contributorUserId)
    {
        if (amount <= 0)
        {
            throw new Exceptions.DomainException("XP amount must be positive.");
        }

        if (contributorUserId == Guid.Empty)
        {
            throw new Exceptions.DomainException("Contributor user ID cannot be empty.");
        }

        var updated = new Dictionary<Guid, int>(_contributions);
        if (updated.TryGetValue(contributorUserId, out int existing))
        {
            updated[contributorUserId] = existing + amount;
        }
        else
        {
            updated[contributorUserId] = amount;
        }

        return new GuildXp(TotalXp + amount, updated);
    }

    /// <summary>
    /// Get a specific member's contribution.
    /// </summary>
    public int ContributionFor(Guid userId)
    {
        return _contributions.TryGetValue(userId, out int value) ? value : 0;
    }
}
