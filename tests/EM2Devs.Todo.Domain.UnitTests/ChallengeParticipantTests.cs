using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for ChallengeParticipant value object.
/// Maps to: docs/features/social/challenge-mode.feature
/// </summary>
public sealed class ChallengeParticipantTests
{
    private static readonly Guid _userId = Guid.NewGuid();
    private static readonly DateTimeOffset _joinedAt = new(2026, 4, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateParticipant_When_ValidParameters()
    {
        // Given / When
        var participant = new ChallengeParticipant(_userId, 0, _joinedAt);

        // Then
        participant.UserId.ShouldBe(_userId);
        participant.TasksCompleted.ShouldBe(0);
        participant.JoinedAt.ShouldBe(_joinedAt);
        participant.Withdrawn.ShouldBeFalse();
        participant.LastCompletedAt.ShouldBe(_joinedAt);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UserIdIsEmpty()
    {
        var ex = Should.Throw<DomainException>(() =>
            new ChallengeParticipant(Guid.Empty, 0, _joinedAt));
        ex.Message.ShouldContain("user ID cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TasksCompletedIsNegative()
    {
        var ex = Should.Throw<DomainException>(() =>
            new ChallengeParticipant(_userId, -1, _joinedAt));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementCount_When_RecordingTaskCompletion()
    {
        // Given
        var participant = new ChallengeParticipant(_userId, 0, _joinedAt);
        var completedAt = _joinedAt.AddMinutes(30);

        // When
        var updated = participant.RecordTaskCompletion(completedAt);

        // Then
        updated.TasksCompleted.ShouldBe(1);
        updated.LastCompletedAt.ShouldBe(completedAt);
        updated.Withdrawn.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RecordingCompletionWhileWithdrawn()
    {
        // Given
        var participant = new ChallengeParticipant(_userId, 0, _joinedAt).Withdraw();

        // When / Then
        var ex = Should.Throw<DomainException>(() =>
            participant.RecordTaskCompletion(_joinedAt.AddMinutes(1)));
        ex.Message.ShouldContain("withdrawn participant");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MarkAsWithdrawn_When_Withdrawing()
    {
        // Given
        var participant = new ChallengeParticipant(_userId, 5, _joinedAt);

        // When
        var withdrawn = participant.Withdraw();

        // Then
        withdrawn.Withdrawn.ShouldBeTrue();
        withdrawn.TasksCompleted.ShouldBe(5);
        withdrawn.UserId.ShouldBe(_userId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_WithdrawingTwice()
    {
        // Given
        var participant = new ChallengeParticipant(_userId, 0, _joinedAt).Withdraw();

        // When / Then
        var ex = Should.Throw<DomainException>(() => participant.Withdraw());
        ex.Message.ShouldContain("already withdrawn");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SetLastCompletedAt_When_CustomValueProvided()
    {
        // Given
        var customTime = _joinedAt.AddHours(2);

        // When
        var participant = new ChallengeParticipant(_userId, 3, _joinedAt, lastCompletedAt: customTime);

        // Then
        participant.LastCompletedAt.ShouldBe(customTime);
    }
}
