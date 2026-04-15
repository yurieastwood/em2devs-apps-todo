using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for XP history tracking.
/// Maps to: docs/features/progression/experience-points.feature
/// Scenario: "View XP history over time"
/// </summary>
public sealed class XpHistoryTests
{
    private static readonly DateOnly _today = new(2026, 4, 12);

    // --- XpHistoryEntry creation ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateXpHistoryEntry_When_ValidInputsProvided()
    {
        // Given / When
        var xp = new ExperiencePoints(50);
        var entry = new XpHistoryEntry(_today, xp, "task completion", new ExperiencePoints(150));

        // Then
        entry.Date.ShouldBe(_today);
        entry.XpEarned.ShouldBe(xp);
        entry.Source.ShouldBe("task completion");
        entry.CumulativeTotal.Value.ShouldBe(150);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_XpEarnedIsNull()
    {
        // Given / When / Then
        var ex = Should.Throw<ArgumentNullException>(
            () => new XpHistoryEntry(_today, null!, "task completion", new ExperiencePoints(50)));
        ex.ParamName.ShouldBe("xpEarned");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SourceIsEmpty()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new XpHistoryEntry(_today, new ExperiencePoints(10), "", new ExperiencePoints(10)));
        ex.Message.ShouldContain("source");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SourceIsWhitespace()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new XpHistoryEntry(_today, new ExperiencePoints(10), "   ", new ExperiencePoints(10)));
        ex.Message.ShouldContain("source");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CumulativeTotalIsNull()
    {
        // Given / When / Then
        var ex = Should.Throw<ArgumentNullException>(
            () => new XpHistoryEntry(_today, new ExperiencePoints(10), "task completion", null!));
        ex.ParamName.ShouldBe("cumulativeTotal");
    }

    // --- XpHistory collection ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartEmpty_When_NewXpHistoryCreated()
    {
        // Given / When
        var history = XpHistory.Empty();

        // Then
        history.Entries.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecordEntry_When_RecordXpEarningCalled()
    {
        // Given
        var history = XpHistory.Empty();
        var xp = new ExperiencePoints(30);

        // When
        history = history.RecordXpEarning(_today, xp, "task completion");

        // Then
        history.Entries.Count.ShouldBe(1);
        history.Entries[0].Date.ShouldBe(_today);
        history.Entries[0].XpEarned.Value.ShouldBe(30);
        history.Entries[0].Source.ShouldBe("task completion");
        history.Entries[0].CumulativeTotal.Value.ShouldBe(30);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AccumulateCumulativeTotal_When_MultipleEarningsRecorded()
    {
        // Given
        var history = XpHistory.Empty();

        // When
        history = history.RecordXpEarning(_today, new ExperiencePoints(30), "task completion");
        history = history.RecordXpEarning(_today, new ExperiencePoints(20), "quest bonus");
        history = history.RecordXpEarning(_today.AddDays(1), new ExperiencePoints(15), "streak bonus");

        // Then
        history.Entries.Count.ShouldBe(3);
        history.Entries[0].CumulativeTotal.Value.ShouldBe(30);
        history.Entries[1].CumulativeTotal.Value.ShouldBe(50);
        history.Entries[2].CumulativeTotal.Value.ShouldBe(65);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_RecordXpEarningWithNullXp()
    {
        // Given
        var history = XpHistory.Empty();

        // When / Then
        var ex = Should.Throw<ArgumentNullException>(
            () => history.RecordXpEarning(_today, null!, "task completion"));
        ex.ParamName.ShouldBe("xp");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RecordXpEarningWithEmptySource()
    {
        // Given
        var history = XpHistory.Empty();

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => history.RecordXpEarning(_today, new ExperiencePoints(10), ""));
        ex.Message.ShouldContain("source");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RecordXpEarningWithWhitespaceSource()
    {
        // Given
        var history = XpHistory.Empty();

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => history.RecordXpEarning(_today, new ExperiencePoints(10), "  "));
        ex.Message.ShouldContain("source");
    }

    // --- GetDailyTotal ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZero_When_NoDailyEarnings()
    {
        // Given
        var history = XpHistory.Empty();

        // When
        var total = history.GetDailyTotal(_today);

        // Then
        total.Value.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnSumForDay_When_MultipleEarningsOnSameDay()
    {
        // Given
        var history = XpHistory.Empty();
        history = history.RecordXpEarning(_today, new ExperiencePoints(30), "task completion");
        history = history.RecordXpEarning(_today, new ExperiencePoints(20), "quest bonus");
        history = history.RecordXpEarning(_today, new ExperiencePoints(10), "streak bonus");

        // When
        var total = history.GetDailyTotal(_today);

        // Then
        total.Value.ShouldBe(60);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeOtherDays_When_CalculatingDailyTotal()
    {
        // Given
        var history = XpHistory.Empty();
        history = history.RecordXpEarning(_today, new ExperiencePoints(30), "task completion");
        history = history.RecordXpEarning(_today.AddDays(1), new ExperiencePoints(20), "task completion");

        // When
        var total = history.GetDailyTotal(_today);

        // Then
        total.Value.ShouldBe(30);
    }

    // --- GetWeeklyTotal ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZero_When_NoWeeklyEarnings()
    {
        // Given
        var history = XpHistory.Empty();

        // When
        var total = history.GetWeeklyTotal(_today);

        // Then
        total.Value.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnSumForWeek_When_EarningsWithinSevenDays()
    {
        // Given — weekStart is Monday, earnings across the 7-day week
        var weekStart = new DateOnly(2026, 4, 6); // Monday
        var history = XpHistory.Empty();
        history = history.RecordXpEarning(weekStart, new ExperiencePoints(10), "task completion");
        history = history.RecordXpEarning(weekStart.AddDays(2), new ExperiencePoints(20), "quest bonus");
        history = history.RecordXpEarning(weekStart.AddDays(6), new ExperiencePoints(30), "streak bonus");

        // When
        var total = history.GetWeeklyTotal(weekStart);

        // Then
        total.Value.ShouldBe(60);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeOutsideDays_When_CalculatingWeeklyTotal()
    {
        // Given — weekStart is Monday April 6
        var weekStart = new DateOnly(2026, 4, 6);
        var history = XpHistory.Empty();
        history = history.RecordXpEarning(weekStart, new ExperiencePoints(10), "task completion");
        history = history.RecordXpEarning(weekStart.AddDays(7), new ExperiencePoints(50), "next week task"); // outside week

        // When
        var total = history.GetWeeklyTotal(weekStart);

        // Then
        total.Value.ShouldBe(10);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeBeforeWeekStart_When_CalculatingWeeklyTotal()
    {
        // Given — weekStart is Monday April 6
        var weekStart = new DateOnly(2026, 4, 6);
        var history = XpHistory.Empty();
        history = history.RecordXpEarning(weekStart.AddDays(-1), new ExperiencePoints(40), "previous week"); // before week
        history = history.RecordXpEarning(weekStart, new ExperiencePoints(10), "task completion");

        // When
        var total = history.GetWeeklyTotal(weekStart);

        // Then
        total.Value.ShouldBe(10);
    }

    // --- Multiple sources on same day ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TrackSourcesIndependently_When_MultipleDailyEarnings()
    {
        // Given — three different sources on same day
        var history = XpHistory.Empty();
        history = history.RecordXpEarning(_today, new ExperiencePoints(30), "task completion");
        history = history.RecordXpEarning(_today, new ExperiencePoints(15), "quest bonus");
        history = history.RecordXpEarning(_today, new ExperiencePoints(5), "streak bonus");

        // When — query daily total
        var dailyTotal = history.GetDailyTotal(_today);

        // Then — all sources summed
        dailyTotal.Value.ShouldBe(50);

        // And — individual entries preserve source descriptions
        history.Entries[0].Source.ShouldBe("task completion");
        history.Entries[1].Source.ShouldBe("quest bonus");
        history.Entries[2].Source.ShouldBe("streak bonus");
    }

    // --- PlayerProfile integration ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartWithEmptyHistory_When_NewProfileCreated()
    {
        // Given / When
        var profile = PlayerProfile.NewProfile(TestData.TestUserId);

        // Then
        profile.XpHistory.ShouldNotBeNull();
        profile.XpHistory.Entries.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecordXpEarning_When_CalledOnProfile()
    {
        // Given
        var profile = PlayerProfile.NewProfile(TestData.TestUserId);

        // When
        profile.RecordXpEarning(_today, new ExperiencePoints(30), "task completion");

        // Then
        profile.XpHistory.Entries.Count.ShouldBe(1);
        profile.XpHistory.Entries[0].Date.ShouldBe(_today);
        profile.XpHistory.Entries[0].XpEarned.Value.ShouldBe(30);
        profile.XpHistory.Entries[0].Source.ShouldBe("task completion");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AccumulateCumulative_When_MultipleEarningsOnProfile()
    {
        // Given
        var profile = PlayerProfile.NewProfile(TestData.TestUserId);

        // When
        profile.RecordXpEarning(_today, new ExperiencePoints(30), "task completion");
        profile.RecordXpEarning(_today, new ExperiencePoints(20), "quest bonus");

        // Then
        profile.XpHistory.Entries[0].CumulativeTotal.Value.ShouldBe(30);
        profile.XpHistory.Entries[1].CumulativeTotal.Value.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_RecordXpEarningOnProfileWithNullXp()
    {
        // Given
        var profile = PlayerProfile.NewProfile(TestData.TestUserId);

        // When / Then — validation delegated to XpHistory.RecordXpEarning
        var ex = Should.Throw<ArgumentNullException>(
            () => profile.RecordXpEarning(_today, null!, "task completion"));
        ex.ParamName.ShouldBe("xp");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RecordXpEarningOnProfileWithEmptySource()
    {
        // Given
        var profile = PlayerProfile.NewProfile(TestData.TestUserId);

        // When / Then — validation delegated to XpHistoryEntry constructor
        var ex = Should.Throw<DomainException>(
            () => profile.RecordXpEarning(_today, new ExperiencePoints(10), ""));
        ex.Message.ShouldContain("source");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GetDailyTotal_When_QueriedOnProfile()
    {
        // Given
        var profile = PlayerProfile.NewProfile(TestData.TestUserId);
        profile.RecordXpEarning(_today, new ExperiencePoints(30), "task completion");
        profile.RecordXpEarning(_today, new ExperiencePoints(20), "quest bonus");

        // When
        var total = profile.XpHistory.GetDailyTotal(_today);

        // Then
        total.Value.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GetWeeklyTotal_When_QueriedOnProfile()
    {
        // Given
        var weekStart = new DateOnly(2026, 4, 6);
        var profile = PlayerProfile.NewProfile(TestData.TestUserId);
        profile.RecordXpEarning(weekStart, new ExperiencePoints(30), "task completion");
        profile.RecordXpEarning(weekStart.AddDays(3), new ExperiencePoints(20), "quest bonus");

        // When
        var total = profile.XpHistory.GetWeeklyTotal(weekStart);

        // Then
        total.Value.ShouldBe(50);
    }
}
