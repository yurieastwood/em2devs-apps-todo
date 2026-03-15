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
}
