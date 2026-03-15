using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for AccountabilityPartnership.
/// Maps to: docs/features/social/accountability-partners.feature
/// Rule: "Users can pair with one accountability partner at a time"
/// Rule: "Partners see daily summaries, not task-level detail"
/// </summary>
public sealed class AccountabilityPartnershipTests
{
    private static readonly DateOnly _today = new(2026, 3, 15);
    private static readonly Guid _requesterId = Guid.NewGuid();
    private static readonly Guid _partnerId = Guid.NewGuid();

    // --- Request ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreatePendingPartnership_When_RequestSent()
    {
        // Given / When
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today);

        // Then
        partnership.RequesterId.ShouldBe(_requesterId);
        partnership.PartnerId.ShouldBe(_partnerId);
        partnership.Status.ShouldBe(PartnershipStatus.Pending);
        partnership.IsPending.ShouldBeTrue();
        partnership.IsActive.ShouldBeFalse();
        partnership.CreatedOn.ShouldBe(_today);
        partnership.EndedOn.ShouldBeNull();
    }

    // --- Accept ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BecomeActive_When_PartnerAccepts()
    {
        // Given
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today);

        // When
        var result = partnership.Accept();

        // Then
        result.Status.ShouldBe(PartnershipStatus.Active);
        result.IsActive.ShouldBeTrue();
        result.IsPending.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AcceptingNonPendingPartnership()
    {
        // Given — already active
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();

        // When / Then
        var ex = Should.Throw<DomainException>(() => partnership.Accept());
        ex.Message.ShouldContain("Only pending");
    }

    // --- Decline ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EndPartnership_When_Declined()
    {
        // Given
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today);

        // When
        var result = partnership.Decline();

        // Then
        result.Status.ShouldBe(PartnershipStatus.Ended);
        result.IsActive.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DecliningNonPendingPartnership()
    {
        // Given — already active
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();

        // When / Then
        var ex = Should.Throw<DomainException>(() => partnership.Decline());
        ex.Message.ShouldContain("Only pending");
    }

    // --- End ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EndPartnership_When_ActivePartnershipEnded()
    {
        // Given
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();

        // When
        var result = partnership.End(_today);

        // Then
        result.Status.ShouldBe(PartnershipStatus.Ended);
        result.EndedOn.ShouldBe(_today);
        result.IsActive.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EndingNonActivePartnership()
    {
        // Given — still pending
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today);

        // When / Then
        var ex = Should.Throw<DomainException>(() => partnership.End(_today));
        ex.Message.ShouldContain("Only active");
    }

    // --- Queries ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrue_When_UserIsInvolved()
    {
        // Given
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today);

        // When / Then
        partnership.InvolvesUser(_requesterId).ShouldBeTrue();
        partnership.InvolvesUser(_partnerId).ShouldBeTrue();
        partnership.InvolvesUser(Guid.NewGuid()).ShouldBeFalse();
    }

    // --- Validation ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RequesterIdIsEmpty()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => AccountabilityPartnership.Request(Guid.Empty, _partnerId, _today));
        ex.Message.ShouldContain("Requester ID cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_PartnerIdIsEmpty()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => AccountabilityPartnership.Request(_requesterId, Guid.Empty, _today));
        ex.Message.ShouldContain("Partner ID cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_PartneringWithSelf()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => AccountabilityPartnership.Request(_requesterId, _requesterId, _today));
        ex.Message.ShouldContain("Cannot form a partnership with yourself");
    }
}
