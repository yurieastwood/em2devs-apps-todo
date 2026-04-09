using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for the persistent PlayerProfile aggregate.
/// Maps to: docs/features/progression/streaks.feature, experience-points.feature
/// </summary>
public sealed class PlayerProfileTests
{
    private static readonly DateOnly _today = new(2026, 4, 7);
    private static readonly DateOnly _yesterday = _today.AddDays(-1);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartWithStartingLevelAndZeroStreak_When_NewProfileCreated()
    {
        // Given / When
        var profile = PlayerProfile.NewProfile();

        // Then
        profile.Level.Value.ShouldBe(1);
        profile.Level.CurrentXp.Value.ShouldBe(0);
        profile.Streak.CurrentDays.ShouldBe(0);
        profile.LongestStreak.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementLevel_When_EnoughXpAwarded()
    {
        // Given — fresh profile
        var profile = PlayerProfile.NewProfile();

        // When — award enough XP to reach level 2 (threshold = 50)
        profile.AwardXp(new ExperiencePoints(60));

        // Then
        profile.Level.Value.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NoOp_When_RecordingCompletionTwiceOnSameDay()
    {
        // Given — fresh profile, recorded once today
        var profile = PlayerProfile.NewProfile();
        profile.RecordCompletion(_today);

        // When — recorded again same day
        profile.RecordCompletion(_today);

        // Then — still 1 day, no double-increment
        profile.Streak.CurrentDays.ShouldBe(1);
        profile.LongestStreak.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementCurrentStreak_When_RecordingCompletionOnConsecutiveDay()
    {
        // Given — fresh profile, completed once yesterday
        var profile = PlayerProfile.NewProfile();
        profile.RecordCompletion(_yesterday);

        // When — completed again today
        profile.RecordCompletion(_today);

        // Then — streak advances to 2
        profile.Streak.CurrentDays.ShouldBe(2);
        profile.LongestStreak.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveLongestStreak_When_CurrentStreakResets()
    {
        // Given — build streak to 5, then reset it
        var profile = PlayerProfile.NewProfile();
        profile.RecordCompletion(new DateOnly(2026, 4, 1));
        profile.RecordCompletion(new DateOnly(2026, 4, 2));
        profile.RecordCompletion(new DateOnly(2026, 4, 3));
        profile.RecordCompletion(new DateOnly(2026, 4, 4));
        profile.RecordCompletion(new DateOnly(2026, 4, 5));
        profile.LongestStreak.ShouldBe(5);

        // When — skip days, then complete again (no grace days)
        profile.RecordCompletion(new DateOnly(2026, 4, 10));

        // Then — current resets to 1, longest preserved
        profile.Streak.CurrentDays.ShouldBe(1);
        profile.LongestStreak.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeReconstructible_When_LoadedFromPersistence()
    {
        // Given — repository hands us raw fields
        var level = new Level(7, new ExperiencePoints(120));
        var streak = new Streak(3, _yesterday, 1);

        // When
        var profile = PlayerProfile.Reconstitute(PlayerProfileId.New(), level, streak, longestStreak: 12);

        // Then
        profile.Level.Value.ShouldBe(7);
        profile.Streak.CurrentDays.ShouldBe(3);
        profile.LongestStreak.ShouldBe(12);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveDistinctIds_When_TwoNewProfilesCreated()
    {
        // Given / When
        var first = PlayerProfile.NewProfile();
        var second = PlayerProfile.NewProfile();

        // Then — strongly-typed IDs are unique per instance
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AwardXpCalledWithNull()
    {
        // Given
        var profile = PlayerProfile.NewProfile();

        // When / Then — null guard on AwardXp must fire before falling through to Level.AddXp
        // (ParamName "xp" distinguishes PlayerProfile.AwardXp's guard from Level.AddXp's "earned")
        var ex = Should.Throw<ArgumentNullException>(() => profile.AwardXp(null!));
        ex.ParamName.ShouldBe("xp");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_When_ProcessDayEndCalledAfterMissedDay()
    {
        // Given — 3-day streak ending yesterday
        var profile = PlayerProfile.NewProfile();
        profile.RecordCompletion(_today.AddDays(-2));
        profile.RecordCompletion(_today.AddDays(-1));
        profile.RecordCompletion(_today);
        profile.Streak.CurrentDays.ShouldBe(3);

        // When — day-end evaluation runs the day after the missed day
        // (streak has no grace days, so it should reset)
        profile.ProcessDayEnd(_today.AddDays(2));

        // Then — current streak reset, longest preserved
        profile.Streak.CurrentDays.ShouldBe(0);
        profile.LongestStreak.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NoOp_When_ProcessDayEndCalledWithNoActiveStreak()
    {
        // Given — fresh profile, no streak at all
        var profile = PlayerProfile.NewProfile();

        // When
        profile.ProcessDayEnd(_today);

        // Then — still zero
        profile.Streak.CurrentDays.ShouldBe(0);
    }
}
