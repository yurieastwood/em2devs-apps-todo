using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for WeeklyReview entity and related value objects.
/// Maps to: docs/features/reflection/weekly-review.feature
/// </summary>
public sealed class WeeklyReviewTests
{
    private static readonly DateOnly _weekStart = new(2026, 3, 9); // Monday
    private static readonly DateOnly _nextWeekStart = _weekStart.AddDays(7);
    private static readonly DateOnly _twoWeeksLater = _weekStart.AddDays(14);

    private static WeeklyReviewSummary CreateSummary(
        int tasksCompleted = 24,
        int tasksCreated = 30,
        int questsCompleted = 2,
        int currentStreak = 11,
        int xpEarned = 420) =>
        new(tasksCompleted, tasksCreated, questsCompleted, currentStreak, new ExperiencePoints(xpEarned));

    // ── Scenario: Complete a basic weekly review ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateReviewWithSummaryMetrics_When_StartingWeeklyReview()
    {
        // Given — a free-tier user starts a weekly review
        WeeklyReview review = WeeklyReview.Start(_weekStart);

        // When — summary is set
        WeeklyReviewSummary summary = CreateSummary();
        review.SetSummary(summary);

        // Then — summary metrics are accessible
        review.Summary.ShouldNotBeNull();
        review.Summary.TasksCompleted.ShouldBe(24);
        review.Summary.TasksCreated.ShouldBe(30);
        review.Summary.QuestsCompleted.ShouldBe(2);
        review.Summary.CurrentStreak.ShouldBe(11);
        review.Summary.XpEarned.Value.ShouldBe(420);
        review.Status.ShouldBe(WeeklyReviewStatus.Draft);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SaveReflectionNotes_When_UserEntersText()
    {
        // Given — a review is started
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        review.SetSummary(CreateSummary());

        // When — user enters reflection text
        review.AddReflection("What went well this week?", "Completed all sprint goals");
        review.AddReflection("What could go better next week?", "Better time management");

        // Then — reflections are saved
        review.ReflectionNotes.Count.ShouldBe(2);
        review.ReflectionNotes["What went well this week?"].ShouldBe("Completed all sprint goals");
        review.ReflectionNotes["What could go better next week?"].ShouldBe("Better time management");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CompleteReviewAndAwardXp_When_ReviewSaved()
    {
        // Given — review with summary and reflections
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        review.SetSummary(CreateSummary());
        review.AddReflection("What went well this week?", "Good progress");
        review.AddReflection("What could go better next week?", "More focus");

        // When — review is completed
        ExperiencePoints xp = review.Complete();

        // Then — XP awarded and status is Complete
        xp.Value.ShouldBe(WeeklyReview.ReviewXpReward);
        review.Status.ShouldBe(WeeklyReviewStatus.Complete);
        review.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AppearInHistory_When_ReviewCompleted()
    {
        // Given — multiple completed reviews
        List<WeeklyReview> reviews = [];
        for (int i = 0; i < 6; i++)
        {
            WeeklyReview review = WeeklyReview.Start(_weekStart.AddDays(-7 * i));
            review.SetSummary(CreateSummary());
            review.Complete();
            reviews.Add(review);
        }

        // When — sorted in reverse chronological order
        List<WeeklyReview> history = reviews.OrderByDescending(r => r.WeekStart).ToList();

        // Then — all 6 reviews present, each with summary and status
        history.Count.ShouldBe(6);
        history.First().WeekStart.ShouldBeGreaterThan(history.Last().WeekStart);
        history.ShouldAllBe(r => r.Status == WeeklyReviewStatus.Complete);
        history.ShouldAllBe(r => r.Summary != null);
    }

    // ── Scenario: Earn XP for completing weekly review ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardXp_When_WeeklyReviewCompleted()
    {
        // Given — a review with summary
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        review.SetSummary(CreateSummary());

        // When — completing the review
        ExperiencePoints xp = review.Complete();

        // Then — XP is awarded
        xp.Value.ShouldBe(50);
    }

    // ── Scenario: Start review manually at any time ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartReview_When_ManuallyTriggered()
    {
        // Given/When — user starts review manually
        WeeklyReview review = WeeklyReview.Start(_weekStart);

        // Then — review starts in Draft status
        review.Id.Value.ShouldNotBe(Guid.Empty);
        review.WeekStart.ShouldBe(_weekStart);
        review.Status.ShouldBe(WeeklyReviewStatus.Draft);
    }

    // ── Scenario: Default review schedule when no preference is set ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultToSundayAt6PM_When_NoPreferenceSet()
    {
        // Given/When — default schedule
        ReviewSchedule schedule = ReviewSchedule.Default;

        // Then
        schedule.DayOfWeek.ShouldBe(DayOfWeek.Sunday);
        schedule.TimeOfDay.ShouldBe(new TimeOnly(18, 0));
    }

    // ── Scenario: Configure weekly review schedule ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SetSchedule_When_UserConfiguresDayAndTime()
    {
        // Given/When — user sets schedule to Saturday at 10 AM
        ReviewSchedule schedule = ReviewSchedule.Parse("Saturday at 10 AM");

        // Then
        schedule.DayOfWeek.ShouldBe(DayOfWeek.Saturday);
        schedule.TimeOfDay.ShouldBe(new TimeOnly(10, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateScheduleDirectly_When_DayAndTimeProvided()
    {
        // Given/When
        ReviewSchedule schedule = new(DayOfWeek.Sunday, new TimeOnly(19, 0));

        // Then
        schedule.DayOfWeek.ShouldBe(DayOfWeek.Sunday);
        schedule.TimeOfDay.ShouldBe(new TimeOnly(19, 0));
    }

    // ── Scenario: Review streak builds over consecutive weeks ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementStreak_When_ConsecutiveWeeksCompleted()
    {
        // Given — 11 consecutive weeks
        ReviewStreak streak = new(11, _weekStart);

        // When — completing this week's review
        ReviewStreak result = streak.RecordCompletion(_nextWeekStart);

        // Then — streak is 12
        result.ConsecutiveWeeks.ShouldBe(12);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartNewStreak_When_FirstReviewCompleted()
    {
        // Given — no previous reviews
        ReviewStreak streak = ReviewStreak.NewStreak();

        // When
        ReviewStreak result = streak.RecordCompletion(_weekStart);

        // Then
        result.ConsecutiveWeeks.ShouldBe(1);
        result.LastReviewWeek.ShouldBe(_weekStart);
    }

    // ── Scenario: Missed review does not break streak harshly ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PauseStreak_When_OneWeekMissed()
    {
        // Given — streak of 8 weeks
        ReviewStreak streak = new(8, _weekStart);

        // When — miss one week
        ReviewStreak paused = streak.MissWeek();

        // Then — streak is paused
        paused.IsPaused.ShouldBeTrue();
        paused.ConsecutiveWeeks.ShouldBe(8);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContinueStreak_When_CompletingNextWeekAfterGracePeriod()
    {
        // Given — streak of 8, paused (missed one week)
        ReviewStreak streak = new(8, _weekStart, isPaused: true);

        // When — complete the next week's review (2 weeks after last)
        ReviewStreak result = streak.RecordCompletion(_twoWeeksLater);

        // Then — streak continues from 8
        result.ConsecutiveWeeks.ShouldBe(9);
        result.IsPaused.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_When_GracePeriodExpires()
    {
        // Given — streak paused
        ReviewStreak streak = new(8, _weekStart, isPaused: true);

        // When — miss another week (grace expires)
        ReviewStreak result = streak.MissWeek();

        // Then — streak resets
        result.ConsecutiveWeeks.ShouldBe(0);
        result.IsPaused.ShouldBeFalse();
    }

    // ── Scenario: Complete two missed weeks during the grace period ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CountBothReviews_When_CompletingMissedAndCurrentWeek()
    {
        // Given — streak of 5 weeks, paused (missed 2 weeks)
        ReviewStreak streak = new(5, _weekStart, isPaused: true);

        // When — complete missed week's review
        DateOnly missedWeek = _weekStart.AddDays(7);
        ReviewStreak afterMissed = streak.RecordCompletion(missedWeek);

        // And — complete current week's review
        DateOnly currentWeek = _weekStart.AddDays(14);
        ReviewStreak afterCurrent = afterMissed.RecordCompletion(currentWeek);

        // Then — streak continues: 5 + 2 = 7
        afterCurrent.ConsecutiveWeeks.ShouldBe(7);
        afterCurrent.IsPaused.ShouldBeFalse();
    }

    // ── Scenario: Progress is saved as draft when user logs out mid-review ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SaveAsDraft_When_UserLogsOutMidReview()
    {
        // Given — review started with some reflections
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        review.SetSummary(CreateSummary());
        review.AddReflection("What went well this week?", "Good progress on tasks");

        // When — user logs out (save as draft)
        review.SaveAsDraft();

        // Then — review is in Draft status with reflections preserved
        review.Status.ShouldBe(WeeklyReviewStatus.Draft);
        review.ReflectionNotes.Count.ShouldBe(1);
        review.ReflectionNotes["What went well this week?"].ShouldBe("Good progress on tasks");
    }

    // ── Scenario: Weekly review prompt at scheduled time ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreatePromptNotification_When_ScheduledTimeReached()
    {
        // When — creating a prompt notification
        Notification notification = WeeklyReview.CreatePromptNotification();

        // Then — notification contains estimated time
        notification.Type.ShouldBe(NotificationType.WeeklyReviewPrompt);
        notification.Message.ShouldContain("5 minutes");
    }

    // ── Scenario: Dismiss weekly review prompt ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DismissPrompt_When_UserDismisses()
    {
        // Given — review with prompt
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        // When — user dismisses
        review.DismissPrompt(now);

        // Then — prompt is dismissed
        review.IsPromptDismissed.ShouldBeTrue();
        review.DismissedAt.ShouldBe(now);
        review.Status.ShouldBe(WeeklyReviewStatus.Draft);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TriggerFollowUpReminder_When_24HoursAfterDismiss()
    {
        // Given — prompt dismissed
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        DateTimeOffset dismissTime = new(2026, 3, 15, 19, 0, 0, TimeSpan.Zero);
        review.DismissPrompt(dismissTime);

        // When — 24 hours later
        DateTimeOffset after24Hours = dismissTime.AddHours(24);

        // Then — follow-up should be sent
        review.ShouldSendFollowUpReminder(after24Hours).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotTriggerFollowUp_When_LessThan24HoursAfterDismiss()
    {
        // Given — prompt dismissed
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        DateTimeOffset dismissTime = new(2026, 3, 15, 19, 0, 0, TimeSpan.Zero);
        review.DismissPrompt(dismissTime);

        // When — less than 24 hours later
        DateTimeOffset before24Hours = dismissTime.AddHours(23);

        // Then — no follow-up yet
        review.ShouldSendFollowUpReminder(before24Hours).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotSendMultipleFollowUps_When_AlreadySent()
    {
        // Given — prompt dismissed and follow-up already sent
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        DateTimeOffset dismissTime = new(2026, 3, 15, 19, 0, 0, TimeSpan.Zero);
        review.DismissPrompt(dismissTime);
        review.MarkFollowUpReminderSent();

        // When — 24 hours later
        DateTimeOffset after24Hours = dismissTime.AddHours(24);

        // Then — no further reminders
        review.ShouldSendFollowUpReminder(after24Hours).ShouldBeFalse();
    }

    // ── Scenario: Consistent Planner title progress notification ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_QualifyForConsistentPlanner_When_8OrMoreReviewsCompleted()
    {
        // Given/When — 8 reviews completed
        bool qualifies = WeeklyReview.QualifiesForConsistentPlannerProgress(8);

        // Then
        qualifies.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotQualifyForConsistentPlanner_When_LessThan8Reviews()
    {
        // Given/When — 7 reviews completed
        bool qualifies = WeeklyReview.QualifiesForConsistentPlannerProgress(7);

        // Then
        qualifies.ShouldBeFalse();
    }

    // ── Scenario: Complete an advanced weekly review (premium) ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludePremiumAnalytics_When_PremiumUserStartsReview()
    {
        // Given — premium user
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);
        review.SetSummary(CreateSummary());

        // When — premium data is set
        List<WeeklyReviewSummary> comparisonWeeks =
        [
            CreateSummary(20, 25, 1, 10, 380),
            CreateSummary(22, 28, 2, 9, 400),
            CreateSummary(18, 22, 1, 8, 350),
            CreateSummary(25, 30, 3, 7, 450)
        ];

        review.SetPremiumData(
            comparisonWeeks,
            "Tuesday 9 AM - 12 PM",
            ["Organize photo album", "Tax filing"],
            72,
            ["Quest 'Morning Routine' 80% complete"]);

        // Then — premium fields populated
        review.ComparisonWeeks.ShouldNotBeNull();
        review.ComparisonWeeks.Count.ShouldBe(4);
        review.MostProductiveDayAndTime.ShouldBe("Tuesday 9 AM - 12 PM");
        review.AvoidedTasks.ShouldNotBeNull();
        review.AvoidedTasks.Count.ShouldBe(2);
        review.EstimationAccuracyPercent.ShouldBe(72);
        review.QuestProgressUpdates.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NonPremiumSetsAnalytics()
    {
        // Given — free-tier user
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: false);

        // When/Then
        Should.Throw<DomainException>(() =>
            review.SetPremiumData([], "Tuesday", [], 50, []));
    }

    // ── Scenario: Review surfaces patterns across weeks (premium) ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowTrendInsights_When_PremiumUserHas8Reviews()
    {
        // Given — premium user with 8+ reviews
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);
        review.SetSummary(CreateSummary());

        List<string> insights =
        [
            "Your Tuesday productivity has increased 30% over the last month",
            "You complete more creative tasks in the morning",
            "Your estimation accuracy has improved from 55% to 72%"
        ];

        // When — trend insights are set
        review.SetTrendInsights(insights);

        // Then
        review.TrendInsights.ShouldNotBeNull();
        review.TrendInsights.Count.ShouldBe(3);
        review.TrendInsights[0].ShouldContain("Tuesday productivity");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NonPremiumSetsTrendInsights()
    {
        // Given — free-tier user
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: false);

        // When/Then
        Should.Throw<DomainException>(() =>
            review.SetTrendInsights(["Some insight"]));
    }

    // ── Validation tests ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EmptyReflectionPrompt()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);

        Should.Throw<DomainException>(() => review.AddReflection("", "Some text"));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EmptyReflectionText()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);

        Should.Throw<DomainException>(() => review.AddReflection("Prompt", ""));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompletingWithoutSummary()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);

        Should.Throw<DomainException>(() => review.Complete());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompletingAlreadyCompletedReview()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        review.SetSummary(CreateSummary());
        review.Complete();

        Should.Throw<DomainException>(() => review.Complete());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AddingReflectionToCompletedReview()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        review.SetSummary(CreateSummary());
        review.Complete();

        Should.Throw<DomainException>(() => review.AddReflection("Prompt", "Text"));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SavingCompletedReviewAsDraft()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        review.SetSummary(CreateSummary());
        review.Complete();

        Should.Throw<DomainException>(() => review.SaveAsDraft());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FollowUpWithoutDismissal()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);

        Should.Throw<DomainException>(() => review.MarkFollowUpReminderSent());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotSendFollowUp_When_PromptNotDismissed()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);

        review.ShouldSendFollowUpReminder(DateTimeOffset.UtcNow).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NullSummaryProvided()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);

        Should.Throw<ArgumentNullException>(() => review.SetSummary(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_PremiumDataEstimationOutOfRange()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        Should.Throw<DomainException>(() =>
            review.SetPremiumData([], "Tuesday", [], -1, []));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_PremiumDataEstimationOver100()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        Should.Throw<DomainException>(() =>
            review.SetPremiumData([], "Tuesday", [], 101, []));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EmptyTrendInsights()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        Should.Throw<DomainException>(() =>
            review.SetTrendInsights([]));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NullTrendInsights()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        Should.Throw<ArgumentNullException>(() =>
            review.SetTrendInsights(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NullComparisonWeeks()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        Should.Throw<ArgumentNullException>(() =>
            review.SetPremiumData(null!, "Tuesday", [], 50, []));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EmptyMostProductiveDay()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        Should.Throw<DomainException>(() =>
            review.SetPremiumData([], "", [], 50, []));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NullAvoidedTasks()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        Should.Throw<ArgumentNullException>(() =>
            review.SetPremiumData([], "Tuesday", null!, 50, []));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NullQuestProgressUpdates()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        Should.Throw<ArgumentNullException>(() =>
            review.SetPremiumData([], "Tuesday", [], 50, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveIsPremiumFalse_When_FreeTierUser()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);

        review.IsPremium.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveIsPremiumTrue_When_PremiumUser()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        review.IsPremium.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotReturnSameWeek_When_ReviewAlreadyCompleted()
    {
        ReviewStreak streak = new(5, _weekStart);

        ReviewStreak result = streak.RecordCompletion(_weekStart);

        result.ConsecutiveWeeks.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_When_TooManyWeeksMissed()
    {
        // Given — streak of 5, last review was 3 weeks ago (beyond grace)
        ReviewStreak streak = new(5, _weekStart);
        DateOnly threeWeeksLater = _weekStart.AddDays(21);

        // When — completing a review after too long
        ReviewStreak result = streak.RecordCompletion(threeWeeksLater);

        // Then — streak resets to 1
        result.ConsecutiveWeeks.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnSameInstance_When_MissingWeekWithZeroStreak()
    {
        ReviewStreak streak = new(0, null);

        ReviewStreak result = streak.MissWeek();

        result.ShouldBe(streak);
    }

    // ── Mutation-killing: DomainException message verification ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_EmptyReflectionPrompt()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);

        DomainException ex = Should.Throw<DomainException>(() => review.AddReflection("  ", "text"));
        ex.Message.ShouldContain("Reflection prompt cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_EmptyReflectionText()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);

        DomainException ex = Should.Throw<DomainException>(() => review.AddReflection("Prompt", "  "));
        ex.Message.ShouldContain("Reflection text cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_CompletingAlreadyCompleted()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        review.SetSummary(CreateSummary());
        review.Complete();

        DomainException ex = Should.Throw<DomainException>(() => review.Complete());
        ex.Message.ShouldContain("already complete");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_CompletingWithoutSummary()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);

        DomainException ex = Should.Throw<DomainException>(() => review.Complete());
        ex.Message.ShouldContain("without a summary");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_AddingReflectionToCompleted()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        review.SetSummary(CreateSummary());
        review.Complete();

        DomainException ex = Should.Throw<DomainException>(() => review.AddReflection("Q", "A"));
        ex.Message.ShouldContain("Cannot add reflections to a completed review");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_SavingCompletedAsDraft()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        review.SetSummary(CreateSummary());
        review.Complete();

        DomainException ex = Should.Throw<DomainException>(() => review.SaveAsDraft());
        ex.Message.ShouldContain("Cannot save a completed review as draft");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_FollowUpWithoutDismissal()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart);

        DomainException ex = Should.Throw<DomainException>(() => review.MarkFollowUpReminderSent());
        ex.Message.ShouldContain("Cannot send follow-up reminder when prompt was not dismissed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_NonPremiumSetsAnalytics()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: false);

        DomainException ex = Should.Throw<DomainException>(() =>
            review.SetPremiumData([], "Tuesday", [], 50, []));
        ex.Message.ShouldContain("Premium data is only available for premium users");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_EmptyMostProductiveDay()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        DomainException ex = Should.Throw<DomainException>(() =>
            review.SetPremiumData([], "  ", [], 50, []));
        ex.Message.ShouldContain("Most productive day and time cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_EstimationAccuracyOutOfRange()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        DomainException ex = Should.Throw<DomainException>(() =>
            review.SetPremiumData([], "Tuesday", [], -1, []));
        ex.Message.ShouldContain("Estimation accuracy must be between 0 and 100");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_NonPremiumSetsTrends()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: false);

        DomainException ex = Should.Throw<DomainException>(() =>
            review.SetTrendInsights(["insight"]));
        ex.Message.ShouldContain("Trend insights are only available for premium users");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_EmptyTrendInsights()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        DomainException ex = Should.Throw<DomainException>(() =>
            review.SetTrendInsights([]));
        ex.Message.ShouldContain("Trend insights cannot be empty");
    }

    // ── Mutation-killing: createdAt parameter ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseProvidedCreatedAt_When_Specified()
    {
        DateTimeOffset specificTime = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        WeeklyReview review = WeeklyReview.Start(_weekStart, createdAt: specificTime);

        review.CreatedAt.ShouldBe(specificTime);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseUtcNow_When_CreatedAtNotSpecified()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;
        WeeklyReview review = WeeklyReview.Start(_weekStart);
        DateTimeOffset after = DateTimeOffset.UtcNow;

        review.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
        review.CreatedAt.ShouldBeLessThanOrEqualTo(after);
    }

    // ── Mutation-killing: estimation boundary (0 and 100 exactly) ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptEstimationAccuracyOfZero_When_PremiumUser()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        review.SetPremiumData([], "Tuesday", [], 0, []);

        review.EstimationAccuracyPercent.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptEstimationAccuracyOf100_When_PremiumUser()
    {
        WeeklyReview review = WeeklyReview.Start(_weekStart, isPremium: true);

        review.SetPremiumData([], "Tuesday", [], 100, []);

        review.EstimationAccuracyPercent.ShouldBe(100);
    }

    // ── Mutation-killing: notification message content ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeWeeklyReviewInMessage_When_PromptCreated()
    {
        Notification notification = WeeklyReview.CreatePromptNotification();

        notification.Message.ShouldContain("weekly review");
    }

    // ── Mutation-killing: streak logic - 2 weeks not paused resets ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_When_TwoWeeksMissedAndNotPaused()
    {
        // Streak not paused but 2 weeks gap — should reset
        ReviewStreak streak = new(5, _weekStart, isPaused: false);

        ReviewStreak result = streak.RecordCompletion(_twoWeeksLater);

        result.ConsecutiveWeeks.ShouldBe(1);
    }

    // ── Mutation-killing: ConsistentPlanner threshold boundary ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_QualifyForConsistentPlanner_When_Exactly8Reviews()
    {
        WeeklyReview.QualifiesForConsistentPlannerProgress(8).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_QualifyForConsistentPlanner_When_MoreThan8Reviews()
    {
        WeeklyReview.QualifiesForConsistentPlannerProgress(12).ShouldBeTrue();
    }

    // ── Mutation-killing: constants ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveReviewXpRewardOf50()
    {
        WeeklyReview.ReviewXpReward.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveEstimatedMinutesOf5()
    {
        WeeklyReview.EstimatedMinutes.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveFollowUpReminderHoursOf24()
    {
        WeeklyReview.FollowUpReminderHours.ShouldBe(24);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveConsistentPlannerThresholdOf8()
    {
        WeeklyReview.ConsistentPlannerThreshold.ShouldBe(8);
    }
}
