using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for GuildInviteLink value object.
/// Maps to: docs/features/social/guilds.feature
/// Scenarios: "Generate an invite link for a guild", "Accept a guild invite via link"
/// </summary>
public sealed class GuildInviteLinkTests
{
    private static readonly DateOnly _today = new(2026, 3, 15);
    private static readonly GuildId _guildId = GuildId.New();

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateInviteLink_When_ValidParameters()
    {
        // Given / When
        var invite = GuildInviteLink.Create(_guildId, _today);

        // Then
        invite.GuildId.ShouldBe(_guildId);
        invite.Token.ShouldNotBeNullOrWhiteSpace();
        invite.ExpiresOn.ShouldBe(_today.AddDays(GuildInviteLink.DefaultExpiryDays));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveDefaultExpiry7Days()
    {
        // Then
        GuildInviteLink.DefaultExpiryDays.ShouldBe(7);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeExpired_When_WithinExpiryPeriod()
    {
        // Given
        var invite = GuildInviteLink.Create(_guildId, _today);

        // When / Then
        invite.IsExpired(_today).ShouldBeFalse();
        invite.IsExpired(_today.AddDays(6)).ShouldBeFalse();
        invite.IsExpired(_today.AddDays(7)).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeExpired_When_PastExpiryDate()
    {
        // Given
        var invite = GuildInviteLink.Create(_guildId, _today);

        // When / Then
        invite.IsExpired(_today.AddDays(8)).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_GuildIdIsNull()
    {
        // Given / When / Then
        Should.Throw<ArgumentNullException>(
            () => new GuildInviteLink(null!, "token", _today));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TokenIsEmpty()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new GuildInviteLink(_guildId, "", _today));
        ex.Message.ShouldContain("token cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TokenIsWhitespace()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new GuildInviteLink(_guildId, "   ", _today));
        ex.Message.ShouldContain("token cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateUniqueTokens_When_CreatingMultipleLinks()
    {
        // Given / When
        var invite1 = GuildInviteLink.Create(_guildId, _today);
        var invite2 = GuildInviteLink.Create(_guildId, _today);

        // Then
        invite1.Token.ShouldNotBe(invite2.Token);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StoreGuildId_When_Created()
    {
        // Given / When
        var invite = new GuildInviteLink(_guildId, "abc123", _today);

        // Then
        invite.GuildId.ShouldBe(_guildId);
        invite.Token.ShouldBe("abc123");
        invite.ExpiresOn.ShouldBe(_today);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateTokenWithoutHyphens_When_Created()
    {
        // Given / When
        var invite = GuildInviteLink.Create(_guildId, _today);

        // Then — token uses "N" format (no hyphens, 32 hex chars)
        invite.Token.ShouldNotContain("-");
        invite.Token.Length.ShouldBe(32);
    }
}
