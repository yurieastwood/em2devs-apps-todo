using Shouldly;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for the Subscription entity.
/// Maps to: docs/features/monetisation/subscription-tiers.feature
/// </summary>
public sealed class SubscriptionTests
{
    private static readonly DateTimeOffset _now = new(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _expiresAt = _now.AddMonths(1);
    private static readonly Guid _userId = Guid.NewGuid();

    // --- Scenario: Free-tier user has access to core features ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateFreeSubscription_When_ValidUserId()
    {
        // Given / When
        var sub = Subscription.CreateFree(_userId, _now);

        // Then
        sub.Id.ShouldNotBeNull();
        sub.UserId.ShouldBe(_userId);
        sub.Tier.ShouldBe(SubscriptionTier.Free);
        sub.Status.ShouldBe(SubscriptionStatus.Active);
        sub.StartedAt.ShouldBe(_now);
        sub.ExpiresAt.ShouldBeNull();
        sub.AutoRenew.ShouldBeFalse();
        sub.IsPremium.ShouldBeFalse();
        sub.IsActive.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CreatingFreeWithEmptyUserId()
    {
        var ex = Should.Throw<DomainException>(() => Subscription.CreateFree(Guid.Empty, _now));
        ex.Message.ShouldContain("User ID cannot be empty");
    }

    // --- Scenario: Subscribe to premium ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UpgradeToPro_When_FreeUser()
    {
        // Given
        var sub = Subscription.CreateFree(_userId, _now);

        // When
        sub.UpgradeToPro(_now, _expiresAt);

        // Then
        sub.Tier.ShouldBe(SubscriptionTier.Pro);
        sub.Status.ShouldBe(SubscriptionStatus.Active);
        sub.ExpiresAt.ShouldBe(_expiresAt);
        sub.AutoRenew.ShouldBeTrue();
        sub.IsPremium.ShouldBeTrue();
        sub.IsActive.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UpgradingToProFromPro()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToPro(_now, _expiresAt);

        var ex = Should.Throw<DomainException>(() => sub.UpgradeToPro(_now, _expiresAt));
        ex.Message.ShouldContain("Cannot upgrade to Pro from Pro tier");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UpgradingToProFromTeam()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToTeam(_now, _expiresAt);

        var ex = Should.Throw<DomainException>(() => sub.UpgradeToPro(_now, _expiresAt));
        ex.Message.ShouldContain("Cannot upgrade to Pro from Team tier");
    }

    // --- Scenario: Subscribe to team tier ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UpgradeToTeam_When_FreeUser()
    {
        var sub = Subscription.CreateFree(_userId, _now);

        sub.UpgradeToTeam(_now, _expiresAt);

        sub.Tier.ShouldBe(SubscriptionTier.Team);
        sub.Status.ShouldBe(SubscriptionStatus.Active);
        sub.ExpiresAt.ShouldBe(_expiresAt);
        sub.AutoRenew.ShouldBeTrue();
        sub.IsPremium.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UpgradeToTeam_When_ProUser()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToPro(_now, _expiresAt);

        sub.UpgradeToTeam(_now, _expiresAt.AddMonths(1));

        sub.Tier.ShouldBe(SubscriptionTier.Team);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AlreadyOnTeamTier()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToTeam(_now, _expiresAt);

        var ex = Should.Throw<DomainException>(() => sub.UpgradeToTeam(_now, _expiresAt));
        ex.Message.ShouldContain("Already on Team tier");
    }

    // --- Scenario: Premium subscription expires ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExpireSubscription_When_ProSubscription()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToPro(_now, _expiresAt);

        sub.Expire();

        sub.Status.ShouldBe(SubscriptionStatus.Expired);
        sub.Tier.ShouldBe(SubscriptionTier.Pro); // Tier is preserved for read-only data access
        sub.IsActive.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExpiringAlreadyExpired()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToPro(_now, _expiresAt);
        sub.Expire();

        var ex = Should.Throw<DomainException>(() => sub.Expire());
        ex.Message.ShouldContain("already expired");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExpiringFreeTier()
    {
        var sub = Subscription.CreateFree(_userId, _now);

        var ex = Should.Throw<DomainException>(() => sub.Expire());
        ex.Message.ShouldContain("Free-tier subscriptions do not expire");
    }

    // --- Scenario: Team lead cancels the subscription ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CancelSubscription_When_ActivePro()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToPro(_now, _expiresAt);

        sub.Cancel();

        sub.Status.ShouldBe(SubscriptionStatus.Cancelled);
        sub.AutoRenew.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CancelSubscription_When_ActiveTeam()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToTeam(_now, _expiresAt);

        sub.Cancel();

        sub.Status.ShouldBe(SubscriptionStatus.Cancelled);
        sub.AutoRenew.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CancellingAlreadyCancelled()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToPro(_now, _expiresAt);
        sub.Cancel();

        var ex = Should.Throw<DomainException>(() => sub.Cancel());
        ex.Message.ShouldContain("already cancelled");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CancellingExpired()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToPro(_now, _expiresAt);
        sub.Expire();

        var ex = Should.Throw<DomainException>(() => sub.Cancel());
        ex.Message.ShouldContain("Cannot cancel an expired subscription");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CancellingFreeTier()
    {
        var sub = Subscription.CreateFree(_userId, _now);

        var ex = Should.Throw<DomainException>(() => sub.Cancel());
        ex.Message.ShouldContain("Free-tier subscriptions cannot be cancelled");
    }

    // --- Grace Period ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EnterGracePeriod_When_TeamSubscription()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToTeam(_now, _expiresAt);
        var graceEnd = _expiresAt.AddDays(30);

        sub.EnterGracePeriod(graceEnd);

        sub.Status.ShouldBe(SubscriptionStatus.GracePeriod);
        sub.ExpiresAt.ShouldBe(graceEnd);
        sub.AutoRenew.ShouldBeFalse();
        sub.IsActive.ShouldBeTrue(); // Grace period counts as active
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AlreadyInGracePeriod()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToTeam(_now, _expiresAt);
        sub.EnterGracePeriod(_expiresAt.AddDays(30));

        var ex = Should.Throw<DomainException>(() => sub.EnterGracePeriod(_expiresAt.AddDays(60)));
        ex.Message.ShouldContain("Already in grace period");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GracePeriodForFreeTier()
    {
        var sub = Subscription.CreateFree(_userId, _now);

        var ex = Should.Throw<DomainException>(() => sub.EnterGracePeriod(_expiresAt));
        ex.Message.ShouldContain("Free-tier subscriptions do not have a grace period");
    }

    // --- Scenario: Downgrade from Team to Pro ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DowngradeFromTeamToPro_When_TeamSubscription()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToTeam(_now, _expiresAt);

        sub.DowngradeTo(SubscriptionTier.Pro);

        sub.Tier.ShouldBe(SubscriptionTier.Pro);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DowngradeToFree_When_ProSubscription()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToPro(_now, _expiresAt);

        sub.DowngradeTo(SubscriptionTier.Free);

        sub.Tier.ShouldBe(SubscriptionTier.Free);
        sub.Status.ShouldBe(SubscriptionStatus.Active);
        sub.ExpiresAt.ShouldBeNull();
        sub.AutoRenew.ShouldBeFalse();
        sub.IsPremium.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DowngradeFromTeamToFree_When_TeamSubscription()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToTeam(_now, _expiresAt);

        sub.DowngradeTo(SubscriptionTier.Free);

        sub.Tier.ShouldBe(SubscriptionTier.Free);
        sub.Status.ShouldBe(SubscriptionStatus.Active);
        sub.ExpiresAt.ShouldBeNull();
        sub.AutoRenew.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DowngradingToSameTier()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToPro(_now, _expiresAt);

        var ex = Should.Throw<DomainException>(() => sub.DowngradeTo(SubscriptionTier.Pro));
        ex.Message.ShouldContain("Cannot downgrade");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DowngradingToHigherTier()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToPro(_now, _expiresAt);

        var ex = Should.Throw<DomainException>(() => sub.DowngradeTo(SubscriptionTier.Team));
        ex.Message.ShouldContain("Cannot downgrade");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DowngradingFreeToFree()
    {
        var sub = Subscription.CreateFree(_userId, _now);

        var ex = Should.Throw<DomainException>(() => sub.DowngradeTo(SubscriptionTier.Free));
        ex.Message.ShouldContain("Cannot downgrade");
    }

    // --- Renewal ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RenewSubscription_When_ProExpired()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToPro(_now, _expiresAt);
        sub.Expire();
        var newExpiry = _now.AddMonths(2);

        sub.Renew(newExpiry);

        sub.Status.ShouldBe(SubscriptionStatus.Active);
        sub.ExpiresAt.ShouldBe(newExpiry);
        sub.AutoRenew.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RenewingFreeTier()
    {
        var sub = Subscription.CreateFree(_userId, _now);

        var ex = Should.Throw<DomainException>(() => sub.Renew(_expiresAt));
        ex.Message.ShouldContain("Free-tier subscriptions do not need renewal");
    }

    // --- Scenario: In-progress guild activities on premium expiry ---
    // (This scenario tests that expiry keeps tier info for read-only access)

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveTierOnExpiry_When_ProSubscriptionExpires()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToPro(_now, _expiresAt);

        sub.Expire();

        // Tier is still Pro (data is read-only, not deleted)
        sub.Tier.ShouldBe(SubscriptionTier.Pro);
        sub.Status.ShouldBe(SubscriptionStatus.Expired);
        sub.IsPremium.ShouldBeTrue(); // Still "premium" tier even though expired
        sub.IsActive.ShouldBeFalse(); // But not active
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveTierOnExpiry_When_TeamSubscriptionExpires()
    {
        var sub = Subscription.CreateFree(_userId, _now);
        sub.UpgradeToTeam(_now, _expiresAt);

        sub.Expire();

        sub.Tier.ShouldBe(SubscriptionTier.Team);
        sub.Status.ShouldBe(SubscriptionStatus.Expired);
    }
}
