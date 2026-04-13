using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for leaderboard domain objects.
/// Maps to: docs/features/social/leaderboards.feature
/// </summary>
public sealed class LeaderboardTests
{
    private static readonly DateTimeOffset _mondayUtc = new(2026, 4, 6, 0, 0, 0, TimeSpan.Zero);

    // ─────────────────────────────────────────────────────────
    // Scenario: View my leaderboard cohort
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PlaceUserInCohortWithin10Levels_When_ViewingLeaderboard()
    {
        // Given — user at level 15
        var entry = new LeaderboardEntry("user-1", "Alice", 100, 1, 15, _mondayUtc);
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc)
            .AddEntry(entry);

        // When — view leaderboard for user at level 15
        LeaderboardCohort cohort = leaderboard.GetCohort(15);

        // Then — cohort range should be 11-20 (within 10 levels)
        cohort.MinLevel.ShouldBe(11);
        cohort.MaxLevel.ShouldBe(20);
        cohort.ContainsLevel(15).ShouldBeTrue();
        cohort.Entries.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SeeRankWithinCohort_When_ViewingLeaderboard()
    {
        // Given — user at level 15 in a cohort
        var entry = new LeaderboardEntry("user-1", "Alice", 200, 1, 15, _mondayUtc);
        var entry2 = new LeaderboardEntry("user-2", "Bob", 100, 2, 13, _mondayUtc.AddHours(1));
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc)
            .AddEntry(entry)
            .AddEntry(entry2);

        // When
        LeaderboardCohort cohort = leaderboard.GetCohort(15);

        // Then — should see rank within cohort
        LeaderboardEntry? myEntry = cohort.EntryForUser("user-1");
        myEntry.ShouldNotBeNull();
        myEntry.Rank.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AssignCohortRange1To10_When_UserIsLevel5()
    {
        (int min, int max) = LeaderboardCohort.CohortRangeForLevel(5);
        min.ShouldBe(1);
        max.ShouldBe(10);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AssignCohortRange11To20_When_UserIsLevel15()
    {
        (int min, int max) = LeaderboardCohort.CohortRangeForLevel(15);
        min.ShouldBe(11);
        max.ShouldBe(20);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AssignCohortRange91To100_When_UserIsLevel100()
    {
        (int min, int max) = LeaderboardCohort.CohortRangeForLevel(100);
        min.ShouldBe(91);
        max.ShouldBe(100);
    }

    // ─────────────────────────────────────────────────────────
    // Scenario: Leaderboard ranks by weekly XP
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RankByXpEarnedThisWeek_When_ViewingWeeklyLeaderboard()
    {
        // Given — multiple users in same cohort with different XP
        var entries = new[]
        {
            new LeaderboardEntry("user-1", "Alice", 300, 1, 15, _mondayUtc),
            new LeaderboardEntry("user-2", "Bob", 500, 1, 13, _mondayUtc),
            new LeaderboardEntry("user-3", "Charlie", 200, 1, 18, _mondayUtc),
        };

        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc);
        foreach (var e in entries)
        {
            leaderboard = leaderboard.AddEntry(e);
        }

        // When
        LeaderboardCohort cohort = leaderboard.GetCohort(15);

        // Then — ranked by XP descending
        cohort.Entries[0].UserId.ShouldBe("user-2");
        cohort.Entries[0].MetricValue.ShouldBe(500);
        cohort.Entries[1].UserId.ShouldBe("user-1");
        cohort.Entries[2].UserId.ShouldBe("user-3");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowTop10Users_When_ViewingCohort()
    {
        // Given — 15 users in the same cohort
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc);
        for (int i = 1; i <= 15; i++)
        {
            leaderboard = leaderboard.AddEntry(
                new LeaderboardEntry($"user-{i}", $"User{i}", 100 * (16 - i), 1, 15, _mondayUtc.AddMinutes(i)));
        }

        // When
        LeaderboardCohort cohort = leaderboard.GetCohort(15);

        // Then — top 10
        IReadOnlyList<LeaderboardEntry> top10 = cohort.TopEntries(10);
        top10.Count.ShouldBe(10);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowOwnRankEvenIfOutsideTop10_When_ViewingCohort()
    {
        // Given — user is ranked 12th
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc);
        for (int i = 1; i <= 15; i++)
        {
            leaderboard = leaderboard.AddEntry(
                new LeaderboardEntry($"user-{i}", $"User{i}", 100 * (16 - i), 1, 15, _mondayUtc.AddMinutes(i)));
        }

        // When
        LeaderboardCohort cohort = leaderboard.GetCohort(15);
        LeaderboardEntry? myEntry = cohort.EntryForUser("user-12");

        // Then — can find own entry even if outside top 10
        myEntry.ShouldNotBeNull();
        myEntry.Rank.ShouldBe(12);
    }

    // ─────────────────────────────────────────────────────────
    // Scenario: Leaderboard resets weekly
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetWeeklyXpToZero_When_NewWeekStarts()
    {
        // Given — a leaderboard with entries
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc)
            .AddEntry(new LeaderboardEntry("user-1", "Alice", 500, 1, 15, _mondayUtc));

        // When — new week starts
        DateTimeOffset newWeekStart = _mondayUtc.AddDays(7);
        (Leaderboard newLeaderboard, LeaderboardHistory history) = leaderboard.Reset(newWeekStart);

        // Then — new leaderboard is empty (XP reset to zero)
        newLeaderboard.Entries.Count.ShouldBe(0);
        newLeaderboard.WeekStart.ShouldBe(newWeekStart);

        // And — history preserves old standings
        history.FinalStandings.Count.ShouldBe(1);
        history.FinalStandings[0].MetricValue.ShouldBe(500);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveHistoryStandings_When_WeekResets()
    {
        // Given
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc)
            .AddEntry(new LeaderboardEntry("user-1", "Alice", 300, 1, 15, _mondayUtc))
            .AddEntry(new LeaderboardEntry("user-2", "Bob", 500, 1, 13, _mondayUtc.AddHours(1)));

        // When
        DateTimeOffset newWeekStart = _mondayUtc.AddDays(7);
        (_, LeaderboardHistory history) = leaderboard.Reset(newWeekStart);

        // Then
        history.WeekStart.ShouldBe(_mondayUtc);
        history.WeekEnd.ShouldBe(newWeekStart);
        history.Type.ShouldBe(LeaderboardType.WeeklyXP);
        history.FinalStandings.Count.ShouldBe(2);
    }

    // ─────────────────────────────────────────────────────────
    // Scenario: Cohort assignment when levelling up mid-week
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemainInCurrentCohort_When_LevellingUpMidWeek()
    {
        // Given — user at level 19, top of cohort 11-20
        // The user's entry is recorded with their level at the start of the week (19)
        var entry = new LeaderboardEntry("user-1", "Alice", 500, 1, 19, _mondayUtc);
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc)
            .AddEntry(entry);

        // When — user levels up to 20 during the week
        // The entry still uses the original level (19) — cohort doesn't change mid-week
        LeaderboardCohort cohort = leaderboard.GetCohort(19);

        // Then — user remains in cohort 11-20
        cohort.ContainsLevel(19).ShouldBeTrue();
        cohort.ContainsLevel(20).ShouldBeTrue();
        cohort.EntryForUser("user-1").ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MoveToNewCohort_When_WeekResets()
    {
        // Given — user at level 19 levelled up to 21 (new cohort 21-30)
        // After reset, new entry uses updated level 21
        var newEntry = new LeaderboardEntry("user-1", "Alice", 0, 1, 21, _mondayUtc.AddDays(7));
        var newLeaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc.AddDays(7))
            .AddEntry(newEntry);

        // When — view cohort for level 21
        LeaderboardCohort cohort = newLeaderboard.GetCohort(21);

        // Then — user is now in cohort 21-30
        cohort.MinLevel.ShouldBe(21);
        cohort.MaxLevel.ShouldBe(30);
        cohort.EntryForUser("user-1").ShouldNotBeNull();
    }

    // ─────────────────────────────────────────────────────────
    // Scenario: Weekly leaderboard resets at a consistent time (Monday 00:00 UTC)
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetAtMondayMidnightUtc_When_WeeklyReset()
    {
        // Given — current week started on Monday 2026-04-06
        // When — checking if reset should occur on the next Monday
        DateTimeOffset nextMonday = new(2026, 4, 13, 0, 0, 0, TimeSpan.Zero);

        // Then — should reset
        Leaderboard.ShouldReset(nextMonday, _mondayUtc).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotResetBeforeMonday_When_CheckingReset()
    {
        // Given — it's Sunday evening
        DateTimeOffset sundayEvening = new(2026, 4, 12, 23, 59, 59, TimeSpan.Zero);

        // Then — should NOT reset yet
        Leaderboard.ShouldReset(sundayEvening, _mondayUtc).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ComputeCorrectWeekStart_When_GivenTimestamp()
    {
        // Given — a Wednesday afternoon UTC
        DateTimeOffset wednesday = new(2026, 4, 8, 14, 30, 0, TimeSpan.Zero);

        // When
        DateTimeOffset weekStart = Leaderboard.GetCurrentWeekStart(wednesday);

        // Then — should be Monday 2026-04-06 00:00 UTC
        weekStart.ShouldBe(_mondayUtc);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ComputeCorrectWeekStart_When_TimestampIsMonday()
    {
        // Given — Monday itself
        DateTimeOffset weekStart = Leaderboard.GetCurrentWeekStart(_mondayUtc);

        // Then
        weekStart.ShouldBe(_mondayUtc);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ComputeCorrectWeekStart_When_TimestampIsSunday()
    {
        // Given — Sunday
        DateTimeOffset sunday = new(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);

        // When
        DateTimeOffset weekStart = Leaderboard.GetCurrentWeekStart(sunday);

        // Then — still the Monday before
        weekStart.ShouldBe(_mondayUtc);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllUsersSeeSameWeekBoundary_When_InDifferentTimezones()
    {
        // Given — timestamps from different timezones at the same UTC moment
        DateTimeOffset utcTime = new(2026, 4, 13, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset tokyoTime = new(2026, 4, 13, 9, 0, 0, TimeSpan.FromHours(9));
        DateTimeOffset nyTime = new(2026, 4, 12, 20, 0, 0, TimeSpan.FromHours(-4));

        // When — all compute week start
        DateTimeOffset ws1 = Leaderboard.GetCurrentWeekStart(utcTime);
        DateTimeOffset ws2 = Leaderboard.GetCurrentWeekStart(tokyoTime);
        DateTimeOffset ws3 = Leaderboard.GetCurrentWeekStart(nyTime);

        // Then — all see the same Monday 00:00 UTC
        ws1.ShouldBe(ws2);
        ws2.ShouldBe(ws3);
    }

    // ─────────────────────────────────────────────────────────
    // Scenario: Level-mismatched users never appear together
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeMismatchedLevelUsers_When_ViewingCohort()
    {
        // Given — user at level 12 and another at level 45
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc)
            .AddEntry(new LeaderboardEntry("user-1", "Alice", 100, 1, 12, _mondayUtc))
            .AddEntry(new LeaderboardEntry("user-2", "Bob", 500, 1, 45, _mondayUtc));

        // When — user at level 12 views leaderboard
        LeaderboardCohort cohort = leaderboard.GetCohort(12);

        // Then — level-45 user should NOT appear
        cohort.EntryForUser("user-2").ShouldBeNull();
        cohort.EntryForUser("user-1").ShouldNotBeNull();
    }

    // ─────────────────────────────────────────────────────────
    // Scenario: View leaderboard types
    // ─────────────────────────────────────────────────────────

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(LeaderboardType.WeeklyXP)]
    [InlineData(LeaderboardType.LongestStreak)]
    [InlineData(LeaderboardType.QuestCloser)]
    public void Should_CreateLeaderboardByType_When_SelectingType(LeaderboardType type)
    {
        // Given / When
        var leaderboard = Leaderboard.Create(type, _mondayUtc)
            .AddEntry(new LeaderboardEntry("user-1", "Alice", 100, 1, 15, _mondayUtc));

        // Then
        leaderboard.Type.ShouldBe(type);
        leaderboard.Entries.Count.ShouldBe(1);
    }

    // ─────────────────────────────────────────────────────────
    // Scenario: View guild leaderboard
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateGuildLeaderboard_When_SelectingGuildType()
    {
        // Given — guild members with contributions
        var leaderboard = Leaderboard.Create(LeaderboardType.Guild, _mondayUtc)
            .AddEntry(new LeaderboardEntry("member-1", "Alice", 300, 1, 15, _mondayUtc))
            .AddEntry(new LeaderboardEntry("member-2", "Bob", 200, 1, 13, _mondayUtc.AddHours(1)));

        // Then — separate guild leaderboard
        leaderboard.Type.ShouldBe(LeaderboardType.Guild);
        leaderboard.Entries.Count.ShouldBe(2);
        leaderboard.Entries[0].MetricValue.ShouldBe(300);
        leaderboard.Entries[1].MetricValue.ShouldBe(200);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RankGuildMembersByContribution_When_ViewingGuildLeaderboard()
    {
        // Given
        var leaderboard = Leaderboard.Create(LeaderboardType.Guild, _mondayUtc)
            .AddEntry(new LeaderboardEntry("member-1", "Alice", 100, 1, 15, _mondayUtc))
            .AddEntry(new LeaderboardEntry("member-2", "Bob", 300, 1, 13, _mondayUtc.AddHours(1)));

        // Then — Bob ranked first due to higher contribution
        leaderboard.Entries[0].UserId.ShouldBe("member-2");
        leaderboard.Entries[0].Rank.ShouldBe(1);
        leaderboard.Entries[1].UserId.ShouldBe("member-1");
        leaderboard.Entries[1].Rank.ShouldBe(2);
    }

    // ─────────────────────────────────────────────────────────
    // Scenario: Opt out of leaderboards
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HideFromRankings_When_OptedOut()
    {
        // Given — user opts out
        var settings = LeaderboardSettings.Default().WithOptOut();
        settings.OptedOut.ShouldBeTrue();

        // When — entry is created with opted-out flag
        var entry = new LeaderboardEntry("user-1", "Alice", 100, 1, 15, _mondayUtc, isOptedOut: true);
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc)
            .AddEntry(entry);

        // Then — should not appear in top entries
        LeaderboardCohort cohort = leaderboard.GetCohort(15);
        IReadOnlyList<LeaderboardEntry> top = cohort.TopEntries(10);
        top.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StillViewLeaderboardAsSpectator_When_OptedOut()
    {
        // Given — opted-out user + visible user
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc)
            .AddEntry(new LeaderboardEntry("user-1", "Alice", 100, 1, 15, _mondayUtc, isOptedOut: true))
            .AddEntry(new LeaderboardEntry("user-2", "Bob", 200, 1, 13, _mondayUtc));

        // When — view cohort
        LeaderboardCohort cohort = leaderboard.GetCohort(15);

        // Then — can see other users (spectating)
        IReadOnlyList<LeaderboardEntry> top = cohort.TopEntries(10);
        top.Count.ShouldBe(1);
        top[0].UserId.ShouldBe("user-2");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SeePlaceholderForOwnRank_When_OptedOut()
    {
        // Given — opted-out user
        var entry = new LeaderboardEntry("user-1", "Alice", 100, 1, 15, _mondayUtc, isOptedOut: true);
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc)
            .AddEntry(entry);

        // When — look up own entry
        LeaderboardCohort cohort = leaderboard.GetCohort(15);
        LeaderboardEntry? myEntry = cohort.EntryForUser("user-1");

        // Then — entry exists with rank (as placeholder) but is marked opted out
        myEntry.ShouldNotBeNull();
        myEntry.IsOptedOut.ShouldBeTrue();
        myEntry.Rank.ShouldBeGreaterThan(0);
    }

    // ─────────────────────────────────────────────────────────
    // Scenario: Anonymous leaderboard participation
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowAsAnonymousQuestor_When_AnonymousModeEnabled()
    {
        // Given — user enables anonymous mode
        var settings = LeaderboardSettings.Default().WithAnonymous();
        settings.Anonymous.ShouldBeTrue();

        // When — entry is created with anonymous flag
        var entry = new LeaderboardEntry("user-1", "Alice", 100, 1, 15, _mondayUtc, isAnonymous: true);

        // Then — display name should be "Anonymous Questor"
        entry.DisplayName.ShouldBe("Anonymous Questor");
        entry.IsAnonymous.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowLevelAndXp_When_Anonymous()
    {
        // Given
        var entry = new LeaderboardEntry("user-1", "Alice", 250, 1, 15, _mondayUtc, isAnonymous: true);

        // Then — level and XP visible, name hidden
        entry.MetricValue.ShouldBe(250);
        entry.UserLevel.ShouldBe(15);
        entry.DisplayName.ShouldBe("Anonymous Questor");
    }

    // ─────────────────────────────────────────────────────────
    // Scenario: Leaderboard tie resolution
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RankEarlierAchievementHigher_When_XpIsTied()
    {
        // Given — two users with same XP, Alice achieved first
        DateTimeOffset aliceTime = _mondayUtc.AddHours(2);
        DateTimeOffset bobTime = _mondayUtc.AddHours(5);

        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc)
            .AddEntry(new LeaderboardEntry("bob", "Bob", 300, 1, 15, bobTime))
            .AddEntry(new LeaderboardEntry("alice", "Alice", 300, 1, 15, aliceTime));

        // Then — Alice ranked higher (earlier achievedAt)
        leaderboard.Entries[0].UserId.ShouldBe("alice");
        leaderboard.Entries[0].Rank.ShouldBe(1);
        leaderboard.Entries[1].UserId.ShouldBe("bob");
        leaderboard.Entries[1].Rank.ShouldBe(2);
    }

    // ─────────────────────────────────────────────────────────
    // LeaderboardEntry validation tests
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EntryUserIdIsEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardEntry("", "Name", 0, 1, 1, _mondayUtc));
        ex.Message.ShouldContain("User ID cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EntryUserIdIsWhitespace()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardEntry("  ", "Name", 0, 1, 1, _mondayUtc));
        ex.Message.ShouldContain("User ID cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EntryDisplayNameIsEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardEntry("user-1", "", 0, 1, 1, _mondayUtc));
        ex.Message.ShouldContain("Display name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EntryDisplayNameIsWhitespace()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardEntry("user-1", "  ", 0, 1, 1, _mondayUtc));
        ex.Message.ShouldContain("Display name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MetricValueIsNegative()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardEntry("user-1", "Name", -1, 1, 1, _mondayUtc));
        ex.Message.ShouldContain("Metric value cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EntryRankIsZero()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardEntry("user-1", "Name", 0, 0, 1, _mondayUtc));
        ex.Message.ShouldContain("Rank must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EntryRankIsNegative()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardEntry("user-1", "Name", 0, -1, 1, _mondayUtc));
        ex.Message.ShouldContain("Rank must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EntryUserLevelIsZero()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardEntry("user-1", "Name", 0, 1, 0, _mondayUtc));
        ex.Message.ShouldContain("User level must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EntryUserLevelIsNegative()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardEntry("user-1", "Name", 0, 1, -1, _mondayUtc));
        ex.Message.ShouldContain("User level must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateEntry_When_ValidParameters()
    {
        var entry = new LeaderboardEntry("user-1", "Alice", 100, 1, 15, _mondayUtc);

        entry.UserId.ShouldBe("user-1");
        entry.DisplayName.ShouldBe("Alice");
        entry.MetricValue.ShouldBe(100);
        entry.Rank.ShouldBe(1);
        entry.UserLevel.ShouldBe(15);
        entry.AchievedAt.ShouldBe(_mondayUtc);
        entry.IsOptedOut.ShouldBeFalse();
        entry.IsAnonymous.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveAnonymousDisplayName_When_WithRankCalled()
    {
        var entry = new LeaderboardEntry("user-1", "Alice", 100, 1, 15, _mondayUtc, isAnonymous: true);
        var reranked = entry.WithRank(5);

        reranked.Rank.ShouldBe(5);
        reranked.DisplayName.ShouldBe("Anonymous Questor");
        reranked.IsAnonymous.ShouldBeTrue();
    }

    // ─────────────────────────────────────────────────────────
    // LeaderboardCohort validation tests
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CohortMinLevelIsZero()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardCohort(0, 10, []));
        ex.Message.ShouldContain("Minimum level must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CohortMaxLevelLessThanMin()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardCohort(10, 5, []));
        ex.Message.ShouldContain("Maximum level cannot be less than minimum level");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CohortRangeExceeds10()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardCohort(1, 12, []));
        ex.Message.ShouldContain("Cohort level range cannot exceed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CohortEntriesNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new LeaderboardCohort(1, 10, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CohortRangeForLevelIsZero()
    {
        var ex = Should.Throw<DomainException>(
            () => LeaderboardCohort.CohortRangeForLevel(0));
        ex.Message.ShouldContain("Level must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CohortRangeForLevelIsNegative()
    {
        var ex = Should.Throw<DomainException>(
            () => LeaderboardCohort.CohortRangeForLevel(-1));
        ex.Message.ShouldContain("Level must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContainLevel_When_LevelIsWithinRange()
    {
        var cohort = new LeaderboardCohort(11, 20, []);
        cohort.ContainsLevel(11).ShouldBeTrue();
        cohort.ContainsLevel(15).ShouldBeTrue();
        cohort.ContainsLevel(20).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotContainLevel_When_LevelIsOutsideRange()
    {
        var cohort = new LeaderboardCohort(11, 20, []);
        cohort.ContainsLevel(10).ShouldBeFalse();
        cohort.ContainsLevel(21).ShouldBeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // LeaderboardHistory validation tests
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_HistoryWeekEndBeforeStart()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardHistory(_mondayUtc, _mondayUtc.AddDays(-1), LeaderboardType.WeeklyXP, []));
        ex.Message.ShouldContain("Week end must be after week start");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_HistoryWeekEndEqualsStart()
    {
        var ex = Should.Throw<DomainException>(
            () => new LeaderboardHistory(_mondayUtc, _mondayUtc, LeaderboardType.WeeklyXP, []));
        ex.Message.ShouldContain("Week end must be after week start");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_HistoryStandingsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new LeaderboardHistory(_mondayUtc, _mondayUtc.AddDays(7), LeaderboardType.WeeklyXP, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateHistory_When_ValidParameters()
    {
        var history = new LeaderboardHistory(_mondayUtc, _mondayUtc.AddDays(7), LeaderboardType.WeeklyXP, []);
        history.WeekStart.ShouldBe(_mondayUtc);
        history.WeekEnd.ShouldBe(_mondayUtc.AddDays(7));
        history.Type.ShouldBe(LeaderboardType.WeeklyXP);
        history.FinalStandings.ShouldBeEmpty();
    }

    // ─────────────────────────────────────────────────────────
    // LeaderboardSettings tests
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultToVisibleAndNonAnonymous_When_Created()
    {
        var settings = LeaderboardSettings.Default();
        settings.OptedOut.ShouldBeFalse();
        settings.Anonymous.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OptOut_When_WithOptOutCalled()
    {
        var settings = LeaderboardSettings.Default().WithOptOut();
        settings.OptedOut.ShouldBeTrue();
        settings.Anonymous.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OptIn_When_WithOptInCalled()
    {
        var settings = LeaderboardSettings.Default().WithOptOut().WithOptIn();
        settings.OptedOut.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EnableAnonymous_When_WithAnonymousCalled()
    {
        var settings = LeaderboardSettings.Default().WithAnonymous();
        settings.Anonymous.ShouldBeTrue();
        settings.OptedOut.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DisableAnonymous_When_WithIdentifiedCalled()
    {
        var settings = LeaderboardSettings.Default().WithAnonymous().WithIdentified();
        settings.Anonymous.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveAnonymous_When_OptingOut()
    {
        var settings = new LeaderboardSettings(false, true).WithOptOut();
        settings.OptedOut.ShouldBeTrue();
        settings.Anonymous.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveOptedOut_When_EnablingAnonymous()
    {
        var settings = new LeaderboardSettings(true, false).WithAnonymous();
        settings.OptedOut.ShouldBeTrue();
        settings.Anonymous.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveOptedOut_When_DisablingAnonymous()
    {
        var settings = new LeaderboardSettings(true, true).WithIdentified();
        settings.OptedOut.ShouldBeTrue();
        settings.Anonymous.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveAnonymous_When_OptingIn()
    {
        var settings = new LeaderboardSettings(true, true).WithOptIn();
        settings.OptedOut.ShouldBeFalse();
        settings.Anonymous.ShouldBeTrue();
    }

    // ─────────────────────────────────────────────────────────
    // Leaderboard validation tests
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_LeaderboardEntriesNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new Leaderboard(LeaderboardType.WeeklyXP, _mondayUtc, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AddingNullEntry()
    {
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc);
        Should.Throw<ArgumentNullException>(
            () => leaderboard.AddEntry(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateEmptyLeaderboard_When_UsingCreate()
    {
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc);
        leaderboard.Type.ShouldBe(LeaderboardType.WeeklyXP);
        leaderboard.WeekStart.ShouldBe(_mondayUtc);
        leaderboard.Entries.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GetNextMondayUtc_When_CalledWithWeekStart()
    {
        DateTimeOffset nextMonday = Leaderboard.GetNextMondayUtc(_mondayUtc);
        nextMonday.ShouldBe(_mondayUtc.AddDays(7));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNullForUser_When_UserNotInCohort()
    {
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc)
            .AddEntry(new LeaderboardEntry("user-1", "Alice", 100, 1, 15, _mondayUtc));

        LeaderboardCohort cohort = leaderboard.GetCohort(15);
        cohort.EntryForUser("nonexistent").ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnEmptyTopEntries_When_CohortHasNoVisibleEntries()
    {
        var cohort = new LeaderboardCohort(1, 10, []);
        cohort.TopEntries(10).ShouldBeEmpty();
    }

    // ─────────────────────────────────────────────────────────
    // LeaderboardEntry.WithRank tests for non-anonymous
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveDisplayName_When_WithRankCalledOnNonAnonymous()
    {
        var entry = new LeaderboardEntry("user-1", "Alice", 100, 1, 15, _mondayUtc);
        var reranked = entry.WithRank(3);

        reranked.Rank.ShouldBe(3);
        reranked.DisplayName.ShouldBe("Alice");
        reranked.IsAnonymous.ShouldBeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // Mutation coverage: boundary / equality edge cases
    // ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeUserAtExactMaxLevel_When_FilteringCohort()
    {
        // Kills mutant: e.UserLevel <= max → e.UserLevel < max (Leaderboard.cs:56)
        var leaderboard = Leaderboard.Create(LeaderboardType.WeeklyXP, _mondayUtc)
            .AddEntry(new LeaderboardEntry("user-1", "Alice", 100, 1, 20, _mondayUtc));

        LeaderboardCohort cohort = leaderboard.GetCohort(15);
        cohort.EntryForUser("user-1").ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowCohortWithSameMinAndMaxLevel_When_Creating()
    {
        // Kills mutant: maxLevel < minLevel → maxLevel <= minLevel (LeaderboardCohort.cs:31)
        var cohort = new LeaderboardCohort(5, 5, []);
        cohort.MinLevel.ShouldBe(5);
        cohort.MaxLevel.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowCohortRangeOfExactly10_When_Creating()
    {
        // Kills mutant: maxLevel - minLevel > CohortLevelRange → >= CohortLevelRange (LeaderboardCohort.cs:36)
        var cohort = new LeaderboardCohort(1, 11, []);
        cohort.MinLevel.ShouldBe(1);
        cohort.MaxLevel.ShouldBe(11);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptLevel1_When_ComputingCohortRange()
    {
        // Kills mutant: level < 1 → level <= 1 (LeaderboardCohort.cs:54)
        (int min, int max) = LeaderboardCohort.CohortRangeForLevel(1);
        min.ShouldBe(1);
        max.ShouldBe(10);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CapMaxLevelAtMaxLevel_When_ComputingCohortRangeForMaxLevel()
    {
        // Ensures Math.Min caps at Level.MaxLevel for the highest bucket
        (int min, int max) = LeaderboardCohort.CohortRangeForLevel(Level.MaxLevel);
        max.ShouldBe(Level.MaxLevel);
        min.ShouldBe(91);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ComputeCorrectMaxLevel_When_BelowMaxLevel()
    {
        (int min, int max) = LeaderboardCohort.CohortRangeForLevel(50);
        max.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptUserLevel1_When_CreatingEntry()
    {
        // Kills mutant: userLevel < 1 → userLevel <= 1 (LeaderboardEntry.cs:52)
        var entry = new LeaderboardEntry("user-1", "Alice", 100, 1, 1, _mondayUtc);
        entry.UserLevel.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseOriginalDisplayName_When_WithRankOnNonAnonymous()
    {
        // Kills mutant: conditional (false) mutation on ternary (LeaderboardEntry.cs:73)
        var entry = new LeaderboardEntry("user-1", "RealName", 100, 1, 15, _mondayUtc);
        var reranked = entry.WithRank(2);
        reranked.DisplayName.ShouldBe("RealName");
        reranked.DisplayName.ShouldNotBe(LeaderboardEntry.AnonymousDisplayName);
    }
}
