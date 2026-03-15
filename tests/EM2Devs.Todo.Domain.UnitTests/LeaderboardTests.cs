using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for LeaderboardCohort, LeaderboardEntry.
/// Maps to: docs/features/social/leaderboards.feature
/// Rule: "Leaderboards compare users within similar cohorts only"
/// Rule: "Multiple leaderboard types cater to different motivations"
/// Rule: "Users control their leaderboard visibility"
/// </summary>
public sealed class LeaderboardTests
{
    private static readonly Guid _userId = Guid.NewGuid();

    // --- LeaderboardCohort ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateCohort_When_UserLevelIsValid()
    {
        // Given / When
        var cohort = LeaderboardCohort.ForUserLevel(15);

        // Then — level 15 +/- 10 = 5 to 25
        cohort.MinLevel.ShouldBe(5);
        cohort.MaxLevel.ShouldBe(25);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ClampMinToOne_When_UserLevelIsLow()
    {
        // Given / When
        var cohort = LeaderboardCohort.ForUserLevel(5);

        // Then — min clamped to 1
        cohort.MinLevel.ShouldBe(1);
        cohort.MaxLevel.ShouldBe(15);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeLevel_When_WithinCohortRange()
    {
        // Given
        var cohort = LeaderboardCohort.ForUserLevel(15);

        // When / Then
        cohort.IncludesLevel(15).ShouldBeTrue();
        cohort.IncludesLevel(5).ShouldBeTrue();
        cohort.IncludesLevel(25).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeLevel_When_OutsideCohortRange()
    {
        // Given
        var cohort = LeaderboardCohort.ForUserLevel(12);

        // Then — level 45 is outside range 2-22
        cohort.IncludesLevel(45).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentOutOfRange_When_UserLevelIsZero()
    {
        // Given / When / Then
        var ex = Should.Throw<ArgumentOutOfRangeException>(
            () => LeaderboardCohort.ForUserLevel(0));
        ex.Message.ShouldContain("must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MinLevelIsZero()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardCohort(0, 10));
        ex.Message.ShouldContain("Minimum level must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MaxLessThanMin()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardCohort(10, 5));
        ex.Message.ShouldContain("greater than or equal to minimum");
    }

    // --- LeaderboardEntry ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateEntry_When_ValidParameters()
    {
        // Given / When
        var entry = new LeaderboardEntry(
            _userId, 1, 500, 15, LeaderboardVisibility.Public);

        // Then
        entry.UserId.ShouldBe(_userId);
        entry.Rank.ShouldBe(1);
        entry.Score.ShouldBe(500);
        entry.UserLevel.ShouldBe(15);
        entry.IsVisible.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowUsername_When_VisibilityIsPublic()
    {
        // Given
        var entry = new LeaderboardEntry(
            _userId, 1, 500, 15, LeaderboardVisibility.Public);

        // When
        string name = entry.DisplayName("Jordan");

        // Then
        name.ShouldBe("Jordan");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowAnonymous_When_VisibilityIsAnonymous()
    {
        // Given
        var entry = new LeaderboardEntry(
            _userId, 1, 500, 15, LeaderboardVisibility.Anonymous);

        // When
        string name = entry.DisplayName("Jordan");

        // Then
        name.ShouldBe("Anonymous Questor");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowPlaceholder_When_OptedOut()
    {
        // Given
        var entry = new LeaderboardEntry(
            _userId, 1, 500, 15, LeaderboardVisibility.OptedOut);

        // When / Then
        entry.DisplayName("Jordan").ShouldBe("---");
        entry.IsVisible.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UserIdIsEmpty()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardEntry(Guid.Empty, 1, 100, 10, LeaderboardVisibility.Public));
        ex.Message.ShouldContain("User ID cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RankIsZero()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardEntry(_userId, 0, 100, 10, LeaderboardVisibility.Public));
        ex.Message.ShouldContain("Rank must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ScoreIsNegative()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardEntry(_userId, 1, -1, 10, LeaderboardVisibility.Public));
        ex.Message.ShouldContain("Score cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UserLevelIsZero()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardEntry(_userId, 1, 100, 0, LeaderboardVisibility.Public));
        ex.Message.ShouldContain("User level must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptZeroScore_When_Creating()
    {
        // Given / When
        var entry = new LeaderboardEntry(
            _userId, 1, 0, 1, LeaderboardVisibility.Public);

        // Then
        entry.Score.ShouldBe(0);
        entry.UserLevel.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptLevelOne_When_CreatingEntry()
    {
        // Given / When
        var entry = new LeaderboardEntry(
            _userId, 1, 100, 1, LeaderboardVisibility.Public);

        // Then
        entry.UserLevel.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptCohort_When_MinEqualsMax()
    {
        // Given / When
        var cohort = new LeaderboardCohort(5, 5);

        // Then
        cohort.MinLevel.ShouldBe(5);
        cohort.MaxLevel.ShouldBe(5);
        cohort.IncludesLevel(5).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateCohort_When_UserLevelIsOne()
    {
        // Given / When
        var cohort = LeaderboardCohort.ForUserLevel(1);

        // Then
        cohort.MinLevel.ShouldBe(1);
        cohort.MaxLevel.ShouldBe(11);
    }
}
