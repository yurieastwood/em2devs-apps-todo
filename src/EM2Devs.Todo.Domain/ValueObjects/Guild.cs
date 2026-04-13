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

    public GuildId Id { get; }
    public string Name { get; }
    public string Description { get; }
    public bool IsDisbanded { get; }
    public IReadOnlyList<GuildMember> Members => _members.AsReadOnly();
    public int MemberCount => _members.Count;

    public Guild(GuildId id, string name, string description, IEnumerable<GuildMember> members, bool isDisbanded = false)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));

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

        if (!isDisbanded && !_members.Exists(m => m.Role == GuildRole.Leader))
        {
            throw new Exceptions.DomainException("Guild must have a leader.");
        }

        Name = name;
        Description = description ?? string.Empty;
        IsDisbanded = isDisbanded;
    }

    /// <summary>
    /// Backward-compatible constructor without GuildId (generates a new one).
    /// </summary>
    public Guild(string name, string description, IEnumerable<GuildMember> members)
        : this(GuildId.New(), name, description, members)
    {
    }

    public static Guild Create(string name, string description, Guid leaderId, DateOnly today)
    {
        var leader = new GuildMember(leaderId, GuildRole.Leader, today);
        return new Guild(GuildId.New(), name, description, [leader]);
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
        return new Guild(Id, Name, Description, updated);
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
        return new Guild(Id, Name, Description, updated);
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

        return new Guild(Id, Name, Description, updated);
    }

    /// <summary>
    /// A non-leader member leaves the guild voluntarily.
    /// </summary>
    public Guild Leave(Guid userId)
    {
        GuildMember? member = _members.Find(m => m.UserId == userId);

        if (member is null)
        {
            throw new Exceptions.DomainException("User is not a guild member.");
        }

        if (member.Role == GuildRole.Leader)
        {
            throw new Exceptions.DomainException(
                "Leader cannot leave without transferring leadership first.");
        }

        List<GuildMember> updated = _members.Where(m => m.UserId != userId).ToList();
        return new Guild(Id, Name, Description, updated);
    }

    /// <summary>
    /// Leader leaves the guild after transferring leadership to the longest-serving member.
    /// </summary>
    public Guild LeaderLeave()
    {
        if (_members.Count == 1)
        {
            throw new Exceptions.DomainException(
                "Cannot leave as last member. Disband the guild instead.");
        }

        Guid oldLeaderId = LeaderId;
        List<GuildMember> nonLeaders = _members
            .Where(m => m.Role != GuildRole.Leader)
            .OrderBy(m => m.JoinedOn)
            .ToList();
        GuildMember longestServing = nonLeaders[0];

        List<GuildMember> updated = [];
        foreach (GuildMember m in _members)
        {
            if (m.UserId == oldLeaderId)
            {
                continue; // remove old leader
            }

            if (m.UserId == longestServing.UserId)
            {
                updated.Add(new GuildMember(m.UserId, GuildRole.Leader, m.JoinedOn));
            }
            else
            {
                updated.Add(m);
            }
        }

        return new Guild(Id, Name, Description, updated);
    }

    /// <summary>
    /// Disband the guild. Only the leader can disband.
    /// </summary>
    public Guild Disband(Guid requesterId)
    {
        if (requesterId != LeaderId)
        {
            throw new Exceptions.DomainException(
                "Only the guild leader can disband the guild.");
        }

        return new Guild(Id, Name, Description, _members, isDisbanded: true);
    }

    /// <summary>
    /// Update guild name and/or description. Only the leader can do this.
    /// </summary>
    public Guild UpdateDetails(Guid requesterId, string newName, string newDescription)
    {
        if (requesterId != LeaderId)
        {
            throw new Exceptions.DomainException(
                "Only the guild leader can edit guild details.");
        }

        return new Guild(Id, newName, newDescription, _members);
    }

    /// <summary>
    /// Generate an invite link for this guild.
    /// </summary>
    public GuildInviteLink GenerateInviteLink(DateOnly today)
    {
        return GuildInviteLink.Create(Id, today);
    }

    /// <summary>
    /// Accept an invite link and add the user to the guild.
    /// </summary>
    public Guild AcceptInvite(GuildInviteLink invite, Guid userId, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(invite);

        if (invite.GuildId != Id)
        {
            throw new Exceptions.DomainException("Invite link does not belong to this guild.");
        }

        if (invite.IsExpired(today))
        {
            throw new Exceptions.DomainException("Invite link has expired.");
        }

        return AddMember(userId, today);
    }

    public Guid LeaderId =>
        _members.Find(m => m.Role == GuildRole.Leader)!.UserId;

    public bool IsMember(Guid userId) =>
        _members.Exists(m => m.UserId == userId);

    public bool IsAtCapacity => _members.Count >= MaxMembers;
}
