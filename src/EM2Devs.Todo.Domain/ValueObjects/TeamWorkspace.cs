using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a team workspace that groups users under a team subscription.
/// Manages members, capacity, and team-specific feature access.
/// </summary>
public sealed record TeamWorkspace
{
    public const int DefaultMaxMembers = 25;

    private readonly List<TeamMember> _members;

    public TeamWorkspaceId Id { get; }
    public string Name { get; }
    public Guid OwnerId { get; }
    public int MaxMembers { get; }
    public bool IsActive { get; }
    public IReadOnlyList<TeamMember> Members => _members.AsReadOnly();
    public int MemberCount => _members.Count;

    public TeamWorkspace(TeamWorkspaceId id, string name, Guid ownerId, int maxMembers,
        IEnumerable<TeamMember> members, bool isActive = true)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Team workspace name cannot be empty.");
        }

        if (name.Length > 100)
        {
            throw new DomainException("Team workspace name cannot exceed 100 characters.");
        }

        if (ownerId == Guid.Empty)
        {
            throw new DomainException("Owner ID cannot be empty.");
        }

        if (maxMembers < 1)
        {
            throw new DomainException("Maximum members must be at least 1.");
        }

        _members = members?.ToList() ?? throw new ArgumentNullException(nameof(members));

        if (_members.Count > maxMembers)
        {
            throw new DomainException($"Team cannot have more than {maxMembers} members.");
        }

        Name = name;
        OwnerId = ownerId;
        MaxMembers = maxMembers;
        IsActive = isActive;
    }

    /// <summary>
    /// Creates a new team workspace with the owner as the first admin member.
    /// </summary>
    public static TeamWorkspace Create(string name, Guid ownerId, int maxMembers = DefaultMaxMembers)
    {
        var owner = new TeamMember(ownerId, TeamRole.Admin, DateTimeOffset.UtcNow);
        return new TeamWorkspace(TeamWorkspaceId.New(), name, ownerId, maxMembers, [owner]);
    }

    /// <summary>
    /// Adds a member to the workspace.
    /// </summary>
    public TeamWorkspace AddMember(Guid userId, DateTimeOffset joinedAt)
    {
        if (_members.Count >= MaxMembers)
        {
            throw new DomainException($"Team workspace is at maximum capacity of {MaxMembers} members.");
        }

        if (_members.Exists(m => m.UserId == userId))
        {
            throw new DomainException("User is already a team member.");
        }

        var newMember = new TeamMember(userId, TeamRole.Member, joinedAt);
        List<TeamMember> updated = [.. _members, newMember];
        return new TeamWorkspace(Id, Name, OwnerId, MaxMembers, updated, IsActive);
    }

    /// <summary>
    /// Removes a member from the workspace.
    /// </summary>
    public TeamWorkspace RemoveMember(Guid userId)
    {
        if (userId == OwnerId)
        {
            throw new DomainException("Cannot remove the workspace owner.");
        }

        if (!_members.Exists(m => m.UserId == userId))
        {
            throw new DomainException("User is not a team member.");
        }

        List<TeamMember> updated = _members.Where(m => m.UserId != userId).ToList();
        return new TeamWorkspace(Id, Name, OwnerId, MaxMembers, updated, IsActive);
    }

    /// <summary>
    /// Deactivates the workspace (e.g., on subscription cancellation).
    /// </summary>
    public TeamWorkspace Deactivate()
    {
        return new TeamWorkspace(Id, Name, OwnerId, MaxMembers, _members, isActive: false);
    }

    /// <summary>
    /// Whether the workspace has room for more members.
    /// </summary>
    public bool HasCapacity => _members.Count < MaxMembers;

    /// <summary>
    /// Whether a user is a member of this workspace.
    /// </summary>
    public bool IsMember(Guid userId) => _members.Exists(m => m.UserId == userId);
}

/// <summary>
/// Role within a team workspace.
/// </summary>
public enum TeamRole
{
    Admin,
    Member
}

/// <summary>
/// A member of a team workspace.
/// </summary>
public sealed record TeamMember
{
    public Guid UserId { get; }
    public TeamRole Role { get; }
    public DateTimeOffset JoinedAt { get; }

    public TeamMember(Guid userId, TeamRole role, DateTimeOffset joinedAt)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("Team member user ID cannot be empty.");
        }

        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }
}
