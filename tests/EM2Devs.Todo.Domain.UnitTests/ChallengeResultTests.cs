using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for ChallengeResult and ChallengeRanking value objects.
/// Maps to: docs/features/social/challenge-mode.feature
/// Rule: "Challenge ends and results are announced"
/// Rule: "Tie resolution in challenge rankings"
/// </summary>
public sealed class ChallengeResultTests
{
    private static readonly Guid _userId1 = Guid.NewGuid();
    private static readonly Guid _userId2 = Guid.NewGuid();
    private static readonly Guid _userId3 = Guid.NewGuid();
    private static readonly Guid _userId4 = Guid.NewGuid();
    private static readonly DateTimeOffset _baseTime = new(2026, 4, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RankParticipantsByTaskCount_When_CreatingResults()
    {
        // Given
        var participants = new List<ChallengeParticipant>
        {
            new(_userId1, 10, _baseTime, lastCompletedAt: _baseTime.AddMinutes(10)),
            new(_userId2, 15, _baseTime, lastCompletedAt: _baseTime.AddMinutes(20)),
            new(_userId3, 8, _baseTime, lastCompletedAt: _baseTime.AddMinutes(30)),
        };

        // When
        var result = ChallengeResult.FromParticipants(participants);

        // Then
        result.Rankings.Count.ShouldBe(3);
        result.Rankings[0].UserId.ShouldBe(_userId2);
        result.Rankings[0].Rank.ShouldBe(1);
        result.Rankings[1].UserId.ShouldBe(_userId1);
        result.Rankings[1].Rank.ShouldBe(2);
        result.Rankings[2].UserId.ShouldBe(_userId3);
        result.Rankings[2].Rank.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeWithdrawnParticipants_When_CreatingResults()
    {
        // Given
        var participants = new List<ChallengeParticipant>
        {
            new(_userId1, 10, _baseTime, withdrawn: true, lastCompletedAt: _baseTime.AddMinutes(10)),
            new(_userId2, 5, _baseTime, lastCompletedAt: _baseTime.AddMinutes(20)),
        };

        // When
        var result = ChallengeResult.FromParticipants(participants);

        // Then
        result.Rankings.Count.ShouldBe(1);
        result.Rankings[0].UserId.ShouldBe(_userId2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BreakTieByEarliestCompletion_When_SameTaskCount()
    {
        // Given — both completed 15 tasks, userId1 reached 15 at minute 15, userId2 at minute 35
        var participants = new List<ChallengeParticipant>
        {
            new(_userId1, 15, _baseTime, lastCompletedAt: _baseTime.AddMinutes(15)),
            new(_userId2, 15, _baseTime, lastCompletedAt: _baseTime.AddMinutes(35)),
        };

        // When
        var result = ChallengeResult.FromParticipants(participants);

        // Then — userId1 ranks higher (reached count first)
        result.Rankings[0].UserId.ShouldBe(_userId1);
        result.Rankings[0].Rank.ShouldBe(1);
        result.Rankings[1].UserId.ShouldBe(_userId2);
        result.Rankings[1].Rank.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardTop3Cosmetics_When_MoreThan3Participants()
    {
        // Given
        var participants = new List<ChallengeParticipant>
        {
            new(_userId1, 20, _baseTime, lastCompletedAt: _baseTime.AddMinutes(10)),
            new(_userId2, 15, _baseTime, lastCompletedAt: _baseTime.AddMinutes(20)),
            new(_userId3, 10, _baseTime, lastCompletedAt: _baseTime.AddMinutes(30)),
            new(_userId4, 5, _baseTime, lastCompletedAt: _baseTime.AddMinutes(40)),
        };

        // When
        var result = ChallengeResult.FromParticipants(participants);

        // Then
        result.Rankings[0].ReceivesCosmetic.ShouldBeTrue();
        result.Rankings[1].ReceivesCosmetic.ShouldBeTrue();
        result.Rankings[2].ReceivesCosmetic.ShouldBeTrue();
        result.Rankings[3].ReceivesCosmetic.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardCorrectXp_When_ResultsCreated()
    {
        // Given
        var participants = new List<ChallengeParticipant>
        {
            new(_userId1, 10, _baseTime, lastCompletedAt: _baseTime.AddMinutes(10)),
            new(_userId2, 5, _baseTime, lastCompletedAt: _baseTime.AddMinutes(20)),
            new(_userId3, 3, _baseTime, lastCompletedAt: _baseTime.AddMinutes(30)),
            new(_userId4, 1, _baseTime, lastCompletedAt: _baseTime.AddMinutes(40)),
        };

        // When
        var result = ChallengeResult.FromParticipants(participants);

        // Then
        int expectedTop3 = ChallengeResult.ParticipationXp + ChallengeResult.Top3BonusXp;
        result.Rankings[0].XpReward.ShouldBe(expectedTop3);
        result.Rankings[1].XpReward.ShouldBe(expectedTop3);
        result.Rankings[2].XpReward.ShouldBe(expectedTop3);
        result.Rankings[3].XpReward.ShouldBe(ChallengeResult.ParticipationXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeFinalized_When_ResultCreated()
    {
        // Given
        var participants = new List<ChallengeParticipant>
        {
            new(_userId1, 5, _baseTime, lastCompletedAt: _baseTime.AddMinutes(10)),
        };

        // When
        var result = ChallengeResult.FromParticipants(participants);

        // Then
        result.IsFinalized.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HandleEmptyParticipants_When_CreatingResults()
    {
        // Given
        var participants = Array.Empty<ChallengeParticipant>();

        // When
        var result = ChallengeResult.FromParticipants(participants);

        // Then
        result.Rankings.Count.ShouldBe(0);
        result.IsFinalized.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NullParticipants()
    {
        var ex = Should.Throw<DomainException>(() =>
            ChallengeResult.FromParticipants(null!));
        ex.Message.ShouldContain("cannot be null");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NullRankings()
    {
        var ex = Should.Throw<DomainException>(() =>
            new ChallengeResult(null!));
        ex.Message.ShouldContain("cannot be null");
    }

    // --- ChallengeRanking validation ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RankingUserIdEmpty()
    {
        var ex = Should.Throw<DomainException>(() =>
            new ChallengeRanking(Guid.Empty, 1, 10, 50, true));
        ex.Message.ShouldContain("user ID");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RankLessThanOne()
    {
        var ex = Should.Throw<DomainException>(() =>
            new ChallengeRanking(_userId1, 0, 10, 50, true));
        ex.Message.ShouldContain("at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RankingTasksNegative()
    {
        var ex = Should.Throw<DomainException>(() =>
            new ChallengeRanking(_userId1, 1, -1, 50, true));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_XpRewardNegative()
    {
        var ex = Should.Throw<DomainException>(() =>
            new ChallengeRanking(_userId1, 1, 10, -1, true));
        ex.Message.ShouldContain("cannot be negative");
    }

    // --- Mutation-killing boundary tests ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptZeroXpReward_When_Creating()
    {
        // Kills mutant: xpReward < 0 -> xpReward <= 0
        var ranking = new ChallengeRanking(_userId1, 1, 10, 0, false);
        ranking.XpReward.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StoreRankings_When_ConstructedDirectly()
    {
        // Kills mutant: statement mutation on Rankings assignment
        var rankings = new List<ChallengeRanking>
        {
            new(_userId1, 1, 10, 150, true),
        };
        var result = new ChallengeResult(rankings);
        result.Rankings.Count.ShouldBe(1);
        result.Rankings[0].UserId.ShouldBe(_userId1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NullParticipantsInFromParticipants()
    {
        // Kills mutant: statement mutation on null guard in FromParticipants
        var ex = Should.Throw<DomainException>(() =>
            ChallengeResult.FromParticipants(null!));
        ex.Message.ShouldBe("Participants cannot be null.");
    }
}
