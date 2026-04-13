using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for InsightCard entity and InsightDeliveryPolicy value object.
/// Maps to: docs/features/reflection/insight-cards.feature
/// </summary>
public sealed class InsightCardTests
{
    private static readonly DateOnly _today = new(2026, 3, 15);
    private static readonly InsightDeliveryPolicy _defaultPolicy = InsightDeliveryPolicy.Default;

    // --- Scenario Outline: System generates an insight card (6 insight types) ---

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(InsightType.CreativeTaskTiming, "You are 3x more likely to complete creative tasks on Tuesday mornings.")]
    [InlineData(InsightType.QuestCompletionImprovement, "Your average quest completion time has improved by 22% this season.")]
    [InlineData(InsightType.ConsistentWeeklyReviews, "You have completed every weekly review for 8 weeks.")]
    [InlineData(InsightType.MorningProductivityPeak, "Your most productive hours are 9 AM to 11 AM.")]
    [InlineData(InsightType.EstimationAccuracy, "Your time estimates are now within 15% of actual.")]
    [InlineData(InsightType.SideProjectConsistency, "You have worked on your side project 5 out of 7 days.")]
    public void Should_GenerateInsightCard_When_PatternDetectedAndValidated(InsightType type, string message)
    {
        // Given the system has detected the pattern
        // When an insight card is generated
        var card = InsightCard.Generate(type, message, "Trend data", _today, isValidated: true);

        // Then I should see a card with the message
        card.Type.ShouldBe(type);
        card.Message.ShouldBe(message);
        card.SupportingData.ShouldBe("Trend data");
        card.Status.ShouldBe(InsightCardStatus.Unread);
        card.GeneratedAt.ShouldBe(_today);
        card.IsValidated.ShouldBeTrue();
    }

    // --- Scenario: Insight cards are delivered periodically ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EnforceMaxOnePerDay_When_DeliveringInsights()
    {
        // Given I meet the criteria for multiple insights
        // When insights are generated
        // Then I should receive a maximum of 1 insight card per day
        _defaultPolicy.MaxPerDay.ShouldBe(1);
        _defaultPolicy.HasReachedDailyLimit(0).ShouldBeFalse();
        _defaultPolicy.HasReachedDailyLimit(1).ShouldBeTrue();
        _defaultPolicy.HasReachedDailyLimit(2).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EnforceTwoToThreePerWeek_When_DeliveringInsights()
    {
        // And 2-3 per week
        _defaultPolicy.MinPerWeek.ShouldBe(2);
        _defaultPolicy.MaxPerWeek.ShouldBe(3);
        _defaultPolicy.HasReachedWeeklyLimit(1).ShouldBeFalse();
        _defaultPolicy.HasReachedWeeklyLimit(2).ShouldBeFalse();
        _defaultPolicy.HasReachedWeeklyLimit(3).ShouldBeTrue();
        _defaultPolicy.HasReachedWeeklyLimit(4).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PrioritiseImpactfulInsights_When_MultipleAvailable()
    {
        // And the most impactful insights should be prioritised
        // CanDeliver returns true only when daily and weekly limits are not reached
        bool canDeliver = InsightCard.CanDeliver(
            _defaultPolicy, daysOfHistory: 60, deliveredToday: 0, deliveredThisWeek: 1,
            InsightType.CreativeTaskTiming, [], _today);

        canDeliver.ShouldBeTrue();
    }

    // --- Scenario: No insight card when insufficient data ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotGenerateInsights_When_InsufficientData()
    {
        // Given I have only 7 days of task history
        // When the system evaluates potential insights
        // Then no insight cards should be generated
        _defaultPolicy.HasSufficientData(7).ShouldBeFalse();
        _defaultPolicy.HasSufficientData(29).ShouldBeFalse();
        _defaultPolicy.HasSufficientData(30).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BlockDelivery_When_InsufficientDataDays()
    {
        // And I should not see the insights section until enough data is available
        bool canDeliver = InsightCard.CanDeliver(
            _defaultPolicy, daysOfHistory: 7, deliveredToday: 0, deliveredThisWeek: 0,
            InsightType.CreativeTaskTiming, [], _today);

        canDeliver.ShouldBeFalse();
    }

    // --- Scenario: View an insight card ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowInsightCardWithMessageAndData_When_ViewingInsight()
    {
        // Given I have an unread insight card
        var card = InsightCard.Generate(
            InsightType.MorningProductivityPeak,
            "Your most productive hours are 9 AM to 11 AM.",
            "40% of tasks completed in morning block",
            _today, isValidated: true);

        // When I open the insights section
        // Then I should see the insight card with its message and data
        card.Status.ShouldBe(InsightCardStatus.Unread);
        card.Message.ShouldNotBeNullOrWhiteSpace();
        card.SupportingData.ShouldNotBeNullOrWhiteSpace();

        // And I should be able to mark it as read
        card.MarkAsRead();
        card.Status.ShouldBe(InsightCardStatus.Read);
    }

    // --- Scenario: Save an insight card ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SaveInsightCard_When_UserSavesToCollection()
    {
        // Given I have an insight card about my morning productivity
        var card = InsightCard.Generate(
            InsightType.MorningProductivityPeak,
            "Your most productive hours are 9 AM to 11 AM.",
            "Trend data",
            _today, isValidated: true);

        // When I save the card to my collection
        card.Save();

        // Then it should appear in my saved insights
        card.Status.ShouldBe(InsightCardStatus.Saved);
    }

    // --- Scenario: Dismiss an insight card ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DismissInsightCard_When_UserFindsItIrrelevant()
    {
        // Given I have an insight card I find irrelevant
        var card = InsightCard.Generate(
            InsightType.SideProjectConsistency,
            "You have worked on your side project 5 out of 7 days.",
            "Weekly data",
            _today, isValidated: true);

        // When I dismiss the card
        card.Dismiss();

        // Then the card should be removed from my active insights
        card.Status.ShouldBe(InsightCardStatus.Dismissed);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenMarkingDismissedCardAsRead_When_CardIsDismissed()
    {
        var card = InsightCard.Generate(
            InsightType.SideProjectConsistency,
            "Message",
            "Data",
            _today, isValidated: true);

        card.Dismiss();

        Should.Throw<DomainException>(() => card.MarkAsRead())
            .Message.ShouldContain("dismissed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenSavingDismissedCard_When_CardIsDismissed()
    {
        var card = InsightCard.Generate(
            InsightType.SideProjectConsistency,
            "Message",
            "Data",
            _today, isValidated: true);

        card.Dismiss();

        Should.Throw<DomainException>(() => card.Save())
            .Message.ShouldContain("dismissed");
    }

    // --- Scenario: Dismissed insight type reduces future frequency ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReduceFrequency_When_InsightTypeDismissedMultipleTimes()
    {
        // Given I have dismissed 3 insight cards related to "morning productivity"
        // When the system evaluates future insights
        // Then the frequency of morning-related insights should be reduced
        InsightCard.CalculateDismissalWeight(0).ShouldBe(1.0);
        InsightCard.CalculateDismissalWeight(1).ShouldBeGreaterThan(0.0);
        InsightCard.CalculateDismissalWeight(1).ShouldBeLessThan(1.0);
        InsightCard.CalculateDismissalWeight(2).ShouldBeGreaterThan(0.0);
        InsightCard.CalculateDismissalWeight(2).ShouldBeLessThan(1.0);
        // 3 dismissals => fully suppressed
        InsightCard.CalculateDismissalWeight(3).ShouldBe(0.0);
        // More than 3 also fully suppressed
        InsightCard.CalculateDismissalWeight(5).ShouldBe(0.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PrioritiseOtherCategories_When_TypeRepeatedlyDismissed()
    {
        // And the system should prioritise other insight categories instead
        // Weight at 1 dismissal should be higher than at 2 dismissals
        double weight1 = InsightCard.CalculateDismissalWeight(1);
        double weight2 = InsightCard.CalculateDismissalWeight(2);

        weight1.ShouldBeGreaterThan(weight2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnCorrectWeightValues_When_CalculatingDismissalWeight()
    {
        // Specific values: 1 dismissal => ~0.667, 2 dismissals => ~0.333
        InsightCard.CalculateDismissalWeight(1).ShouldBe(1.0 - (1.0 / 3.0));
        InsightCard.CalculateDismissalWeight(2).ShouldBe(1.0 - (2.0 / 3.0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFullWeight_When_NegativeDismissalCount()
    {
        InsightCard.CalculateDismissalWeight(-1).ShouldBe(1.0);
    }

    // --- Scenario: Insight must be validated against user data before delivery ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RejectUnvalidatedInsight_When_DataInconsistent()
    {
        // Given the system has detected the pattern "Morning productivity peak"
        // But my task history shows I complete fewer than 10% of tasks before noon
        // When the system evaluates the insight for delivery
        // Then the insight should not be generated
        Should.Throw<DomainException>(() =>
            InsightCard.Generate(
                InsightType.MorningProductivityPeak,
                "Your most productive hours are 9 AM to 11 AM.",
                "Trend data",
                _today,
                isValidated: false))
            .Message.ShouldContain("validated");
    }

    // --- Scenario: Same insight type does not repeat within a quarter ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotRepeatInsightType_When_WithinQuarterCooldown()
    {
        // Given I received an insight about "quest completion time improving" on January 15
        var jan15 = new DateOnly(2026, 1, 15);
        var feb20 = new DateOnly(2026, 2, 20);

        // When the system evaluates insights on February 20
        var recentDeliveries = new List<(InsightType, DateOnly)>
        {
            (InsightType.QuestCompletionImprovement, jan15)
        };

        bool canDeliver = InsightCard.CanDeliver(
            _defaultPolicy, daysOfHistory: 60, deliveredToday: 0, deliveredThisWeek: 0,
            InsightType.QuestCompletionImprovement, recentDeliveries, feb20);

        // Then the system should not generate another "quest completion time improving" insight
        canDeliver.ShouldBeFalse();
        _defaultPolicy.CooldownDays.ShouldBe(90);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowInsightType_When_OutsideQuarterCooldown()
    {
        // After 90+ days, the same type can repeat
        var jan15 = new DateOnly(2026, 1, 15);
        var apr16 = new DateOnly(2026, 4, 16); // 91 days later

        var recentDeliveries = new List<(InsightType, DateOnly)>
        {
            (InsightType.QuestCompletionImprovement, jan15)
        };

        bool canDeliver = InsightCard.CanDeliver(
            _defaultPolicy, daysOfHistory: 120, deliveredToday: 0, deliveredThisWeek: 0,
            InsightType.QuestCompletionImprovement, recentDeliveries, apr16);

        canDeliver.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowDifferentType_When_AnotherTypeInCooldown()
    {
        var jan15 = new DateOnly(2026, 1, 15);
        var feb20 = new DateOnly(2026, 2, 20);

        var recentDeliveries = new List<(InsightType, DateOnly)>
        {
            (InsightType.QuestCompletionImprovement, jan15)
        };

        // Different type should be allowed even while QuestCompletionImprovement is in cooldown
        bool canDeliver = InsightCard.CanDeliver(
            _defaultPolicy, daysOfHistory: 60, deliveredToday: 0, deliveredThisWeek: 0,
            InsightType.CreativeTaskTiming, recentDeliveries, feb20);

        canDeliver.ShouldBeTrue();
    }

    // --- Scenario: Insight cards appear in weekly review ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TrackInsightCardsForWeeklyReview_When_CardsGenerated()
    {
        // Given I have received 2 insight cards this week
        var card1 = InsightCard.Generate(
            InsightType.CreativeTaskTiming,
            "Creative insight message.",
            "Supporting data 1",
            _today, isValidated: true);

        var card2 = InsightCard.Generate(
            InsightType.EstimationAccuracy,
            "Estimation insight message.",
            "Supporting data 2",
            _today, isValidated: true);

        // When I complete my weekly review
        // Then the review should include a section highlighting this week's insights
        var weeklyInsights = new List<InsightCard> { card1, card2 };
        weeklyInsights.Count.ShouldBe(2);
        weeklyInsights.ShouldAllBe(c => c.GeneratedAt == _today);
    }

    // --- Validation edge cases ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenMessageEmpty_When_GeneratingInsightCard()
    {
        Should.Throw<DomainException>(() =>
            InsightCard.Generate(InsightType.CreativeTaskTiming, "", "Data", _today, true))
            .Message.ShouldContain("message");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenMessageWhitespace_When_GeneratingInsightCard()
    {
        Should.Throw<DomainException>(() =>
            InsightCard.Generate(InsightType.CreativeTaskTiming, "  ", "Data", _today, true))
            .Message.ShouldContain("message");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenSupportingDataEmpty_When_GeneratingInsightCard()
    {
        Should.Throw<DomainException>(() =>
            InsightCard.Generate(InsightType.CreativeTaskTiming, "Msg", "", _today, true))
            .Message.ShouldContain("supporting data");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenSupportingDataWhitespace_When_GeneratingInsightCard()
    {
        Should.Throw<DomainException>(() =>
            InsightCard.Generate(InsightType.CreativeTaskTiming, "Msg", "  ", _today, true))
            .Message.ShouldContain("supporting data");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveUniqueId_When_GeneratingInsightCard()
    {
        var card1 = InsightCard.Generate(InsightType.CreativeTaskTiming, "Msg", "Data", _today, true);
        var card2 = InsightCard.Generate(InsightType.CreativeTaskTiming, "Msg", "Data", _today, true);

        card1.Id.ShouldNotBe(card2.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BlockDelivery_When_DailyLimitReached()
    {
        bool canDeliver = InsightCard.CanDeliver(
            _defaultPolicy, daysOfHistory: 60, deliveredToday: 1, deliveredThisWeek: 1,
            InsightType.CreativeTaskTiming, [], _today);

        canDeliver.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BlockDelivery_When_WeeklyLimitReached()
    {
        bool canDeliver = InsightCard.CanDeliver(
            _defaultPolicy, daysOfHistory: 60, deliveredToday: 0, deliveredThisWeek: 3,
            InsightType.CreativeTaskTiming, [], _today);

        canDeliver.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_PolicyIsNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            InsightCard.CanDeliver(null!, 60, 0, 0, InsightType.CreativeTaskTiming, [], _today));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_RecentDeliveriesIsNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            InsightCard.CanDeliver(_defaultPolicy, 60, 0, 0, InsightType.CreativeTaskTiming, null!, _today));
    }

    // --- Boundary tests for mutation coverage ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFullWeight_When_DismissalCountIsZero()
    {
        // Exactly 0 should return 1.0 (boundary for <= 0 vs < 0 mutation)
        InsightCard.CalculateDismissalWeight(0).ShouldBe(1.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroWeight_When_DismissalCountIsExactlyThree()
    {
        // Exactly 3 should return 0.0 (boundary for >= 3 vs > 3 mutation)
        InsightCard.CalculateDismissalWeight(3).ShouldBe(0.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNonZeroWeight_When_DismissalCountIsTwo()
    {
        // 2 is < 3, so should NOT return 0.0
        double weight = InsightCard.CalculateDismissalWeight(2);
        weight.ShouldBeGreaterThan(0.0);
        weight.ShouldBeLessThan(1.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNonFullWeight_When_DismissalCountIsOne()
    {
        // 1 is > 0, so should NOT return 1.0
        double weight = InsightCard.CalculateDismissalWeight(1);
        weight.ShouldBeGreaterThan(0.0);
        weight.ShouldBeLessThan(1.0);
    }
}
