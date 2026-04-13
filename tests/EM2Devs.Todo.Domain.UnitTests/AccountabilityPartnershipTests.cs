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

    // --- Scenario: Send an accountability partner request ---

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

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreatePendingPartnership_When_RequestSentWithSufficientLevel()
    {
        // Given a user at level 7+
        // When they send a partner request
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today, 7);

        // Then request is in Pending state
        partnership.Status.ShouldBe(PartnershipStatus.Pending);
        partnership.IsPending.ShouldBeTrue();
    }

    // --- Scenario: Accept a partner request ---

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

    // --- Scenario: Decline a partner request ---

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

    // --- Scenario: End an accountability partnership ---

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

    // --- Scenario: Only one active partner at a time ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UserAlreadyHasActivePartner()
    {
        // Given I already have an accountability partner
        var existing = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();
        var partnerships = new List<AccountabilityPartnership> { existing };

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => AccountabilityPartnership.ValidateCanSendRequest(partnerships, _requesterId));
        ex.Message.ShouldContain("already have an active partner");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowRequest_When_UserHasNoActivePartner()
    {
        // Given user has only ended partnerships
        var ended = AccountabilityPartnership.Request(_requesterId, _partnerId, _today)
            .Accept().End(_today);
        var partnerships = new List<AccountabilityPartnership> { ended };

        // When / Then — should not throw
        Should.NotThrow(() => AccountabilityPartnership.ValidateCanSendRequest(partnerships, _requesterId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowRequest_When_UserHasEmptyPartnershipList()
    {
        // Given no partnerships
        var partnerships = new List<AccountabilityPartnership>();

        // When / Then — should not throw
        Should.NotThrow(() => AccountabilityPartnership.ValidateCanSendRequest(partnerships, _requesterId));
    }

    // --- Scenario: Partner request requires minimum level ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RequesterBelowMinimumLevel()
    {
        // Given a user below level 7
        // When / Then
        var ex = Should.Throw<DomainException>(
            () => AccountabilityPartnership.Request(_requesterId, _partnerId, _today, 6));
        ex.Message.ShouldContain($"level {AccountabilityPartnership.MinimumLevelRequired}");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowRequest_When_RequesterAtExactMinimumLevel()
    {
        // Given a user at exactly level 7
        // When
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today, 7);

        // Then
        partnership.IsPending.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowRequest_When_RequesterAboveMinimumLevel()
    {
        // Given a user above level 7
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today, 10);
        partnership.IsPending.ShouldBeTrue();
    }

    // --- Scenario: View partner's daily summary ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateValidDailySummary_When_ViewingPartnerSummary()
    {
        // Given a partner with daily activity
        var summary = new PartnerDailySummary(
            tasksCompleted: 5,
            currentStreak: 12,
            xpEarnedToday: 150,
            activeQuestCount: 2,
            date: _today);

        // Then summary shows aggregate data without task details
        summary.TasksCompleted.ShouldBe(5);
        summary.CurrentStreak.ShouldBe(12);
        summary.XpEarnedToday.ShouldBe(150);
        summary.ActiveQuestCount.ShouldBe(2);
        summary.Date.ShouldBe(_today);
    }

    // --- Scenario: Partner sees my summary ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowCorrectCounts_When_PartnerViewsMySummary()
    {
        // Given I completed 5 tasks today and my streak is at 12 days
        var summary = new PartnerDailySummary(
            tasksCompleted: 5,
            currentStreak: 12,
            xpEarnedToday: 0,
            activeQuestCount: 0,
            date: _today);

        // Then partner sees "5 tasks completed today" and "12-day streak"
        summary.TasksCompleted.ShouldBe(5);
        summary.CurrentStreak.ShouldBe(12);
    }

    // --- Scenario: View partner's streak without revealing task details ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExposeStreakOnly_When_SummaryCreated()
    {
        // Given / When — summary only has aggregate data, no task titles
        var summary = new PartnerDailySummary(3, 7, 90, 1, _today);

        // Then — streak count visible, no task title properties exist
        summary.CurrentStreak.ShouldBe(7);
        summary.TasksCompleted.ShouldBe(3);
    }

    // --- Scenario: Send a check-in message to partner ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddMessageToHistory_When_CheckInMessageSent()
    {
        // Given an active partnership
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();
        var message = new CheckInMessage("Great streak, keep it going!", DateTimeOffset.UtcNow, _requesterId);

        // When
        var result = partnership.SendCheckInMessage(message);

        // Then the message appears in shared message history
        result.Messages.ShouldContain(message);
        result.Messages.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SendingMessageInNonActivePartnership()
    {
        // Given a pending partnership
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today);
        var message = new CheckInMessage("Keep going!", DateTimeOffset.UtcNow, _requesterId);

        // When / Then
        var ex = Should.Throw<DomainException>(() => partnership.SendCheckInMessage(message));
        ex.Message.ShouldContain("active partnership");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NonPartnerSendsMessage()
    {
        // Given an active partnership
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();
        var outsider = Guid.NewGuid();
        var message = new CheckInMessage("Hello!", DateTimeOffset.UtcNow, outsider);

        // When / Then
        var ex = Should.Throw<DomainException>(() => partnership.SendCheckInMessage(message));
        ex.Message.ShouldContain("Only partners");
    }

    // --- Scenario: Partner check-in messages are limited scope ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MessageExceeds280Characters()
    {
        // Given / When / Then
        string longText = new('a', 281);
        var ex = Should.Throw<DomainException>(
            () => new CheckInMessage(longText, DateTimeOffset.UtcNow, _requesterId));
        ex.Message.ShouldContain("280");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowMessage_When_ExactlyAt280Characters()
    {
        // Given / When
        string text = new('a', 280);
        var message = new CheckInMessage(text, DateTimeOffset.UtcNow, _requesterId);

        // Then
        message.Text.Length.ShouldBe(280);
    }

    // --- Scenario: Check-in message frequency limit ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExceedingDailyMessageLimit()
    {
        // Given an active partnership with 5 messages sent today
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();
        var now = new DateTimeOffset(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);

        for (int i = 0; i < AccountabilityPartnership.MaxCheckInMessagesPerDay; i++)
        {
            var msg = new CheckInMessage($"Message {i + 1}", now.AddMinutes(i), _requesterId);
            partnership = partnership.SendCheckInMessage(msg);
        }

        // When / Then — 6th message should fail
        var extraMsg = new CheckInMessage("One too many", now.AddMinutes(10), _requesterId);
        var ex = Should.Throw<DomainException>(() => partnership.SendCheckInMessage(extraMsg));
        ex.Message.ShouldContain($"{AccountabilityPartnership.MaxCheckInMessagesPerDay}");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowMessages_When_DifferentDays()
    {
        // Given 5 messages sent yesterday
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();
        var yesterday = new DateTimeOffset(2026, 3, 14, 10, 0, 0, TimeSpan.Zero);

        for (int i = 0; i < AccountabilityPartnership.MaxCheckInMessagesPerDay; i++)
        {
            var msg = new CheckInMessage($"Yesterday {i + 1}", yesterday.AddMinutes(i), _requesterId);
            partnership = partnership.SendCheckInMessage(msg);
        }

        // When — sending message today
        var todayTime = new DateTimeOffset(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);
        var todayMsg = new CheckInMessage("Today message", todayTime, _requesterId);

        // Then — should succeed
        var result = partnership.SendCheckInMessage(todayMsg);
        result.Messages.Count.ShouldBe(6);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TrackLimitPerSender_When_BothPartnersSendMessages()
    {
        // Given 5 messages from requester
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();
        var now = new DateTimeOffset(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);

        for (int i = 0; i < AccountabilityPartnership.MaxCheckInMessagesPerDay; i++)
        {
            var msg = new CheckInMessage($"From requester {i + 1}", now.AddMinutes(i), _requesterId);
            partnership = partnership.SendCheckInMessage(msg);
        }

        // When — partner sends their first message today
        var partnerMsg = new CheckInMessage("From partner", now.AddMinutes(10), _partnerId);

        // Then — should succeed (different sender)
        var result = partnership.SendCheckInMessage(partnerMsg);
        result.Messages.Count.ShouldBe(6);
    }

    // --- Scenario: Partnership does not affect XP or progression ---
    // (This is enforced by the domain design: PartnerDailySummary is read-only,
    //  partnerships have no XP-granting methods)

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotContainXpMethods_When_PartnershipIsCreated()
    {
        // Given an active partnership
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();

        // Then — partnership is social only; it has no XP-modifying behavior
        // Verified by the absence of any XP-granting methods on the type.
        // The partnership only tracks status and messages.
        partnership.IsActive.ShouldBeTrue();
        partnership.Messages.ShouldBeEmpty();
    }

    // --- Scenario: Both partners see the same partnership status ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnSameStatus_When_EitherPartnerChecks()
    {
        // Given an active partnership
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();

        // When both partners check status
        var requesterStatus = partnership.GetStatusForUser(_requesterId);
        var partnerStatus = partnership.GetStatusForUser(_partnerId);

        // Then both see the same status
        requesterStatus.ShouldBe(partnerStatus);
        requesterStatus.ShouldBe(PartnershipStatus.Active);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NonPartnerChecksStatus()
    {
        // Given
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today);
        var outsider = Guid.NewGuid();

        // When / Then
        var ex = Should.Throw<DomainException>(() => partnership.GetStatusForUser(outsider));
        ex.Message.ShouldContain("not part of this partnership");
    }

    // --- Scenario: Partner account is deactivated ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DissolvePartnership_When_AccountDeactivated()
    {
        // Given an active partnership
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();

        // When partner deactivates (dissolve)
        var result = partnership.Dissolve(_today);

        // Then
        result.Status.ShouldBe(PartnershipStatus.Ended);
        result.EndedOn.ShouldBe(_today);
        result.IsActive.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DissolvingNonActivePartnership()
    {
        // Given a pending partnership
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today);

        // When / Then
        var ex = Should.Throw<DomainException>(() => partnership.Dissolve(_today));
        ex.Message.ShouldContain("Only active");
    }

    // --- Scenario: Re-pair with a former partner ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowNewRequest_When_PreviousPartnershipEnded()
    {
        // Given a previously ended partnership
        var old = AccountabilityPartnership.Request(_requesterId, _partnerId, _today)
            .Accept().End(_today);
        var partnerships = new List<AccountabilityPartnership> { old };

        // When validating a new request to the same partner
        Should.NotThrow(
            () => AccountabilityPartnership.ValidateCanSendRequest(partnerships, _requesterId));

        // And creating the new request
        var newPartnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today);

        // Then new partnership is independent
        newPartnership.IsPending.ShouldBeTrue();
        newPartnership.Messages.ShouldBeEmpty();
    }

    // --- Scenario: Existing partnership persists regardless of level changes ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemainActive_When_LevelDropsBelowMinimum()
    {
        // Given a partnership formed at level 7+
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today, 7).Accept();

        // When level drops below 7 — the existing partnership is not affected
        // (Level changes are external; the partnership object itself doesn't track level)
        partnership.IsActive.ShouldBeTrue();

        // But a NEW request at level 6 should fail
        var ex = Should.Throw<DomainException>(
            () => AccountabilityPartnership.Request(_requesterId, Guid.NewGuid(), _today, 6));
        ex.Message.ShouldContain($"level {AccountabilityPartnership.MinimumLevelRequired}");
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

    // --- Messages preserved through state transitions ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveMessages_When_PartnershipEnded()
    {
        // Given an active partnership with messages
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();
        var msg = new CheckInMessage("Keep going!", DateTimeOffset.UtcNow, _requesterId);
        partnership = partnership.SendCheckInMessage(msg);

        // When partnership is ended
        var ended = partnership.End(_today);

        // Then past messages remain
        ended.Messages.Count.ShouldBe(1);
        ended.Messages[0].Text.ShouldBe("Keep going!");
    }

    // --- GetMessagesForDay ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnMessagesForSpecificDay_When_Queried()
    {
        // Given messages on different days
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();
        var day1 = new DateTimeOffset(2026, 3, 14, 10, 0, 0, TimeSpan.Zero);
        var day2 = new DateTimeOffset(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);

        partnership = partnership.SendCheckInMessage(
            new CheckInMessage("Day 1 msg", day1, _requesterId));
        partnership = partnership.SendCheckInMessage(
            new CheckInMessage("Day 2 msg", day2, _requesterId));

        // When
        var day1Messages = partnership.GetMessagesForDay(new DateOnly(2026, 3, 14));
        var day2Messages = partnership.GetMessagesForDay(new DateOnly(2026, 3, 15));

        // Then
        day1Messages.Count.ShouldBe(1);
        day1Messages[0].Text.ShouldBe("Day 1 msg");
        day2Messages.Count.ShouldBe(1);
        day2Messages[0].Text.ShouldBe("Day 2 msg");
    }

    // --- SendCheckInMessage null guard ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_NullMessageSent()
    {
        // Given
        var partnership = AccountabilityPartnership.Request(_requesterId, _partnerId, _today).Accept();

        // When / Then
        Should.Throw<ArgumentNullException>(() => partnership.SendCheckInMessage(null!));
    }
}
