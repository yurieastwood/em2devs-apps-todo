namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Guild entity for small group collaboration (2-12 members).
/// Manages membership, leadership, and capacity.
/// </summary>
public sealed record Guild
{
    public const int MaxMembers = 12;
    public const int MaxGuildsPerLeader = 3;

    /// <summary>XP bonus awarded to each contributor when a guild quest is completed.</summary>
    public const int QuestCompletionBonusXp = 100;

    /// <summary>XP awarded per individual guild task completion.</summary>
    public const int TaskCompletionXp = 25;

    private readonly List<GuildMember> _members;
    private readonly List<GuildQuest> _quests;
    private readonly List<GuildFeedItem> _feedItems;

    public GuildId Id { get; }
    public string Name { get; }
    public string Description { get; }
    public bool IsDisbanded { get; }
    public GuildXp Xp { get; }
    public GuildLevel Level { get; }
    public IReadOnlyList<GuildMember> Members => _members.AsReadOnly();
    public IReadOnlyList<GuildQuest> Quests => _quests.AsReadOnly();
    public IReadOnlyList<GuildFeedItem> FeedItems => _feedItems.AsReadOnly();
    public int MemberCount => _members.Count;
    public IReadOnlyList<GuildQuest> ActiveQuests => _quests.Where(q => !q.IsCompleted).ToList().AsReadOnly();

    public Guild(GuildId id, string name, string description, IEnumerable<GuildMember> members,
        bool isDisbanded = false,
        GuildXp? xp = null,
        GuildLevel? level = null,
        IEnumerable<GuildQuest>? quests = null,
        IEnumerable<GuildFeedItem>? feedItems = null)
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
        Xp = xp ?? GuildXp.Zero();
        Level = level ?? GuildLevel.Starting();
        _quests = quests?.ToList() ?? [];
        _feedItems = feedItems?.ToList() ?? [];
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
        return new Guild(Id, Name, Description, updated, xp: Xp, level: Level, quests: _quests, feedItems: _feedItems);
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

        // Unassign the removed member's in-progress quest tasks
        List<GuildQuest> updatedQuests = _quests
            .Select(q => q.UnassignTasksForUser(userId))
            .ToList();

        return new Guild(Id, Name, Description, updated, xp: Xp, level: Level, quests: updatedQuests, feedItems: _feedItems);
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

        return new Guild(Id, Name, Description, updated, xp: Xp, level: Level, quests: _quests, feedItems: _feedItems);
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
        return new Guild(Id, Name, Description, updated, xp: Xp, level: Level, quests: _quests, feedItems: _feedItems);
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

        return new Guild(Id, Name, Description, updated, xp: Xp, level: Level, quests: _quests, feedItems: _feedItems);
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

        return new Guild(Id, Name, Description, _members, isDisbanded: true, xp: Xp, level: Level, quests: _quests, feedItems: _feedItems);
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

        return new Guild(Id, newName, newDescription, _members, xp: Xp, level: Level, quests: _quests, feedItems: _feedItems);
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

    /// <summary>
    /// Create a guild quest and add it to the quest board.
    /// </summary>
    public Guild CreateQuest(string title, string description, DateOnly? dueDate, IEnumerable<GuildTask> tasks)
    {
        var quest = new GuildQuest(GuildQuestId.New(), title, description, dueDate, tasks);
        List<GuildQuest> updatedQuests = [.. _quests, quest];
        return new Guild(Id, Name, Description, _members, xp: Xp, level: Level, quests: updatedQuests, feedItems: _feedItems);
    }

    /// <summary>
    /// Complete a task within a guild quest. Awards guild XP and updates the feed.
    /// If the quest is fully completed, awards bonus XP and a quest completion feed item.
    /// </summary>
    public Guild CompleteQuestTask(GuildQuestId questId, GuildTaskId taskId, Guid completedByUserId, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(questId);

        GuildQuest? quest = _quests.Find(q => q.Id == questId);
        if (quest is null)
        {
            throw new Exceptions.DomainException("Quest not found in this guild.");
        }

        GuildQuest updatedQuest = quest.CompleteTask(taskId);
        List<GuildQuest> updatedQuests = _quests
            .Select(q => q.Id == questId ? updatedQuest : q)
            .ToList();

        // Award task completion XP
        GuildXp updatedXp = Xp.AddXp(TaskCompletionXp, completedByUserId);

        List<GuildFeedItem> updatedFeed = [.. _feedItems];
        updatedFeed.Add(new GuildFeedItem(
            GuildFeedEventType.TaskCompleted,
            completedByUserId,
            $"Completed task in quest \"{quest.Title}\"",
            now));

        // Check if quest is now complete
        GuildLevel updatedLevel = Level;
        if (updatedQuest.IsCompleted)
        {
            // Award bonus XP to the guild for quest completion
            updatedXp = updatedXp.AddXp(QuestCompletionBonusXp, completedByUserId);

            updatedFeed.Add(new GuildFeedItem(
                GuildFeedEventType.QuestCompleted,
                completedByUserId,
                $"Guild quest \"{quest.Title}\" completed!",
                now));
        }

        // Check for level up
        (GuildLevel newLevel, bool levelledUp) = updatedLevel.AddXp(updatedXp.TotalXp - Xp.TotalXp);
        updatedLevel = newLevel;

        if (levelledUp)
        {
            updatedFeed.Add(new GuildFeedItem(
                GuildFeedEventType.GuildLevelUp,
                completedByUserId,
                $"Guild levelled up to level {updatedLevel.Value}!",
                now));
        }

        return new Guild(Id, Name, Description, _members, xp: updatedXp, level: updatedLevel, quests: updatedQuests, feedItems: updatedFeed);
    }

    /// <summary>
    /// Add a feed item for member joining.
    /// </summary>
    public Guild AddMemberWithFeed(Guid userId, DateOnly today, DateTimeOffset now, TitleType? activeTitle = null)
    {
        Guild updated = AddMember(userId, today, activeTitle);
        List<GuildFeedItem> updatedFeed = [.. updated._feedItems];
        updatedFeed.Add(new GuildFeedItem(
            GuildFeedEventType.MemberJoined,
            userId,
            "Joined the guild",
            now));
        return new Guild(updated.Id, updated.Name, updated.Description, updated._members,
            xp: updated.Xp, level: updated.Level, quests: updated._quests, feedItems: updatedFeed);
    }

    /// <summary>
    /// Remove a member and add feed item.
    /// </summary>
    public Guild RemoveMemberWithFeed(Guid userId, DateTimeOffset now)
    {
        Guild updated = RemoveMember(userId);
        List<GuildFeedItem> updatedFeed = [.. updated._feedItems];
        updatedFeed.Add(new GuildFeedItem(
            GuildFeedEventType.MemberRemoved,
            userId,
            "Was removed from the guild",
            now));
        return new Guild(updated.Id, updated.Name, updated.Description, updated._members,
            xp: updated.Xp, level: updated.Level, quests: updated._quests, feedItems: updatedFeed);
    }
}
