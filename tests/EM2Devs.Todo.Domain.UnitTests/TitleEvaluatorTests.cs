using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for TitleEvaluator domain service.
/// Maps to: docs/features/progression/titles-and-ranks.feature
/// Rule: "Titles are earned through sustained behaviour, not one-off achievements"
/// Rule: "Users choose which title to display"
/// Rule: "Titles are permanently earned and never revoked"
/// </summary>
public sealed class TitleEvaluatorTests
{
    private static readonly DateOnly _today = new(2026, 3, 15);

    // =================================================================
    // Scenario Outline: Earn a title through sustained behaviour
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardStreakMasterTitle_When_30DayStreakMaintained()
    {
        // Given — 30 qualifying actions spread across 30 distinct days (sustained)
        var requirement = TitleRequirement.For(TitleType.StreakMaster);
        var actions = BuildSustainedActions(count: 30, distinctDays: 30, startDate: _today.AddDays(-29));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeTrue();
        result.TitleType.ShouldBe(TitleType.StreakMaster);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardEarlyBirdTitle_When_50TasksBefore9AmOver4Weeks()
    {
        // Given — 50 qualifying actions spread across 28 distinct days
        var requirement = TitleRequirement.For(TitleType.EarlyBird);
        var actions = BuildSustainedActions(count: 50, distinctDays: 28, startDate: _today.AddDays(-27));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeTrue();
        result.TitleType.ShouldBe(TitleType.EarlyBird);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardNightOwlTitle_When_50TasksAfter9PmOver4Weeks()
    {
        // Given
        var requirement = TitleRequirement.For(TitleType.NightOwl);
        var actions = BuildSustainedActions(count: 50, distinctDays: 28, startDate: _today.AddDays(-27));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeTrue();
        result.TitleType.ShouldBe(TitleType.NightOwl);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardMorningArchitectTitle_When_ComplexTasksBeforeNoonFor6Weeks()
    {
        // Given — 42 actions spread across 42 distinct days (exceeds both count=30 and days=42)
        var requirement = TitleRequirement.For(TitleType.MorningArchitect);
        var actions = BuildSustainedActions(count: 42, distinctDays: 42, startDate: _today.AddDays(-41));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeTrue();
        result.TitleType.ShouldBe(TitleType.MorningArchitect);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardBossSlayerTitle_When_10BossTasksCompleted()
    {
        // Given — Boss Slayer only requires count, no time spread
        var requirement = TitleRequirement.For(TitleType.BossSlayer);
        var actions = BuildSustainedActions(count: 10, distinctDays: 10, startDate: _today.AddDays(-9));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeTrue();
        result.TitleType.ShouldBe(TitleType.BossSlayer);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardQuestCloserTitle_When_25QuestsCompleted()
    {
        // Given
        var requirement = TitleRequirement.For(TitleType.QuestCloser);
        var actions = BuildSustainedActions(count: 25, distinctDays: 25, startDate: _today.AddDays(-24));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeTrue();
        result.TitleType.ShouldBe(TitleType.QuestCloser);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardConsistentPlannerTitle_When_12WeeklyReviewsCompleted()
    {
        // Given — 12 reviews spread across 12 distinct weeks
        var requirement = TitleRequirement.For(TitleType.ConsistentPlanner);
        var actions = BuildSustainedActions(count: 12, distinctDays: 84, startDate: _today.AddDays(-83));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeTrue();
        result.TitleType.ShouldBe(TitleType.ConsistentPlanner);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardMarathonBuilderTitle_When_60ConsecutiveDaysOnSaga()
    {
        // Given
        var requirement = TitleRequirement.For(TitleType.MarathonBuilder);
        var actions = BuildSustainedActions(count: 60, distinctDays: 60, startDate: _today.AddDays(-59));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeTrue();
        result.TitleType.ShouldBe(TitleType.MarathonBuilder);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardTeamAnchorTitle_When_8WeeksOfGuildContributions()
    {
        // Given — 8 contributions spread across 56 distinct days (8 weeks)
        var requirement = TitleRequirement.For(TitleType.TeamAnchor);
        var actions = BuildSustainedActions(count: 8, distinctDays: 56, startDate: _today.AddDays(-55));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeTrue();
        result.TitleType.ShouldBe(TitleType.TeamAnchor);
    }

    // =================================================================
    // Scenario: Title requires sustained behaviour, not bursts
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotAwardEarlyBirdTitle_When_50TasksCompletedInSingleWeek()
    {
        // Given — 50 qualifying actions but all within a single week (7 distinct days)
        var requirement = TitleRequirement.For(TitleType.EarlyBird);
        var actions = BuildSustainedActions(count: 50, distinctDays: 7, startDate: _today.AddDays(-6));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then — should not be awarded because the time span requirement is not met
        result.IsEarned.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowProgressTowardSustainedRequirement_When_BurstDetected()
    {
        // Given — count met but days not spread enough
        var requirement = TitleRequirement.For(TitleType.EarlyBird);
        var actions = BuildSustainedActions(count: 50, distinctDays: 7, startDate: _today.AddDays(-6));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then — progress should reflect partial completion
        result.IsEarned.ShouldBeFalse();
        result.ProgressPercentage.ShouldBeGreaterThan(0);
        result.ProgressPercentage.ShouldBeLessThan(100);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotAwardStreakMasterTitle_When_30ActionsOnSameDay()
    {
        // Given — 30 actions but all on the same day (1 distinct day)
        var requirement = TitleRequirement.For(TitleType.StreakMaster);
        var actions = BuildSustainedActions(count: 30, distinctDays: 1, startDate: _today);

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeFalse();
    }

    // =================================================================
    // Scenario: Title progress is visible before earning
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Show60PercentProgress_When_18Of30DayStreakCompleted()
    {
        // Given — 18 days of a 30-day streak requirement
        var requirement = TitleRequirement.For(TitleType.StreakMaster);
        var actions = BuildSustainedActions(count: 18, distinctDays: 18, startDate: _today.AddDays(-17));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeFalse();
        result.ProgressPercentage.ShouldBe(60);
        result.RemainingDescription.ShouldBe("12 more days of consistent completions needed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Show0PercentProgress_When_NoActionsCompleted()
    {
        // Given — no qualifying actions
        var requirement = TitleRequirement.For(TitleType.StreakMaster);
        var actions = Array.Empty<TitleQualifyingAction>();

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeFalse();
        result.ProgressPercentage.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Show100PercentProgress_When_TitleEarned()
    {
        // Given — all requirements met
        var requirement = TitleRequirement.For(TitleType.StreakMaster);
        var actions = BuildSustainedActions(count: 30, distinctDays: 30, startDate: _today.AddDays(-29));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeTrue();
        result.ProgressPercentage.ShouldBe(100);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CapProgressAt99_When_CountMetButDaysNotMet()
    {
        // Given — count met (50 actions) but only 14 distinct days (need 28)
        var requirement = TitleRequirement.For(TitleType.EarlyBird);
        var actions = BuildSustainedActions(count: 50, distinctDays: 14, startDate: _today.AddDays(-13));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then — not earned, progress capped below 100
        result.IsEarned.ShouldBeFalse();
        result.ProgressPercentage.ShouldBeLessThan(100);
        result.ProgressPercentage.ShouldBeGreaterThan(0);
    }

    // =================================================================
    // Scenario: Select an active title (uses TitleInventory)
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SetActiveTitle_When_PlayerSelectsEarnedTitle()
    {
        // Given — player has earned two titles
        var inventory = TitleInventory.Empty()
            .AwardTitle(new Title(TitleType.EarlyBird, _today))
            .AwardTitle(new Title(TitleType.BossSlayer, _today));

        // When
        var result = inventory.SelectActiveTitle(TitleType.BossSlayer);

        // Then — Boss Slayer appears as active
        result.ActiveTitle.ShouldBe(TitleType.BossSlayer);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SelectingUnearnedActiveTitle()
    {
        // Given
        var inventory = TitleInventory.Empty()
            .AwardTitle(new Title(TitleType.EarlyBird, _today));

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => inventory.SelectActiveTitle(TitleType.NightOwl));
        ex.Message.ShouldContain("has not been earned");
    }

    // =================================================================
    // Scenario: View all earned titles
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnAllEarnedTitlesWithDates_When_ViewingCollection()
    {
        // Given — 5 earned titles
        var inventory = TitleInventory.Empty()
            .AwardTitle(new Title(TitleType.EarlyBird, _today.AddDays(-30)))
            .AwardTitle(new Title(TitleType.BossSlayer, _today.AddDays(-20)))
            .AwardTitle(new Title(TitleType.StreakMaster, _today.AddDays(-10)))
            .AwardTitle(new Title(TitleType.QuestCloser, _today.AddDays(-5)))
            .AwardTitle(new Title(TitleType.NightOwl, _today));

        // When
        var earnedTitles = inventory.EarnedTitles;

        // Then — all 5 titles with their earn dates
        earnedTitles.Count.ShouldBe(5);
        earnedTitles.ShouldContain(t => t.Type == TitleType.EarlyBird && t.EarnedOn == _today.AddDays(-30));
        earnedTitles.ShouldContain(t => t.Type == TitleType.BossSlayer && t.EarnedOn == _today.AddDays(-20));
        earnedTitles.ShouldContain(t => t.Type == TitleType.StreakMaster && t.EarnedOn == _today.AddDays(-10));
        earnedTitles.ShouldContain(t => t.Type == TitleType.QuestCloser && t.EarnedOn == _today.AddDays(-5));
        earnedTitles.ShouldContain(t => t.Type == TitleType.NightOwl && t.EarnedOn == _today);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowSelectingAnyEarnedTitle_When_ViewingCollection()
    {
        // Given
        var inventory = TitleInventory.Empty()
            .AwardTitle(new Title(TitleType.EarlyBird, _today))
            .AwardTitle(new Title(TitleType.BossSlayer, _today))
            .AwardTitle(new Title(TitleType.StreakMaster, _today));

        // When / Then — can select any earned title
        var result1 = inventory.SelectActiveTitle(TitleType.EarlyBird);
        result1.ActiveTitle.ShouldBe(TitleType.EarlyBird);

        var result2 = inventory.SelectActiveTitle(TitleType.BossSlayer);
        result2.ActiveTitle.ShouldBe(TitleType.BossSlayer);

        var result3 = inventory.SelectActiveTitle(TitleType.StreakMaster);
        result3.ActiveTitle.ShouldBe(TitleType.StreakMaster);
    }

    // =================================================================
    // Scenario: Title retained after behaviour change
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RetainEarlyBirdTitle_When_NoMorningTasksForWeeks()
    {
        // Given — earned Early Bird 30 days ago through consistent morning completions
        var inventory = TitleInventory.Empty()
            .AwardTitle(new Title(TitleType.EarlyBird, _today.AddDays(-30)));

        // When — no morning tasks for 3 weeks (behaviour changed)
        // Titles are permanent — no revocation mechanism exists

        // Then — title still earned and selectable
        inventory.HasTitle(TitleType.EarlyBird).ShouldBeTrue();
        var selected = inventory.SelectActiveTitle(TitleType.EarlyBird);
        selected.ActiveTitle.ShouldBe(TitleType.EarlyBird);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RetainAllTitles_When_BehaviourChangesForMultipleTitles()
    {
        // Given — earned multiple titles over time
        var inventory = TitleInventory.Empty()
            .AwardTitle(new Title(TitleType.EarlyBird, _today.AddDays(-60)))
            .AwardTitle(new Title(TitleType.StreakMaster, _today.AddDays(-30)))
            .AwardTitle(new Title(TitleType.BossSlayer, _today.AddDays(-10)));

        // When — behaviour changes (no activity for weeks)
        // Titles are permanent

        // Then — all still earned
        inventory.EarnedTitles.Count.ShouldBe(3);
        inventory.HasTitle(TitleType.EarlyBird).ShouldBeTrue();
        inventory.HasTitle(TitleType.StreakMaster).ShouldBeTrue();
        inventory.HasTitle(TitleType.BossSlayer).ShouldBeTrue();
    }

    // =================================================================
    // TitleRequirement edge cases and guard clauses
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateRequirement_When_AllTitleTypesRequested()
    {
        // Given / When / Then — every TitleType has a requirement
        foreach (TitleType type in Enum.GetValues<TitleType>())
        {
            var requirement = TitleRequirement.For(type);
            requirement.TitleType.ShouldBe(type);
            requirement.RequiredCount.ShouldBeGreaterThan(0);
            requirement.RequiredDistinctDays.ShouldBeGreaterThan(0);
        }
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_EvaluatingNullRequirement()
    {
        // Given / When / Then
        Should.Throw<ArgumentNullException>(
            () => TitleEvaluator.Evaluate(null!, [], _today));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_EvaluatingNullActions()
    {
        // Given
        var requirement = TitleRequirement.For(TitleType.StreakMaster);

        // When / Then
        Should.Throw<ArgumentNullException>(
            () => TitleEvaluator.Evaluate(requirement, null!, _today));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotAwardTitle_When_CountNotMet()
    {
        // Given — only 29 of required 30 for Streak Master
        var requirement = TitleRequirement.For(TitleType.StreakMaster);
        var actions = BuildSustainedActions(count: 29, distinctDays: 29, startDate: _today.AddDays(-28));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotAwardTitle_When_DistinctDaysNotMet()
    {
        // Given — count met but not enough distinct days
        var requirement = TitleRequirement.For(TitleType.StreakMaster);
        // 30 actions but only 15 distinct days (need 30)
        var actions = BuildSustainedActions(count: 30, distinctDays: 15, startDate: _today.AddDays(-14));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnCorrectRemainingDescription_When_EarlyBirdPartialProgress()
    {
        // Given — 25 of 50 required actions, 14 of 28 required days
        var requirement = TitleRequirement.For(TitleType.EarlyBird);
        var actions = BuildSustainedActions(count: 25, distinctDays: 14, startDate: _today.AddDays(-13));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeFalse();
        result.ProgressPercentage.ShouldBe(50);
        result.RemainingDescription.ShouldBe("25 more qualifying actions needed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnDaysRemainingDescription_When_CountMetButDaysInsufficient()
    {
        // Given — count met (50) but only 14 of 28 required distinct days
        var requirement = TitleRequirement.For(TitleType.EarlyBird);
        var actions = BuildSustainedActions(count: 50, distinctDays: 14, startDate: _today.AddDays(-13));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeFalse();
        result.RemainingDescription.ShouldBe("14 more days of consistent completions needed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnEmptyRemainingDescription_When_TitleEarned()
    {
        // Given — all requirements met
        var requirement = TitleRequirement.For(TitleType.StreakMaster);
        var actions = BuildSustainedActions(count: 30, distinctDays: 30, startDate: _today.AddDays(-29));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeTrue();
        result.RemainingDescription.ShouldBeEmpty();
    }

    // =================================================================
    // TitleQualifyingAction tests
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateQualifyingAction_When_ValidDateProvided()
    {
        // Given / When
        var action = new TitleQualifyingAction(_today);

        // Then
        action.OccurredOn.ShouldBe(_today);
    }

    // =================================================================
    // TitleProgress edge cases
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTitleType_When_ProgressQueried()
    {
        // Given
        var requirement = TitleRequirement.For(TitleType.BossSlayer);
        var actions = BuildSustainedActions(count: 5, distinctDays: 5, startDate: _today.AddDays(-4));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.TitleType.ShouldBe(TitleType.BossSlayer);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExceedRequirements_When_MoreActionsThanRequired()
    {
        // Given — well exceeds requirements
        var requirement = TitleRequirement.For(TitleType.BossSlayer);
        var actions = BuildSustainedActions(count: 20, distinctDays: 20, startDate: _today.AddDays(-19));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then
        result.IsEarned.ShouldBeTrue();
        result.ProgressPercentage.ShouldBe(100);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseMinimumOfCountAndDaysProgress_When_ProgressDiffers()
    {
        // Given — count progress is higher than days progress
        // Streak Master: 30 count, 30 days
        // 25 actions across 15 days => count progress = 83%, days progress = 50%
        // Overall should be min(83, 50) = 50
        var requirement = TitleRequirement.For(TitleType.StreakMaster);
        var actions = BuildSustainedActions(count: 25, distinctDays: 15, startDate: _today.AddDays(-14));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then — should use the lower progress (days=50), not the higher (count=83)
        result.ProgressPercentage.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseCountProgressAsMin_When_DaysProgressIsHigher()
    {
        // Given — days progress is higher than count progress
        // Streak Master: 30 count, 30 days
        // 15 actions across 25 days => count progress = 50%, days progress = 83%
        // Overall should be min(50, 83) = 50
        var requirement = TitleRequirement.For(TitleType.StreakMaster);
        var actions = BuildSustainedActions(count: 15, distinctDays: 25, startDate: _today.AddDays(-24));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then — should use the lower progress (count=50), not the higher (days=83)
        result.ProgressPercentage.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculateExactCountProgress_When_PartialProgressMade()
    {
        // Given — Boss Slayer: 10 count required, 1 day required
        // 3 actions across 1 day => count progress = 30%, days progress = 99 (capped)
        // Overall = min(30, 99) = 30
        var requirement = TitleRequirement.For(TitleType.BossSlayer);
        var actions = BuildSustainedActions(count: 3, distinctDays: 1, startDate: _today);

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then — exact percentage based on count
        result.ProgressPercentage.ShouldBe(30);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowDaysRemainingDescription_When_DaysExactlyMeetRequirement()
    {
        // Given — count met (50), days exactly met (28 of 28) for Early Bird
        var requirement = TitleRequirement.For(TitleType.EarlyBird);
        var actions = BuildSustainedActions(count: 50, distinctDays: 28, startDate: _today.AddDays(-27));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then — both requirements met, should be earned
        result.IsEarned.ShouldBeTrue();
    }

    // =================================================================
    // TitleRequirement action label tests (kill string mutations)
    // =================================================================

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(TitleType.EarlyBird, "qualifying actions")]
    [InlineData(TitleType.MorningArchitect, "qualifying actions")]
    [InlineData(TitleType.NightOwl, "qualifying actions")]
    [InlineData(TitleType.MarathonBuilder, "days of consistent completions")]
    [InlineData(TitleType.BossSlayer, "qualifying actions")]
    [InlineData(TitleType.StreakMaster, "days of consistent completions")]
    [InlineData(TitleType.QuestCloser, "qualifying actions")]
    [InlineData(TitleType.ConsistentPlanner, "qualifying actions")]
    [InlineData(TitleType.TeamAnchor, "qualifying actions")]
    public void Should_ReturnCorrectActionLabel_When_RequirementCreated(
        TitleType type, string expectedLabel)
    {
        // Given / When
        var requirement = TitleRequirement.For(type);

        // Then
        requirement.ActionLabel.ShouldBe(expectedLabel);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeActionLabelInRemainingDescription_When_CountNotMet()
    {
        // Given — Early Bird: 50 count, label = "qualifying actions"
        // 10 actions, plenty of days
        var requirement = TitleRequirement.For(TitleType.EarlyBird);
        var actions = BuildSustainedActions(count: 10, distinctDays: 10, startDate: _today.AddDays(-9));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then — remaining description uses the action label
        result.RemainingDescription.ShouldBe("40 more qualifying actions needed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeStreakActionLabelInDescription_When_StreakCountNotMet()
    {
        // Given — Streak Master has label "days of consistent completions"
        var requirement = TitleRequirement.For(TitleType.StreakMaster);
        var actions = BuildSustainedActions(count: 10, distinctDays: 10, startDate: _today.AddDays(-9));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then — uses the correct label
        result.RemainingDescription.ShouldBe("20 more days of consistent completions needed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowCountRemaining_When_DaysExactlyMetButCountInsufficient()
    {
        // Given — Early Bird: 50 count, 28 days
        // 25 actions across exactly 28 distinct days (days met, count not met)
        var requirement = TitleRequirement.For(TitleType.EarlyBird);
        var actions = BuildSustainedActions(count: 25, distinctDays: 28, startDate: _today.AddDays(-27));

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, _today);

        // Then — days boundary exactly met, so description should be about count
        result.IsEarned.ShouldBeFalse();
        result.RemainingDescription.ShouldBe("25 more qualifying actions needed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentOutOfRange_When_UnknownTitleTypeRequested()
    {
        // Given / When / Then
        var ex = Should.Throw<ArgumentOutOfRangeException>(
            () => TitleRequirement.For((TitleType)999));
        ex.Message.ShouldContain("Unknown title type");
    }

    // =================================================================
    // Helpers
    // =================================================================

    private static TitleQualifyingAction[] BuildSustainedActions(
        int count, int distinctDays, DateOnly startDate)
    {
        var actions = new TitleQualifyingAction[count];
        for (int i = 0; i < count; i++)
        {
            // Spread actions evenly across the distinct days
            int dayOffset = distinctDays > 1
                ? (int)((long)i * (distinctDays - 1) / (count - 1))
                : 0;
            actions[i] = new TitleQualifyingAction(startDate.AddDays(dayOffset));
        }

        return actions;
    }
}
