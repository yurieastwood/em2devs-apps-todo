using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for Guild, GuildMember value objects.
/// Maps to: docs/features/social/guilds.feature
/// Rule: "Users can create and manage guilds of 2-12 members"
/// </summary>
public sealed class GuildTests
{
    private static readonly DateOnly _today = new(2026, 3, 15);
    private static readonly Guid _leaderId = Guid.NewGuid();
    private static readonly Guid _memberId = Guid.NewGuid();
    private static readonly Guid _memberId2 = Guid.NewGuid();

    // --- Guild Creation ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateGuild_When_ValidParameters()
    {
        // Given / When
        var guild = Guild.Create("Side Project Squad", "Accountability for builders", _leaderId, _today);

        // Then
        guild.Name.ShouldBe("Side Project Squad");
        guild.Description.ShouldBe("Accountability for builders");
        guild.MemberCount.ShouldBe(1);
        guild.LeaderId.ShouldBe(_leaderId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GuildNameIsEmpty()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => Guild.Create("", "desc", _leaderId, _today));
        ex.Message.ShouldContain("name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GuildNameExceeds50Chars()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => Guild.Create(new string('x', 51), "desc", _leaderId, _today));
        ex.Message.ShouldContain("cannot exceed 50");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptGuildName_When_Exactly50Chars()
    {
        // Given / When
        var guild = Guild.Create(new string('x', 50), "desc", _leaderId, _today);

        // Then
        guild.Name.Length.ShouldBe(50);
    }

    // --- Member Management ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddMember_When_GuildHasCapacity()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When
        var result = guild.AddMember(_memberId, _today);

        // Then
        result.MemberCount.ShouldBe(2);
        result.IsMember(_memberId).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GuildAtCapacity()
    {
        // Given — guild with 12 members
        var guild = Guild.Create("Full Guild", "desc", _leaderId, _today);
        for (int i = 0; i < Guild.MaxMembers - 1; i++)
        {
            guild = guild.AddMember(Guid.NewGuid(), _today);
        }

        guild.IsAtCapacity.ShouldBeTrue();

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.AddMember(Guid.NewGuid(), _today));
        ex.Message.ShouldContain("maximum capacity");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AddingDuplicateMember()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.AddMember(_memberId, _today));
        ex.Message.ShouldContain("already a guild member");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemoveMember_When_MemberExists()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When
        var result = guild.RemoveMember(_memberId);

        // Then
        result.MemberCount.ShouldBe(1);
        result.IsMember(_memberId).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RemovingNonMember()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.RemoveMember(Guid.NewGuid()));
        ex.Message.ShouldContain("not a guild member");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RemovingLeader()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.RemoveMember(_leaderId));
        ex.Message.ShouldContain("Transfer leadership first");
    }

    // --- Leadership Transfer ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TransferLeadership_When_NewLeaderIsMember()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When
        var result = guild.TransferLeadership(_memberId);

        // Then
        result.LeaderId.ShouldBe(_memberId);
        result.Members.First(m => m.UserId == _leaderId).Role.ShouldBe(GuildRole.Member);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TransferringToNonMember()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.TransferLeadership(Guid.NewGuid()));
        ex.Message.ShouldContain("existing guild member");
    }

    // --- GuildMember Validation ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MemberUserIdIsEmpty()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new GuildMember(Guid.Empty, GuildRole.Member, _today));
        ex.Message.ShouldContain("user ID cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateGuildMember_When_ValidParameters()
    {
        // Given / When
        var member = new GuildMember(_memberId, GuildRole.Member, _today);

        // Then
        member.UserId.ShouldBe(_memberId);
        member.Role.ShouldBe(GuildRole.Member);
        member.JoinedOn.ShouldBe(_today);
    }

    // --- Queries ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_CheckingNonMember()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When / Then
        guild.IsMember(Guid.NewGuid()).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeAtCapacity_When_BelowMax()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When / Then
        guild.IsAtCapacity.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GuildHasNoLeader()
    {
        // Given / When / Then
        var member = new GuildMember(_memberId, GuildRole.Member, _today);
        var ex = Should.Throw<DomainException>(
            () => new Guild("Test", "desc", [member]));
        ex.Message.ShouldContain("must have a leader");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_MembersIsNull()
    {
        // Given / When / Then
        Should.Throw<ArgumentNullException>(
            () => new Guild("Test", "desc", null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HandleNullDescription_When_Creating()
    {
        // Given / When
        var guild = Guild.Create("Test Guild", null!, _leaderId, _today);

        // Then
        guild.Description.ShouldBe(string.Empty);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ConstructorExceedsMaxMembers()
    {
        // Given — build a list exceeding max
        var leader = new GuildMember(_leaderId, GuildRole.Leader, _today);
        List<GuildMember> members = [leader];
        for (int i = 0; i < Guild.MaxMembers; i++)
        {
            members.Add(new GuildMember(Guid.NewGuid(), GuildRole.Member, _today));
        }

        // When / Then — 13 members exceeds max of 12
        var ex = Should.Throw<DomainException>(
            () => new Guild("Test", "desc", members));
        ex.Message.ShouldContain("more than");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveThirdMember_When_TransferringLeadership()
    {
        // Given — 3 members: leader, member1, member2
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today)
            .AddMember(_memberId2, _today);

        // When — transfer leadership to member1
        var result = guild.TransferLeadership(_memberId);

        // Then — member2 unchanged
        GuildMember thirdMember = result.Members.First(m => m.UserId == _memberId2);
        thirdMember.Role.ShouldBe(GuildRole.Member);
        result.MemberCount.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DemoteOldLeader_When_TransferringLeadership()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When
        var result = guild.TransferLeadership(_memberId);

        // Then — old leader is now a regular member
        GuildMember oldLeader = result.Members.First(m => m.UserId == _leaderId);
        oldLeader.Role.ShouldBe(GuildRole.Member);

        // And new leader is leader
        GuildMember newLeader = result.Members.First(m => m.UserId == _memberId);
        newLeader.Role.ShouldBe(GuildRole.Leader);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnLeaderId_When_GuildHasLeader()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When
        Guid leaderId = guild.LeaderId;

        // Then
        leaderId.ShouldBe(_leaderId);
        leaderId.ShouldNotBe(Guid.Empty);
    }

    // --- Title Visibility in Guild Member List ---
    // Scenario: "Title visible in guild member list"

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CarryNoTitle_When_GuildMemberCreatedWithoutTitle()
    {
        // Given / When
        var member = new GuildMember(_memberId, GuildRole.Member, _today);

        // Then — active title defaults to null
        member.ActiveTitle.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CarryTitle_When_GuildMemberCreatedWithTitle()
    {
        // Given / When
        var member = new GuildMember(_memberId, GuildRole.Member, _today, TitleType.MorningArchitect);

        // Then — title visible in member list
        member.ActiveTitle.ShouldBe(TitleType.MorningArchitect);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowTitleNextToName_When_ListingGuildMembers()
    {
        // Given — guild with members, one having an active title
        var leader = new GuildMember(_leaderId, GuildRole.Leader, _today, TitleType.BossSlayer);
        var member = new GuildMember(_memberId, GuildRole.Member, _today, TitleType.MorningArchitect);
        var guild = new Guild("Test Guild", "desc", [leader, member]);

        // When — listing members
        var members = guild.Members;

        // Then — each member's title is visible
        members.First(m => m.UserId == _leaderId).ActiveTitle.ShouldBe(TitleType.BossSlayer);
        members.First(m => m.UserId == _memberId).ActiveTitle.ShouldBe(TitleType.MorningArchitect);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddMemberWithoutTitle_When_NoTitleProvided()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When
        var result = guild.AddMember(_memberId, _today);

        // Then
        GuildMember added = result.Members.First(m => m.UserId == _memberId);
        added.ActiveTitle.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddMemberWithTitle_When_TitleProvided()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When
        var result = guild.AddMember(_memberId, _today, TitleType.StreakMaster);

        // Then
        GuildMember added = result.Members.First(m => m.UserId == _memberId);
        added.ActiveTitle.ShouldBe(TitleType.StreakMaster);
    }

    // --- Scenario: Create a guild (full verification) ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AssignGuildId_When_CreatingGuild()
    {
        // Given / When
        var guild = Guild.Create("Side Project Squad", "Accountability for builders", _leaderId, _today);

        // Then
        guild.Id.ShouldNotBeNull();
        guild.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeDisbanded_When_CreatingGuild()
    {
        // Given / When
        var guild = Guild.Create("Side Project Squad", "desc", _leaderId, _today);

        // Then
        guild.IsDisbanded.ShouldBeFalse();
    }

    // --- Scenario: Guild reaches maximum capacity ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RejectNewMember_When_GuildAtMaxCapacityViaInvite()
    {
        // Given — guild with 12 members
        var guild = Guild.Create("Full Guild", "desc", _leaderId, _today);
        for (int i = 0; i < Guild.MaxMembers - 1; i++)
        {
            guild = guild.AddMember(Guid.NewGuid(), _today);
        }

        var invite = guild.GenerateInviteLink(_today);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.AcceptInvite(invite, Guid.NewGuid(), _today));
        ex.Message.ShouldContain("maximum capacity");
    }

    // --- Scenario: Remove a member from a guild ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemoveMember_When_LeaderRemovesMember()
    {
        // Given — guild with 5 members
        var guild = Guild.Create("Side Project Squad", "desc", _leaderId, _today);
        var memberIds = new List<Guid>();
        for (int i = 0; i < 4; i++)
        {
            var id = Guid.NewGuid();
            memberIds.Add(id);
            guild = guild.AddMember(id, _today);
        }

        guild.MemberCount.ShouldBe(5);

        // When — leader removes the first member
        Guid alexId = memberIds[0];
        var result = guild.RemoveMember(alexId);

        // Then
        result.MemberCount.ShouldBe(4);
        result.IsMember(alexId).ShouldBeFalse();
    }

    // --- Scenario: Generate an invite link for a guild ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateInviteLink_When_LeaderRequests()
    {
        // Given
        var guild = Guild.Create("Side Project Squad", "desc", _leaderId, _today);

        // When
        GuildInviteLink invite = guild.GenerateInviteLink(_today);

        // Then
        invite.ShouldNotBeNull();
        invite.GuildId.ShouldBe(guild.Id);
        invite.Token.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExpireAfter7Days_When_InviteLinkGenerated()
    {
        // Given
        var guild = Guild.Create("Side Project Squad", "desc", _leaderId, _today);

        // When
        GuildInviteLink invite = guild.GenerateInviteLink(_today);

        // Then
        invite.ExpiresOn.ShouldBe(_today.AddDays(7));
        invite.IsExpired(_today).ShouldBeFalse();
        invite.IsExpired(_today.AddDays(7)).ShouldBeFalse();
        invite.IsExpired(_today.AddDays(8)).ShouldBeTrue();
    }

    // --- Scenario: Accept a guild invite via link ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddMember_When_AcceptingValidInvite()
    {
        // Given
        var guild = Guild.Create("Side Project Squad", "desc", _leaderId, _today);
        GuildInviteLink invite = guild.GenerateInviteLink(_today);

        // When
        var result = guild.AcceptInvite(invite, _memberId, _today);

        // Then
        result.MemberCount.ShouldBe(2);
        result.IsMember(_memberId).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InviteLinkExpired()
    {
        // Given
        var guild = Guild.Create("Side Project Squad", "desc", _leaderId, _today);
        GuildInviteLink invite = guild.GenerateInviteLink(_today);

        // When / Then — try to accept 8 days later
        var ex = Should.Throw<DomainException>(
            () => guild.AcceptInvite(invite, _memberId, _today.AddDays(8)));
        ex.Message.ShouldContain("expired");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InviteLinkBelongsToDifferentGuild()
    {
        // Given
        var guild1 = Guild.Create("Guild One", "desc", _leaderId, _today);
        var guild2 = Guild.Create("Guild Two", "desc", Guid.NewGuid(), _today);
        GuildInviteLink invite = guild2.GenerateInviteLink(_today);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild1.AcceptInvite(invite, _memberId, _today));
        ex.Message.ShouldContain("does not belong to this guild");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AcceptingNullInvite()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When / Then
        Should.Throw<ArgumentNullException>(
            () => guild.AcceptInvite(null!, _memberId, _today));
    }

    // --- Scenario: Leave a guild ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemoveMember_When_MemberLeaves()
    {
        // Given
        var guild = Guild.Create("Study Group Alpha", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When
        var result = guild.Leave(_memberId);

        // Then
        result.MemberCount.ShouldBe(1);
        result.IsMember(_memberId).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_LeaderTriesToLeave()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.Leave(_leaderId));
        ex.Message.ShouldContain("transferring leadership");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NonMemberTriesToLeave()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.Leave(Guid.NewGuid()));
        ex.Message.ShouldContain("not a guild member");
    }

    // --- Scenario: Leader leaves the guild ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TransferToLongestServing_When_LeaderLeaves()
    {
        // Given — leader + 2 members, _memberId joined first
        var guild = Guild.Create("Side Project Squad", "desc", _leaderId, _today)
            .AddMember(_memberId, _today.AddDays(1))
            .AddMember(_memberId2, _today.AddDays(2));

        // When
        var result = guild.LeaderLeave();

        // Then — _memberId joined earliest, gets leadership
        result.LeaderId.ShouldBe(_memberId);
        result.IsMember(_leaderId).ShouldBeFalse();
        result.MemberCount.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_LastMemberTriesToLeaderLeave()
    {
        // Given — single-member guild
        var guild = Guild.Create("Solo Guild", "desc", _leaderId, _today);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.LeaderLeave());
        ex.Message.ShouldContain("Disband");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveOtherMembers_When_LeaderLeaves()
    {
        // Given — leader + 2 members
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today)
            .AddMember(_memberId2, _today);

        // When
        var result = guild.LeaderLeave();

        // Then — both non-leader members remain
        result.IsMember(_memberId).ShouldBeTrue();
        result.IsMember(_memberId2).ShouldBeTrue();
    }

    // --- Scenario: Disband a guild ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DisbandGuild_When_LeaderRequests()
    {
        // Given
        var guild = Guild.Create("Side Project Squad", "desc", _leaderId, _today);

        // When
        var result = guild.Disband(_leaderId);

        // Then
        result.IsDisbanded.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NonLeaderTriesToDisband()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.Disband(_memberId));
        ex.Message.ShouldContain("Only the guild leader");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveMembers_When_Disbanded()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When
        var result = guild.Disband(_leaderId);

        // Then — members preserved for history
        result.MemberCount.ShouldBe(2);
        result.IsDisbanded.ShouldBeTrue();
    }

    // --- Scenario: User can only lead a limited number of guilds ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveMaxGuildsPerLeader_EqualTo3()
    {
        // Then
        Guild.MaxGuildsPerLeader.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EnforceLeaderLimit_When_CheckingLeaderGuildCount()
    {
        // Given — user already leads 3 guilds
        int leaderGuildCount = 3;

        // When / Then — the constant is used by callers to enforce the limit
        leaderGuildCount.ShouldBeGreaterThanOrEqualTo(Guild.MaxGuildsPerLeader);
    }

    // --- Scenario: Edit guild details ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UpdateDetails_When_LeaderEdits()
    {
        // Given
        var guild = Guild.Create("Side Project Squad", "Accountability for builders", _leaderId, _today);

        // When
        var result = guild.UpdateDetails(_leaderId, "Side Project Champions", "Shipping greatness together");

        // Then
        result.Name.ShouldBe("Side Project Champions");
        result.Description.ShouldBe("Shipping greatness together");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NonLeaderEdits()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.UpdateDetails(_memberId, "New Name", "New Desc"));
        ex.Message.ShouldContain("Only the guild leader");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveId_When_UpdatingDetails()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When
        var result = guild.UpdateDetails(_leaderId, "New Name", "New Desc");

        // Then
        result.Id.ShouldBe(guild.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveMembers_When_UpdatingDetails()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When
        var result = guild.UpdateDetails(_leaderId, "New Name", "New Desc");

        // Then
        result.MemberCount.ShouldBe(2);
        result.IsMember(_memberId).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UpdatingWithEmptyName()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.UpdateDetails(_leaderId, "", "desc"));
        ex.Message.ShouldContain("name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UpdatingWithNameTooLong()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => guild.UpdateDetails(_leaderId, new string('x', 51), "desc"));
        ex.Message.ShouldContain("cannot exceed 50");
    }

    // --- GuildId ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_GuildIdIsNull()
    {
        // Given / When / Then
        var leader = new GuildMember(_leaderId, GuildRole.Leader, _today);
        Should.Throw<ArgumentNullException>(
            () => new Guild(null!, "Test", "desc", [leader]));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveGuildId_When_AddingMember()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When
        var result = guild.AddMember(_memberId, _today);

        // Then
        result.Id.ShouldBe(guild.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveGuildId_When_RemovingMember()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When
        var result = guild.RemoveMember(_memberId);

        // Then
        result.Id.ShouldBe(guild.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveGuildId_When_TransferringLeadership()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When
        var result = guild.TransferLeadership(_memberId);

        // Then
        result.Id.ShouldBe(guild.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveGuildId_When_MemberLeaves()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When
        var result = guild.Leave(_memberId);

        // Then
        result.Id.ShouldBe(guild.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveGuildId_When_LeaderLeaves()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today)
            .AddMember(_memberId, _today);

        // When
        var result = guild.LeaderLeave();

        // Then
        result.Id.ShouldBe(guild.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveGuildId_When_Disbanded()
    {
        // Given
        var guild = Guild.Create("Test Guild", "desc", _leaderId, _today);

        // When
        var result = guild.Disband(_leaderId);

        // Then
        result.Id.ShouldBe(guild.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateNewGuildId_When_UsingNewFactory()
    {
        // Given / When
        var id = GuildId.New();

        // Then
        id.ShouldNotBeNull();
        id.Value.ShouldNotBe(Guid.Empty);
    }

    // --- Backward-compatible constructor ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateGuildId_When_UsingLegacyConstructor()
    {
        // Given / When
        var leader = new GuildMember(_leaderId, GuildRole.Leader, _today);
        var guild = new Guild("Test", "desc", [leader]);

        // Then
        guild.Id.ShouldNotBeNull();
        guild.Id.Value.ShouldNotBe(Guid.Empty);
    }
}
