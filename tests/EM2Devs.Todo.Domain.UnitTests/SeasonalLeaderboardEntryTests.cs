using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for SeasonalLeaderboardEntry value object.
/// Maps to: docs/features/progression/seasons.feature
/// Rule: "Seasonal leaderboards reset each quarter and rank users by seasonal XP"
/// </summary>
public sealed class SeasonalLeaderboardEntryTests
{
    // --- Creation ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateEntry_When_ValidParameters()
    {
        // Given / When
        var entry = new SeasonalLeaderboardEntry("user-1", new ExperiencePoints(500), 1, 10);

        // Then
        entry.UserId.ShouldBe("user-1");
        entry.SeasonalXp.Value.ShouldBe(500);
        entry.Rank.ShouldBe(1);
        entry.UserLevel.ShouldBe(10);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UserIdIsEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new SeasonalLeaderboardEntry("", new ExperiencePoints(0), 1, 1));
        ex.Message.ShouldContain("User ID cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UserIdIsWhitespace()
    {
        var ex = Should.Throw<DomainException>(
            () => new SeasonalLeaderboardEntry("  ", new ExperiencePoints(0), 1, 1));
        ex.Message.ShouldContain("User ID cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_XpIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new SeasonalLeaderboardEntry("user-1", null!, 1, 1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RankIsZero()
    {
        var ex = Should.Throw<DomainException>(
            () => new SeasonalLeaderboardEntry("user-1", new ExperiencePoints(0), 0, 1));
        ex.Message.ShouldContain("Rank must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RankIsNegative()
    {
        var ex = Should.Throw<DomainException>(
            () => new SeasonalLeaderboardEntry("user-1", new ExperiencePoints(0), -1, 1));
        ex.Message.ShouldContain("Rank must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UserLevelIsZero()
    {
        var ex = Should.Throw<DomainException>(
            () => new SeasonalLeaderboardEntry("user-1", new ExperiencePoints(0), 1, 0));
        ex.Message.ShouldContain("User level must be at least 1");
    }

    // --- Scenario: View seasonal leaderboard / cohort ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeInCohort_When_WithinFiveLevels()
    {
        // Given — user at level 10
        var entry = new SeasonalLeaderboardEntry("user-1", new ExperiencePoints(500), 1, 10);

        // When / Then — users within 5 levels should be in cohort
        entry.IsInCohort(5).ShouldBeTrue();   // exactly 5 below
        entry.IsInCohort(15).ShouldBeTrue();  // exactly 5 above
        entry.IsInCohort(10).ShouldBeTrue();  // same level
        entry.IsInCohort(8).ShouldBeTrue();   // 2 below
        entry.IsInCohort(12).ShouldBeTrue();  // 2 above
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeInCohort_When_MoreThanFiveLevelsApart()
    {
        // Given — user at level 10
        var entry = new SeasonalLeaderboardEntry("user-1", new ExperiencePoints(500), 1, 10);

        // When / Then — users more than 5 levels away should NOT be in cohort
        entry.IsInCohort(4).ShouldBeFalse();   // 6 below
        entry.IsInCohort(16).ShouldBeFalse();  // 6 above
    }

    // --- Scenario: Season ends and final ranks are recorded ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecordRank_When_SeasonEnds()
    {
        // Given — user ranked 15th in cohort
        var entry = new SeasonalLeaderboardEntry("user-1", new ExperiencePoints(1200), 15, 10);

        // Then
        entry.Rank.ShouldBe(15);
    }

    // --- Scenario: User joins mid-season ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartWithZeroSeasonalXp_When_JoiningMidSeason()
    {
        // Given / When — new user joins leaderboard with 0 XP
        var entry = new SeasonalLeaderboardEntry("new-user", new ExperiencePoints(0), 100, 1);

        // Then
        entry.SeasonalXp.Value.ShouldBe(0);
        entry.Rank.ShouldBe(100);
    }
}
