using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for the TeamWorkspace value object.
/// Maps to: docs/features/monetisation/subscription-tiers.feature
/// Scenarios: "Subscribe to team tier", "Team tier includes team-specific features",
///            "Team lead cancels the subscription", "Team member is removed from the team",
///            "Downgrade from Team to Pro"
/// </summary>
public sealed class TeamWorkspaceTests
{
    private static readonly Guid _ownerId = Guid.NewGuid();
    private static readonly Guid _memberId = Guid.NewGuid();
    private static readonly DateTimeOffset _now = new(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);

    // --- Scenario: Subscribe to team tier ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateTeamWorkspace_When_ValidParameters()
    {
        var workspace = TeamWorkspace.Create("My Team", _ownerId);

        workspace.Id.ShouldNotBeNull();
        workspace.Name.ShouldBe("My Team");
        workspace.OwnerId.ShouldBe(_ownerId);
        workspace.MaxMembers.ShouldBe(TeamWorkspace.DefaultMaxMembers);
        workspace.IsActive.ShouldBeTrue();
        workspace.MemberCount.ShouldBe(1);
        workspace.Members[0].UserId.ShouldBe(_ownerId);
        workspace.Members[0].Role.ShouldBe(TeamRole.Admin);
        workspace.HasCapacity.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateTeamWorkspace_When_CustomMaxMembers()
    {
        var workspace = TeamWorkspace.Create("Small Team", _ownerId, 5);

        workspace.MaxMembers.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NameIsEmpty()
    {
        var ex = Should.Throw<DomainException>(() => TeamWorkspace.Create("", _ownerId));
        ex.Message.ShouldContain("Team workspace name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NameIsWhitespace()
    {
        var ex = Should.Throw<DomainException>(() => TeamWorkspace.Create("  ", _ownerId));
        ex.Message.ShouldContain("Team workspace name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NameExceedsMaxLength()
    {
        string longName = new('A', 101);
        var ex = Should.Throw<DomainException>(() => TeamWorkspace.Create(longName, _ownerId));
        ex.Message.ShouldContain("Team workspace name cannot exceed 100 characters");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_OwnerIdIsEmpty()
    {
        // Create method builds a TeamMember first, which validates the empty Guid
        var ex = Should.Throw<DomainException>(() => TeamWorkspace.Create("Team", Guid.Empty));
        ex.Message.ShouldContain("user ID cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ConstructorOwnerIdIsEmpty()
    {
        var member = new TeamMember(Guid.NewGuid(), TeamRole.Admin, _now);
        var ex = Should.Throw<DomainException>(
            () => new TeamWorkspace(TeamWorkspaceId.New(), "Team", Guid.Empty, 25, [member]));
        ex.Message.ShouldContain("Owner ID cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MaxMembersIsLessThanOne()
    {
        var id = TeamWorkspaceId.New();
        var owner = new TeamMember(_ownerId, TeamRole.Admin, _now);
        var ex = Should.Throw<DomainException>(
            () => new TeamWorkspace(id, "Team", _ownerId, 0, [owner]));
        ex.Message.ShouldContain("Maximum members must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MembersExceedMax()
    {
        var id = TeamWorkspaceId.New();
        var owner = new TeamMember(_ownerId, TeamRole.Admin, _now);
        var member = new TeamMember(_memberId, TeamRole.Member, _now);
        var ex = Should.Throw<DomainException>(
            () => new TeamWorkspace(id, "Team", _ownerId, 1, [owner, member]));
        ex.Message.ShouldContain("Team cannot have more than 1 members");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_IdIsNull()
    {
        var owner = new TeamMember(_ownerId, TeamRole.Admin, _now);
        Should.Throw<ArgumentNullException>(
            () => new TeamWorkspace(null!, "Team", _ownerId, 25, [owner]));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_MembersIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new TeamWorkspace(TeamWorkspaceId.New(), "Team", _ownerId, 25, null!));
    }

    // --- Adding members ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddMember_When_WorkspaceHasCapacity()
    {
        var workspace = TeamWorkspace.Create("Team", _ownerId);

        var updated = workspace.AddMember(_memberId, _now);

        updated.MemberCount.ShouldBe(2);
        updated.IsMember(_memberId).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_WorkspaceAtCapacity()
    {
        var workspace = TeamWorkspace.Create("Tiny Team", _ownerId, 1);

        var ex = Should.Throw<DomainException>(() => workspace.AddMember(_memberId, _now));
        ex.Message.ShouldContain("maximum capacity");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UserAlreadyMember()
    {
        var workspace = TeamWorkspace.Create("Team", _ownerId);
        var updated = workspace.AddMember(_memberId, _now);

        var ex = Should.Throw<DomainException>(() => updated.AddMember(_memberId, _now));
        ex.Message.ShouldContain("already a team member");
    }

    // --- Scenario: Team member is removed from the team ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemoveMember_When_ValidMember()
    {
        var workspace = TeamWorkspace.Create("Team", _ownerId)
            .AddMember(_memberId, _now);

        var updated = workspace.RemoveMember(_memberId);

        updated.MemberCount.ShouldBe(1);
        updated.IsMember(_memberId).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RemovingOwner()
    {
        var workspace = TeamWorkspace.Create("Team", _ownerId);

        var ex = Should.Throw<DomainException>(() => workspace.RemoveMember(_ownerId));
        ex.Message.ShouldContain("Cannot remove the workspace owner");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RemovingNonMember()
    {
        var workspace = TeamWorkspace.Create("Team", _ownerId);
        var nonMemberId = Guid.NewGuid();

        var ex = Should.Throw<DomainException>(() => workspace.RemoveMember(nonMemberId));
        ex.Message.ShouldContain("not a team member");
    }

    // --- Scenario: Team lead cancels the subscription ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DeactivateWorkspace_When_SubscriptionCancelled()
    {
        var workspace = TeamWorkspace.Create("Team", _ownerId)
            .AddMember(_memberId, _now);

        var deactivated = workspace.Deactivate();

        deactivated.IsActive.ShouldBeFalse();
        deactivated.MemberCount.ShouldBe(2); // Members preserved
        deactivated.Name.ShouldBe("Team");
    }

    // --- HasCapacity ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotHaveCapacity_When_AtMaxMembers()
    {
        var workspace = TeamWorkspace.Create("Tiny Team", _ownerId, 1);

        workspace.HasCapacity.ShouldBeFalse();
    }

    // --- IsMember ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_UserIsNotMember()
    {
        var workspace = TeamWorkspace.Create("Team", _ownerId);

        workspace.IsMember(Guid.NewGuid()).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrue_When_UserIsOwner()
    {
        var workspace = TeamWorkspace.Create("Team", _ownerId);

        workspace.IsMember(_ownerId).ShouldBeTrue();
    }

    // --- TeamMember ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateTeamMember_When_ValidParameters()
    {
        var member = new TeamMember(_memberId, TeamRole.Member, _now);

        member.UserId.ShouldBe(_memberId);
        member.Role.ShouldBe(TeamRole.Member);
        member.JoinedAt.ShouldBe(_now);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TeamMemberUserIdIsEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new TeamMember(Guid.Empty, TeamRole.Member, _now));
        ex.Message.ShouldContain("Team member user ID cannot be empty");
    }

    // --- DefaultMaxMembers ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveDefaultMaxOf25()
    {
        TeamWorkspace.DefaultMaxMembers.ShouldBe(25);
    }

    // --- Name exactly 100 chars (boundary) ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowName_When_Exactly100Characters()
    {
        string name = new('A', 100);
        var workspace = TeamWorkspace.Create(name, _ownerId);
        workspace.Name.ShouldBe(name);
    }
}
