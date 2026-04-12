namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Guild entity for small group collaboration (2-12 members).
/// Manages membership, leadership, and capacity.
/// </summary>
public sealed record Guild
{
    public const int MaxMembers = 12;
    public const int MaxGuildsPerLeader = 3;

    private readonly List<GuildMember> _members;

    public string Name { get; }
    public string Description { get; }
    public IReadOnlyList<GuildMember> Members => _members.AsReadOnly();
    public int MemberCount => _members.Count;

    public Guild(string name, string description, IEnumerable<GuildMember> members)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new Exceptions.DomainException("Guild name cannot be empty.");
        }

        if (name.Length > 50)
        {
            throw new Exceptions.DomainException("Guild name cannot exceed 50 characters.");
        }

        _members = members?.ToList()
            ?? throw new ArgumentNullException(nameof(members));

        if (_members.Count > MaxMembers)
        {
            throw new Exceptions.DomainException(
                $"Guild cannot have more than {MaxMembers} members.");
        }

        if (!_members.Exists(m => m.Role == GuildRole.Leader))
        {
            throw new Exceptions.DomainException("Guild must have a leader.");
        }

        Name = name;
        Description = description ?? string.Empty;
    }

    public static Guild Create(string name, string description, Guid leaderId, DateOnly today)
    {
        var leader = new GuildMember(leaderId, GuildRole.Leader, today);
        return new Guild(name, description, [leader]);
    }

    public Guild AddMember(Guid userId, DateOnly today, TitleType? activeTitle = null)
    {
        if (_members.Count >= MaxMembers)
        {
            throw new Exceptions.DomainException(
                $"Guild is at maximum capacity of {MaxMembers} members.");
        }

        if (_members.Exists(m => m.UserId == userId))
        {
            throw new Exceptions.DomainException("User is already a guild member.");
        }

        var newMember = new GuildMember(userId, GuildRole.Member, today, activeTitle);
        List<GuildMember> updated = [.. _members, newMember];
        return new Guild(Name, Description, updated);
    }

    public Guild RemoveMember(Guid userId)
    {
        GuildMember? member = _members.Find(m => m.UserId == userId);

        if (member is null)
        {
            throw new Exceptions.DomainException("User is not a guild member.");
        }

        if (member.Role == GuildRole.Leader)
        {
            throw new Exceptions.DomainException(
                "Cannot remove the guild leader. Transfer leadership first.");
        }

        List<GuildMember> updated = _members.Where(m => m.UserId != userId).ToList();
        return new Guild(Name, Description, updated);
    }

    public Guild TransferLeadership(Guid newLeaderId)
    {
        GuildMember? newLeader = _members.Find(m => m.UserId == newLeaderId);

        if (newLeader is null)
        {
            throw new Exceptions.DomainException(
                "New leader must be an existing guild member.");
        }

        Guid oldLeaderId = LeaderId;
        List<GuildMember> updated = [];
        foreach (GuildMember m in _members)
        {
            if (m.UserId == newLeaderId)
            {
                updated.Add(new GuildMember(m.UserId, GuildRole.Leader, m.JoinedOn));
            }
            else if (m.UserId == oldLeaderId)
            {
                updated.Add(new GuildMember(m.UserId, GuildRole.Member, m.JoinedOn));
            }
            else
            {
                updated.Add(m);
            }
        }

        return new Guild(Name, Description, updated);
    }

    public Guid LeaderId =>
        _members.Find(m => m.Role == GuildRole.Leader)!.UserId;

    public bool IsMember(Guid userId) =>
        _members.Exists(m => m.UserId == userId);

    public bool IsAtCapacity => _members.Count >= MaxMembers;
}
