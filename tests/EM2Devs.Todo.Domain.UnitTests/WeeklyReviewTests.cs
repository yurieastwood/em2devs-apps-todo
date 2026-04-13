using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for the WeeklyReview entity and associated value objects.
/// Tests encode behaviors from weekly-review.feature.
/// </summary>
public sealed class WeeklyReviewTests
{
    private static readonly DateOnly _weekStart = new(2026, 4, 6); // Monday

    // ── Scenario 1: Complete a basic weekly review ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateBasicReviewWithSummaryMetrics_When_StartingWeeklyReview()
    {
        // Given
        var summary = new WeeklyReviewSummary(
            tasksCompleted: 24, tasksCreated: 30, questsCompleted: 2,
            currentStreakDays: 11, xpEarned: 420);

        // When
        WeeklyReview review = WeeklyReview.Create(_weekStart, summary);

        // Then
        review.Id.Value.ShouldNotBe(Guid.Empty);
        review.WeekStart.ShouldBe(_weekStart);
        review.Summary.TasksCompleted.ShouldBe(24);
        review.Summary.TasksCreated.ShouldBe(30);
        review.Summary.QuestsCompleted.ShouldBe(2);
        review.Summary.CurrentStreakDays.ShouldBe(11);
        review.Summary.XpEarned.ShouldBe(420);
        review.Status.ShouldBe(WeeklyReviewStatus.Draft);
        review.IsPremium.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SaveReviewWithReflections_When_CompletingBasicReview()
    {
        // Given
        WeeklyReview review = CreateBasicReview();
        review.AddReflection(WeeklyReview.WentWellPrompt, "Completed the sprint goals.");
        review.AddReflection(WeeklyReview.CouldGoBetterPrompt, "Better time estimation.");

        // When
        ExperiencePoints xp = review.Complete();

        // Then
        review.Status.ShouldBe(WeeklyReviewStatus.Complete);
        review.Reflections.Count.ShouldBe(2);
        review.Reflections[WeeklyReview.WentWellPrompt].ShouldBe("Completed the sprint goals.");
        review.Reflections[WeeklyReview.CouldGoBetterPrompt].ShouldBe("Better time estimation.");
        xp.Value.ShouldBe(WeeklyReview.CompletionXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardXpOnCompletion_When_ReviewCompleted()
    {
        // Given
        WeeklyReview review = CreateBasicReview();
        review.AddReflection(WeeklyReview.WentWellPrompt, "Great week.");
        review.AddReflection(WeeklyReview.CouldGoBetterPrompt, "More focus.");

        // When
        ExperiencePoints xp = review.Complete();

        // Then
        xp.Value.ShouldBe(25);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AppearInReviewHistory_When_ReviewCompleted()
    {
        // Given
        WeeklyReview review = CreateBasicReview();
        review.AddReflection(WeeklyReview.WentWellPrompt, "Good week.");
        review.AddReflection(WeeklyReview.CouldGoBetterPrompt, "Improve planning.");

        // When
        review.Complete();

        // Then — review is complete and has all data for history display
        review.Status.ShouldBe(WeeklyReviewStatus.Complete);
        review.Summary.ShouldNotBeNull();
        review.Reflections.Count.ShouldBeGreaterThanOrEqualTo(2);
        review.CompletedAt.ShouldNotBeNull();
    }

    // ── Scenario 2: Earn XP for completing weekly review ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Receive25Xp_When_CompletingWeeklyReview()
    {
        // Given
        WeeklyReview review = CreateBasicReview();
        review.AddReflection(WeeklyReview.WentWellPrompt, "A");
        review.AddReflection(WeeklyReview.CouldGoBetterPrompt, "B");

        // When
        ExperiencePoints xp = review.Complete();

        // Then
        xp.Value.ShouldBe(25);
        WeeklyReview.CompletionXp.ShouldBe(25);
    }

    // ── Scenario 3: View past weekly reviews ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RetainSummaryAndReflections_When_ReviewIsCompleted()
    {
        // Given — simulate 6 completed reviews
        List<WeeklyReview> reviews = [];
        for (int i = 0; i < 6; i++)
        {
            DateOnly weekStart = _weekStart.AddDays(-7 * (5 - i));
            var summary = new WeeklyReviewSummary(10 + i, 12 + i, 1, 5 + i, 100 + i * 10);
            WeeklyReview review = WeeklyReview.Create(weekStart, summary);
            review.AddReflection(WeeklyReview.WentWellPrompt, $"Week {i} went well.");
            review.AddReflection(WeeklyReview.CouldGoBetterPrompt, $"Week {i} improvement.");
            review.Complete();
            reviews.Add(review);
        }

        // When — sorted in reverse chronological order
        List<WeeklyReview> history = reviews.OrderByDescending(r => r.WeekStart).ToList();

        // Then
        history.Count.ShouldBe(6);
        history[0].WeekStart.ShouldBeGreaterThan(history[5].WeekStart);
        history.ShouldAllBe(r => r.Summary != null);
        history.ShouldAllBe(r => r.Reflections.Count >= 2);
    }

    // ── Scenario 4: Start review manually at any time ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateReview_When_StartedManuallyRegardlessOfSchedule()
    {
        // Given — no schedule constraint on creation
        var summary = new WeeklyReviewSummary(5, 10, 0, 3, 50);

        // When — creating a review at any time
        WeeklyReview review = WeeklyReview.Create(_weekStart, summary);

        // Then
        review.Status.ShouldBe(WeeklyReviewStatus.Draft);
        review.Summary.ShouldNotBeNull();
    }

    // ── Scenario 5: Default review schedule ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultToSundayAt6PM_When_NoPreferenceSet()
    {
        // When
        ReviewSchedule schedule = ReviewSchedule.Default();

        // Then
        schedule.DayOfWeek.ShouldBe(DayOfWeek.Sunday);
        schedule.TimeOfDay.ShouldBe(new TimeOnly(18, 0));
    }

    // ── Scenario 6: Configure weekly review schedule ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UpdateSchedule_When_UserConfiguresReviewTime()
    {
        // When
        var schedule = new ReviewSchedule(DayOfWeek.Saturday, new TimeOnly(10, 0));

        // Then
        schedule.DayOfWeek.ShouldBe(DayOfWeek.Saturday);
        schedule.TimeOfDay.ShouldBe(new TimeOnly(10, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MatchScheduledTime_When_DayAndTimeAlign()
    {
        // Given
        var schedule = new ReviewSchedule(DayOfWeek.Saturday, new TimeOnly(10, 0));

        // When / Then
        schedule.IsScheduledTime(DayOfWeek.Saturday, new TimeOnly(10, 0)).ShouldBeTrue();
        schedule.IsScheduledTime(DayOfWeek.Sunday, new TimeOnly(10, 0)).ShouldBeFalse();
        schedule.IsScheduledTime(DayOfWeek.Saturday, new TimeOnly(11, 0)).ShouldBeFalse();
    }

    // ── Scenario 7: Review streak builds over weeks ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementStreakTo12_When_12ConsecutiveWeeksCompleted()
    {
        // Given
        ReviewStreak streak = ReviewStreak.New();
        DateOnly firstWeek = _weekStart.AddDays(-77); // 11 weeks back

        // Build up 11 consecutive weeks
        for (int i = 0; i < 11; i++)
        {
            streak = streak.RecordCompletion(firstWeek.AddDays(7 * i));
        }

        streak.ConsecutiveWeeks.ShouldBe(11);

        // When — complete the 12th week
        streak = streak.RecordCompletion(firstWeek.AddDays(77));

        // Then
        streak.ConsecutiveWeeks.ShouldBe(12);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReachConsistentPlannerThreshold_When_4ConsecutiveWeeks()
    {
        // Given
        ReviewStreak streak = ReviewStreak.New();
        DateOnly firstWeek = _weekStart.AddDays(-21);

        for (int i = 0; i < 4; i++)
        {
            streak = streak.RecordCompletion(firstWeek.AddDays(7 * i));
        }

        // Then
        streak.ConsecutiveWeeks.ShouldBe(4);
        streak.HasReachedConsistentPlannerThreshold().ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotReachConsistentPlannerThreshold_When_Under4Weeks()
    {
        // Given
        ReviewStreak streak = ReviewStreak.New();
        DateOnly firstWeek = _weekStart.AddDays(-14);

        for (int i = 0; i < 3; i++)
        {
            streak = streak.RecordCompletion(firstWeek.AddDays(7 * i));
        }

        // Then
        streak.ConsecutiveWeeks.ShouldBe(3);
        streak.HasReachedConsistentPlannerThreshold().ShouldBeFalse();
    }

    // ── Scenario 8: Missed review does not break streak harshly ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PauseStreak_When_OneWeekMissed()
    {
        // Given — streak of 8 weeks
        ReviewStreak streak = ReviewStreak.New();
        DateOnly firstWeek = _weekStart.AddDays(-56);

        for (int i = 0; i < 8; i++)
        {
            streak = streak.RecordCompletion(firstWeek.AddDays(7 * i));
        }

        streak.ConsecutiveWeeks.ShouldBe(8);

        // When — miss one week, process week end
        DateOnly missedWeek = firstWeek.AddDays(56 + 7);
        streak = streak.ProcessWeekEnd(missedWeek);

        // Then
        streak.IsPaused.ShouldBeTrue();
        streak.ConsecutiveWeeks.ShouldBe(8);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContinueStreak_When_CompletingAfterGracePeriod()
    {
        // Given — streak of 8, missed one week (paused)
        ReviewStreak streak = ReviewStreak.New();
        DateOnly firstWeek = _weekStart.AddDays(-63);

        for (int i = 0; i < 8; i++)
        {
            streak = streak.RecordCompletion(firstWeek.AddDays(7 * i));
        }

        // Miss one week
        DateOnly missedWeek = firstWeek.AddDays(56 + 7);
        streak = streak.ProcessWeekEnd(missedWeek);
        streak.IsPaused.ShouldBeTrue();

        // When — complete next week's review (within grace)
        DateOnly catchupWeek = firstWeek.AddDays(56 + 14);
        streak = streak.RecordCompletion(catchupWeek);

        // Then
        streak.ConsecutiveWeeks.ShouldBe(9);
        streak.IsPaused.ShouldBeFalse();
    }

    // ── Scenario 9: Complete two missed weeks during grace period ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContinueStreak_When_CompletingBothMissedAndCurrentWeeks()
    {
        // Given — streak of 5 weeks
        ReviewStreak streak = ReviewStreak.New();
        DateOnly firstWeek = _weekStart.AddDays(-42);

        for (int i = 0; i < 5; i++)
        {
            streak = streak.RecordCompletion(firstWeek.AddDays(7 * i));
        }

        streak.ConsecutiveWeeks.ShouldBe(5);

        // When — complete the missed week's review (week 6)
        DateOnly missedWeek = firstWeek.AddDays(35);
        streak = streak.RecordCompletion(missedWeek);
        streak.ConsecutiveWeeks.ShouldBe(6);

        // And complete the current week's review (week 7)
        DateOnly currentWeek = firstWeek.AddDays(42);
        streak = streak.RecordCompletion(currentWeek);

        // Then
        streak.ConsecutiveWeeks.ShouldBe(7);
        streak.IsPaused.ShouldBeFalse();
    }

    // ── Scenario 10: Progress saved as draft when user logs out mid-review ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SaveAsDraft_When_UserLogsOutMidReview()
    {
        // Given — started review with partial reflections
        WeeklyReview review = CreateBasicReview();
        review.AddReflection(WeeklyReview.WentWellPrompt, "Good progress on the project.");

        // When — user logs out (save as draft)
        bool savedAsDraft = review.SaveAsDraft();

        // Then
        savedAsDraft.ShouldBeTrue();
        review.Status.ShouldBe(WeeklyReviewStatus.Draft);
        review.CanResume.ShouldBeTrue();
        review.Reflections.Count.ShouldBe(1);
        review.Reflections[WeeklyReview.WentWellPrompt].ShouldBe("Good progress on the project.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveReflectionText_When_ResumingDraftReview()
    {
        // Given — draft review with partial reflection
        WeeklyReview review = CreateBasicReview();
        review.AddReflection(WeeklyReview.WentWellPrompt, "Made good progress.");

        // When — resume and verify
        review.CanResume.ShouldBeTrue();

        // Then — previously entered text is preserved
        review.Reflections[WeeklyReview.WentWellPrompt].ShouldBe("Made good progress.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotSaveAsDraft_When_ReviewAlreadyComplete()
    {
        // Given — completed review
        WeeklyReview review = CreateBasicReview();
        review.AddReflection(WeeklyReview.WentWellPrompt, "A");
        review.AddReflection(WeeklyReview.CouldGoBetterPrompt, "B");
        review.Complete();

        // When
        bool savedAsDraft = review.SaveAsDraft();

        // Then
        savedAsDraft.ShouldBeFalse();
        review.CanResume.ShouldBeFalse();
    }

    // ── Scenario 11: Weekly review prompt at scheduled time ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MatchSchedule_When_ScheduledTimeReached()
    {
        // Given
        var schedule = new ReviewSchedule(DayOfWeek.Sunday, new TimeOnly(19, 0));

        // When / Then
        schedule.IsScheduledTime(DayOfWeek.Sunday, new TimeOnly(19, 0)).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IndicateEstimatedTime_When_ReviewPromptCreated()
    {
        // Then — the estimated time constant is 5 minutes
        WeeklyReview.EstimatedMinutes.ShouldBe(5);
    }

    // ── Scenario 12: Dismiss weekly review prompt ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateDismissableNotification_When_ReviewPromptIssued()
    {
        // Given
        Notification notification = Notification.Create(
            NotificationType.WeeklyReviewPrompt,
            "Time for your weekly review! Estimated time: 5 minutes");

        // When
        notification.Dismiss();

        // Then
        notification.IsDismissed.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateFollowUpReminder_When_NotificationDismissed()
    {
        // Given — dismissed notification
        Notification notification = Notification.Create(
            NotificationType.WeeklyReviewPrompt,
            "Time for your weekly review! Estimated time: 5 minutes");
        notification.Dismiss();

        // When — follow-up reminder created 24 hours later
        Notification followUp = Notification.Create(
            NotificationType.WeeklyReviewPrompt,
            "Reminder: You haven't completed your weekly review yet.");

        // Then
        followUp.Type.ShouldBe(NotificationType.WeeklyReviewPrompt);
        followUp.IsDismissed.ShouldBeFalse();
    }

    // ── Scenario 13: Consistent Planner title progress notification ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotifyConsistentPlannerProgress_When_4ConsecutiveReviews()
    {
        // Given
        ReviewStreak streak = ReviewStreak.New();
        DateOnly firstWeek = _weekStart.AddDays(-21);

        for (int i = 0; i < 4; i++)
        {
            streak = streak.RecordCompletion(firstWeek.AddDays(7 * i));
        }

        // Then
        streak.HasReachedConsistentPlannerThreshold().ShouldBeTrue();
        ReviewStreak.ConsistentPlannerThreshold.ShouldBe(4);
    }

    // ── Scenario 14: Complete an advanced weekly review (Premium) ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeAdvancedData_When_PremiumUserCompletesReview()
    {
        // Given
        var summary = new WeeklyReviewSummary(30, 35, 3, 14, 500);
        var pastWeeks = new List<WeeklyReviewSummary>
        {
            new(25, 30, 2, 10, 400),
            new(20, 28, 1, 8, 350),
            new(22, 25, 2, 7, 380),
            new(28, 32, 3, 12, 450)
        };

        // When
        WeeklyReview review = WeeklyReview.CreatePremium(
            _weekStart,
            summary,
            pastWeeks,
            mostProductiveDay: "Tuesday",
            mostProductiveTimeWindow: "9 AM - 12 PM",
            avoidedTasks: ["Clean up backlog", "Write documentation"],
            estimationAccuracyPercent: 72,
            questProgressUpdates: ["Quest A: 80% complete"]);

        // Then
        review.IsPremium.ShouldBeTrue();
        review.Summary.ShouldBe(summary);
        review.PastWeeksSummaries!.Count.ShouldBe(4);
        review.MostProductiveDay.ShouldBe("Tuesday");
        review.MostProductiveTimeWindow.ShouldBe("9 AM - 12 PM");
        review.AvoidedTasks!.Count.ShouldBe(2);
        review.EstimationAccuracyPercent.ShouldBe(72);
        review.QuestProgressUpdates!.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SavePremiumReviewWithReflections_When_Completed()
    {
        // Given
        WeeklyReview review = CreatePremiumReview();
        review.AddReflection(WeeklyReview.WentWellPrompt, "Great productivity.");
        review.AddReflection(WeeklyReview.CouldGoBetterPrompt, "Need better estimation.");

        // When
        ExperiencePoints xp = review.Complete();

        // Then
        review.Status.ShouldBe(WeeklyReviewStatus.Complete);
        xp.Value.ShouldBe(25);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeTrendInsights_When_PremiumReviewHasHistory()
    {
        // Given
        var summary = new WeeklyReviewSummary(30, 35, 3, 14, 500);
        var pastWeeks = new List<WeeklyReviewSummary>
        {
            new(25, 30, 2, 10, 400),
            new(20, 28, 1, 8, 350),
            new(22, 25, 2, 7, 380),
            new(28, 32, 3, 12, 450)
        };
        var trends = new List<string>
        {
            "Your Tuesday productivity has increased 30% over the last month",
            "You complete more creative tasks in the morning",
            "Your estimation accuracy has improved from 55% to 72%"
        };

        // When
        WeeklyReview review = WeeklyReview.CreatePremium(
            _weekStart,
            summary,
            pastWeeks,
            mostProductiveDay: "Tuesday",
            mostProductiveTimeWindow: "9 AM - 12 PM",
            avoidedTasks: [],
            estimationAccuracyPercent: 72,
            questProgressUpdates: [],
            trendInsights: trends);

        // Then
        review.TrendInsights.ShouldNotBeNull();
        review.TrendInsights!.Count.ShouldBe(3);
    }

    // ── Validation tests ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TasksCompletedIsNegative()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new WeeklyReviewSummary(-1, 10, 0, 0, 0));
        ex.Message.ShouldContain("Tasks completed cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TasksCreatedIsNegative()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new WeeklyReviewSummary(10, -1, 0, 0, 0));
        ex.Message.ShouldContain("Tasks created cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_QuestsCompletedIsNegative()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new WeeklyReviewSummary(10, 10, -1, 0, 0));
        ex.Message.ShouldContain("Quests completed cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CurrentStreakDaysIsNegative()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new WeeklyReviewSummary(10, 10, 0, -1, 0));
        ex.Message.ShouldContain("Current streak days cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_XpEarnedIsNegative()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new WeeklyReviewSummary(10, 10, 0, 0, -1));
        ex.Message.ShouldContain("XP earned cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ReviewStreakWeeksAreNegative()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new ReviewStreak(-1, null, false));
        ex.Message.ShouldContain("Review streak weeks cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreatingReviewWithNullSummary()
    {
        Should.Throw<ArgumentNullException>(() =>
            WeeklyReview.Create(_weekStart, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AddingEmptyReflectionPrompt()
    {
        WeeklyReview review = CreateBasicReview();
        DomainException ex = Should.Throw<DomainException>(() =>
            review.AddReflection("", "Some response"));
        ex.Message.ShouldContain("Reflection prompt cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AddingEmptyReflectionResponse()
    {
        WeeklyReview review = CreateBasicReview();
        DomainException ex = Should.Throw<DomainException>(() =>
            review.AddReflection("A prompt", ""));
        ex.Message.ShouldContain("Reflection response cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompletingReviewWithInsufficientReflections()
    {
        WeeklyReview review = CreateBasicReview();
        review.AddReflection(WeeklyReview.WentWellPrompt, "Only one reflection.");

        DomainException ex = Should.Throw<DomainException>(() => review.Complete());
        ex.Message.ShouldContain("at least two reflections");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompletingAlreadyCompleteReview()
    {
        WeeklyReview review = CreateBasicReview();
        review.AddReflection(WeeklyReview.WentWellPrompt, "A");
        review.AddReflection(WeeklyReview.CouldGoBetterPrompt, "B");
        review.Complete();

        DomainException ex = Should.Throw<DomainException>(() => review.Complete());
        ex.Message.ShouldContain("already complete");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AddingReflectionToCompletedReview()
    {
        WeeklyReview review = CreateBasicReview();
        review.AddReflection(WeeklyReview.WentWellPrompt, "A");
        review.AddReflection(WeeklyReview.CouldGoBetterPrompt, "B");
        review.Complete();

        DomainException ex = Should.Throw<DomainException>(() =>
            review.AddReflection("Extra", "Should not work"));
        ex.Message.ShouldContain("Cannot add reflections to a completed review");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_PremiumReviewHasEmptyMostProductiveDay()
    {
        var summary = new WeeklyReviewSummary(10, 10, 0, 0, 0);
        DomainException ex = Should.Throw<DomainException>(() =>
            WeeklyReview.CreatePremium(
                _weekStart, summary, [], "", "9-12",
                [], 50, []));
        ex.Message.ShouldContain("Most productive day cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_PremiumReviewHasEmptyTimeWindow()
    {
        var summary = new WeeklyReviewSummary(10, 10, 0, 0, 0);
        DomainException ex = Should.Throw<DomainException>(() =>
            WeeklyReview.CreatePremium(
                _weekStart, summary, [], "Tuesday", "",
                [], 50, []));
        ex.Message.ShouldContain("Most productive time window cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EstimationAccuracyIsNegative()
    {
        var summary = new WeeklyReviewSummary(10, 10, 0, 0, 0);
        DomainException ex = Should.Throw<DomainException>(() =>
            WeeklyReview.CreatePremium(
                _weekStart, summary, [], "Tuesday", "9 AM - 12 PM",
                [], -1, []));
        ex.Message.ShouldContain("Estimation accuracy must be between 0 and 100");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EstimationAccuracyExceeds100()
    {
        var summary = new WeeklyReviewSummary(10, 10, 0, 0, 0);
        DomainException ex = Should.Throw<DomainException>(() =>
            WeeklyReview.CreatePremium(
                _weekStart, summary, [], "Tuesday", "9 AM - 12 PM",
                [], 101, []));
        ex.Message.ShouldContain("Estimation accuracy must be between 0 and 100");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_PremiumReviewHasNullSummary()
    {
        Should.Throw<ArgumentNullException>(() =>
            WeeklyReview.CreatePremium(
                _weekStart, null!, [], "Tuesday", "9 AM - 12 PM",
                [], 50, []));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_PremiumReviewHasNullPastWeeks()
    {
        var summary = new WeeklyReviewSummary(10, 10, 0, 0, 0);
        Should.Throw<ArgumentNullException>(() =>
            WeeklyReview.CreatePremium(
                _weekStart, summary, null!, "Tuesday", "9 AM - 12 PM",
                [], 50, []));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_PremiumReviewHasNullAvoidedTasks()
    {
        var summary = new WeeklyReviewSummary(10, 10, 0, 0, 0);
        Should.Throw<ArgumentNullException>(() =>
            WeeklyReview.CreatePremium(
                _weekStart, summary, [], "Tuesday", "9 AM - 12 PM",
                null!, 50, []));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_PremiumReviewHasNullQuestProgress()
    {
        var summary = new WeeklyReviewSummary(10, 10, 0, 0, 0);
        Should.Throw<ArgumentNullException>(() =>
            WeeklyReview.CreatePremium(
                _weekStart, summary, [], "Tuesday", "9 AM - 12 PM",
                [], 50, null!));
    }

    // ── ReviewStreak edge cases ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartNewStreak_When_FirstReviewCompleted()
    {
        // Given
        ReviewStreak streak = ReviewStreak.New();

        // When
        streak = streak.RecordCompletion(_weekStart);

        // Then
        streak.ConsecutiveWeeks.ShouldBe(1);
        streak.LastReviewWeekStart.ShouldBe(_weekStart);
        streak.IsPaused.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotChangeStreak_When_SameWeekCompletedTwice()
    {
        // Given
        ReviewStreak streak = ReviewStreak.New();
        streak = streak.RecordCompletion(_weekStart);

        // When
        streak = streak.RecordCompletion(_weekStart);

        // Then
        streak.ConsecutiveWeeks.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_When_GapExceedsGracePeriod()
    {
        // Given — streak of 3 weeks
        ReviewStreak streak = ReviewStreak.New();
        DateOnly firstWeek = _weekStart.AddDays(-14);

        for (int i = 0; i < 3; i++)
        {
            streak = streak.RecordCompletion(firstWeek.AddDays(7 * i));
        }

        // When — skip 3 weeks (21 days gap)
        DateOnly farFutureWeek = firstWeek.AddDays(14 + 21);
        streak = streak.RecordCompletion(farFutureWeek);

        // Then
        streak.ConsecutiveWeeks.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotChangeState_When_ProcessWeekEndWithNoHistory()
    {
        // Given
        ReviewStreak streak = ReviewStreak.New();

        // When
        ReviewStreak result = streak.ProcessWeekEnd(_weekStart);

        // Then
        result.ConsecutiveWeeks.ShouldBe(0);
        result.LastReviewWeekStart.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotPause_When_ProcessWeekEndForCurrentWeek()
    {
        // Given
        ReviewStreak streak = ReviewStreak.New();
        streak = streak.RecordCompletion(_weekStart);

        // When — same week
        ReviewStreak result = streak.ProcessWeekEnd(_weekStart);

        // Then
        result.IsPaused.ShouldBeFalse();
        result.ConsecutiveWeeks.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_When_AlreadyPausedAndAnotherWeekMissed()
    {
        // Given — streak of 3, then paused
        ReviewStreak streak = ReviewStreak.New();
        DateOnly firstWeek = _weekStart.AddDays(-21);

        for (int i = 0; i < 3; i++)
        {
            streak = streak.RecordCompletion(firstWeek.AddDays(7 * i));
        }

        // Pause it (miss one week)
        DateOnly missedWeek = firstWeek.AddDays(21 + 7);
        streak = streak.ProcessWeekEnd(missedWeek);
        streak.IsPaused.ShouldBeTrue();

        // When — another week passes while still paused
        DateOnly anotherWeek = firstWeek.AddDays(21 + 14);
        streak = streak.ProcessWeekEnd(anotherWeek);

        // Then — streak resets
        streak.ConsecutiveWeeks.ShouldBe(0);
        streak.IsPaused.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotPause_When_ProcessWeekEndWithinOneWeek()
    {
        // Given
        ReviewStreak streak = ReviewStreak.New();
        streak = streak.RecordCompletion(_weekStart);

        // When — next week (7 days later)
        ReviewStreak result = streak.ProcessWeekEnd(_weekStart.AddDays(7));

        // Then — within 7 days, no pause
        result.IsPaused.ShouldBeFalse();
        result.ConsecutiveWeeks.ShouldBe(1);
    }

    // ── ReviewSchedule additional tests ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveCorrectDefaultConstants_When_Inspected()
    {
        ReviewSchedule.DefaultDayOfWeek.ShouldBe(DayOfWeek.Sunday);
        ReviewSchedule.DefaultTimeOfDay.ShouldBe(new TimeOnly(18, 0));
    }

    // ── WeeklyReviewSummary equality ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeEqual_When_SameSummaryValues()
    {
        var a = new WeeklyReviewSummary(10, 12, 2, 5, 100);
        var b = new WeeklyReviewSummary(10, 12, 2, 5, 100);
        a.ShouldBe(b);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqual_When_DifferentSummaryValues()
    {
        var a = new WeeklyReviewSummary(10, 12, 2, 5, 100);
        var b = new WeeklyReviewSummary(11, 12, 2, 5, 100);
        a.ShouldNotBe(b);
    }

    // ── ReviewSchedule equality ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeEqual_When_SameScheduleValues()
    {
        var a = new ReviewSchedule(DayOfWeek.Monday, new TimeOnly(9, 0));
        var b = new ReviewSchedule(DayOfWeek.Monday, new TimeOnly(9, 0));
        a.ShouldBe(b);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqual_When_DifferentScheduleDay()
    {
        var a = new ReviewSchedule(DayOfWeek.Monday, new TimeOnly(9, 0));
        var b = new ReviewSchedule(DayOfWeek.Tuesday, new TimeOnly(9, 0));
        a.ShouldNotBe(b);
    }

    // ── ReviewStreak equality ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeEqual_When_SameStreakValues()
    {
        var a = new ReviewStreak(5, _weekStart, false);
        var b = new ReviewStreak(5, _weekStart, false);
        a.ShouldBe(b);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqual_When_DifferentStreakWeeks()
    {
        var a = new ReviewStreak(5, _weekStart, false);
        var b = new ReviewStreak(6, _weekStart, false);
        a.ShouldNotBe(b);
    }

    // ── WeeklyReviewId tests ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateUniqueIds_When_NewCalled()
    {
        WeeklyReviewId id1 = WeeklyReviewId.New();
        WeeklyReviewId id2 = WeeklyReviewId.New();
        id1.ShouldNotBe(id2);
        id1.Value.ShouldNotBe(Guid.Empty);
    }

    // ── Mutation-killing boundary tests ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowZeroTasksCompleted_When_CreatingSummary()
    {
        var summary = new WeeklyReviewSummary(0, 10, 0, 0, 0);
        summary.TasksCompleted.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowZeroTasksCreated_When_CreatingSummary()
    {
        var summary = new WeeklyReviewSummary(10, 0, 0, 0, 0);
        summary.TasksCreated.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseProvidedCreatedAt_When_ExplicitTimestampGiven()
    {
        // Given
        var summary = new WeeklyReviewSummary(10, 10, 0, 0, 0);
        var explicitTime = new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

        // When
        WeeklyReview review = WeeklyReview.Create(_weekStart, summary, explicitTime);

        // Then
        review.CreatedAt.ShouldBe(explicitTime);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseUtcNow_When_NoCreatedAtProvided()
    {
        // Given
        var summary = new WeeklyReviewSummary(10, 10, 0, 0, 0);
        DateTimeOffset before = DateTimeOffset.UtcNow;

        // When
        WeeklyReview review = WeeklyReview.Create(_weekStart, summary);

        // Then
        review.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
        review.CreatedAt.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseProvidedCreatedAt_When_PremiumReviewHasExplicitTimestamp()
    {
        // Given
        var summary = new WeeklyReviewSummary(10, 10, 0, 0, 0);
        var explicitTime = new DateTimeOffset(2026, 2, 20, 14, 0, 0, TimeSpan.Zero);

        // When
        WeeklyReview review = WeeklyReview.CreatePremium(
            _weekStart, summary, [], "Tuesday", "9 AM - 12 PM",
            [], 50, [],
            createdAt: explicitTime);

        // Then
        review.CreatedAt.ShouldBe(explicitTime);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseUtcNow_When_PremiumReviewHasNoCreatedAt()
    {
        // Given
        var summary = new WeeklyReviewSummary(10, 10, 0, 0, 0);
        DateTimeOffset before = DateTimeOffset.UtcNow;

        // When
        WeeklyReview review = WeeklyReview.CreatePremium(
            _weekStart, summary, [], "Tuesday", "9 AM - 12 PM",
            [], 50, []);

        // Then
        review.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
        review.CreatedAt.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowEstimationAccuracyOfZero_When_PremiumReview()
    {
        var summary = new WeeklyReviewSummary(10, 10, 0, 0, 0);
        WeeklyReview review = WeeklyReview.CreatePremium(
            _weekStart, summary, [], "Tuesday", "9 AM - 12 PM",
            [], 0, []);
        review.EstimationAccuracyPercent.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowEstimationAccuracyOf100_When_PremiumReview()
    {
        var summary = new WeeklyReviewSummary(10, 10, 0, 0, 0);
        WeeklyReview review = WeeklyReview.CreatePremium(
            _weekStart, summary, [], "Tuesday", "9 AM - 12 PM",
            [], 100, []);
        review.EstimationAccuracyPercent.ShouldBe(100);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateNewStreakFromNew_When_Inspected()
    {
        // Given / When
        ReviewStreak streak = ReviewStreak.New();

        // Then
        streak.ConsecutiveWeeks.ShouldBe(0);
        streak.LastReviewWeekStart.ShouldBeNull();
        streak.IsPaused.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_When_PausedAndTwoMoreWeeksMissed()
    {
        // Given — streak of 3, then paused
        ReviewStreak streak = ReviewStreak.New();
        DateOnly firstWeek = _weekStart.AddDays(-21);

        for (int i = 0; i < 3; i++)
        {
            streak = streak.RecordCompletion(firstWeek.AddDays(7 * i));
        }

        // Pause at week 5 (2 weeks after last review)
        streak = streak.ProcessWeekEnd(firstWeek.AddDays(28));
        streak.IsPaused.ShouldBeTrue();

        // When — process week end again while paused (3 weeks after last review)
        streak = streak.ProcessWeekEnd(firstWeek.AddDays(35));

        // Then
        streak.ConsecutiveWeeks.ShouldBe(0);
        streak.IsPaused.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PauseNotReset_When_ExactlyTwoWeeksSinceLast()
    {
        // Given — streak of 2
        ReviewStreak streak = ReviewStreak.New();
        streak = streak.RecordCompletion(_weekStart);
        streak = streak.RecordCompletion(_weekStart.AddDays(7));

        // When — 14 days since last review
        streak = streak.ProcessWeekEnd(_weekStart.AddDays(21));

        // Then — should pause, not reset
        streak.IsPaused.ShouldBeTrue();
        streak.ConsecutiveWeeks.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_When_GapIsExactly15DaysNotPaused()
    {
        // Given — streak of 2
        ReviewStreak streak = ReviewStreak.New();
        streak = streak.RecordCompletion(_weekStart);
        streak = streak.RecordCompletion(_weekStart.AddDays(7));

        // When — 15 days gap (more than 14, not paused)
        DateOnly farWeek = _weekStart.AddDays(7 + 15);
        streak = streak.ProcessWeekEnd(farWeek);

        // Then — beyond 14 days without being paused should reset
        streak.ConsecutiveWeeks.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContinueStreak_When_CompletingMissedWeekDirectly()
    {
        // Given — streak of 2, then 14-day gap (missed 1 week)
        ReviewStreak streak = ReviewStreak.New();
        streak = streak.RecordCompletion(_weekStart);
        streak = streak.RecordCompletion(_weekStart.AddDays(7));

        // When — complete 14 days later (not paused, just catching up)
        streak = streak.RecordCompletion(_weekStart.AddDays(21));

        // Then
        streak.ConsecutiveWeeks.ShouldBe(3);
        streak.IsPaused.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateReviewStreakWithPaused_When_ConstructedAsPaused()
    {
        // Given / When
        var streak = new ReviewStreak(5, _weekStart, true);

        // Then
        streak.IsPaused.ShouldBeTrue();
        streak.ConsecutiveWeeks.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotIncrementStreak_When_SameWeekCompletedAgain()
    {
        // Given — build up a streak of 3 so that duplicate detection matters
        ReviewStreak streak = ReviewStreak.New();
        streak = streak.RecordCompletion(_weekStart);
        streak = streak.RecordCompletion(_weekStart.AddDays(7));
        streak = streak.RecordCompletion(_weekStart.AddDays(14));
        streak.ConsecutiveWeeks.ShouldBe(3);

        // When — same week again
        ReviewStreak result = streak.RecordCompletion(_weekStart.AddDays(14));

        // Then — should return same instance, no change at all
        result.ConsecutiveWeeks.ShouldBe(3);
        result.LastReviewWeekStart.ShouldBe(_weekStart.AddDays(14));
        result.IsPaused.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBePaused_When_StreakResets()
    {
        // Given — streak of 2
        ReviewStreak streak = ReviewStreak.New();
        streak = streak.RecordCompletion(_weekStart);
        streak = streak.RecordCompletion(_weekStart.AddDays(7));

        // When — large gap resets streak
        streak = streak.RecordCompletion(_weekStart.AddDays(7 + 28));

        // Then
        streak.ConsecutiveWeeks.ShouldBe(1);
        streak.IsPaused.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContinueStreakFromPaused_When_CompletingWithin21DayGap()
    {
        // Given — streak of 3, paused
        ReviewStreak streak = ReviewStreak.New();
        DateOnly firstWeek = _weekStart.AddDays(-28);

        for (int i = 0; i < 3; i++)
        {
            streak = streak.RecordCompletion(firstWeek.AddDays(7 * i));
        }

        // Pause it
        streak = streak.ProcessWeekEnd(firstWeek.AddDays(28));
        streak.IsPaused.ShouldBeTrue();

        // When — complete 21 days after last review (paused path)
        streak = streak.RecordCompletion(firstWeek.AddDays(14 + 21));

        // Then
        streak.ConsecutiveWeeks.ShouldBe(4);
        streak.IsPaused.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_When_NotPausedAndGapExceeds14Days()
    {
        // Given — streak of 2, not paused, but 21-day gap
        ReviewStreak streak = ReviewStreak.New();
        streak = streak.RecordCompletion(_weekStart);
        streak = streak.RecordCompletion(_weekStart.AddDays(7));
        streak.IsPaused.ShouldBeFalse();

        // When — 21 days later, not paused
        streak = streak.RecordCompletion(_weekStart.AddDays(7 + 21));

        // Then — should reset since not paused
        streak.ConsecutiveWeeks.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_When_ProcessWeekEndWithLargeGapNotPaused()
    {
        // Given — streak of 2, not paused
        ReviewStreak streak = ReviewStreak.New();
        streak = streak.RecordCompletion(_weekStart);
        streak = streak.RecordCompletion(_weekStart.AddDays(7));

        // When — 21 days gap (>14), not paused
        streak = streak.ProcessWeekEnd(_weekStart.AddDays(7 + 21));

        // Then
        streak.ConsecutiveWeeks.ShouldBe(0);
        streak.IsPaused.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_VerifyGracePeriodConstant_When_Inspected()
    {
        ReviewStreak.GracePeriodWeeks.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Pause_When_ProcessWeekEndExactly14DaysAndNotPaused()
    {
        // Given
        ReviewStreak streak = ReviewStreak.New();
        streak = streak.RecordCompletion(_weekStart);

        // When — exactly 14 days later
        streak = streak.ProcessWeekEnd(_weekStart.AddDays(14));

        // Then
        streak.IsPaused.ShouldBeTrue();
        streak.ConsecutiveWeeks.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Reset_When_ProcessWeekEndExactly14DaysAndAlreadyPaused()
    {
        // Given — paused streak
        var streak = new ReviewStreak(3, _weekStart, true);

        // When — exactly 14 days later while already paused
        streak = streak.ProcessWeekEnd(_weekStart.AddDays(14));

        // Then — should reset
        streak.ConsecutiveWeeks.ShouldBe(0);
    }

    // ── Helper methods ──

    private static WeeklyReview CreateBasicReview()
    {
        var summary = new WeeklyReviewSummary(24, 30, 2, 11, 420);
        return WeeklyReview.Create(_weekStart, summary);
    }

    private static WeeklyReview CreatePremiumReview()
    {
        var summary = new WeeklyReviewSummary(30, 35, 3, 14, 500);
        var pastWeeks = new List<WeeklyReviewSummary>
        {
            new(25, 30, 2, 10, 400),
            new(20, 28, 1, 8, 350),
            new(22, 25, 2, 7, 380),
            new(28, 32, 3, 12, 450)
        };

        return WeeklyReview.CreatePremium(
            _weekStart,
            summary,
            pastWeeks,
            mostProductiveDay: "Tuesday",
            mostProductiveTimeWindow: "9 AM - 12 PM",
            avoidedTasks: ["Clean up backlog"],
            estimationAccuracyPercent: 72,
            questProgressUpdates: ["Quest A: 80% complete"]);
    }
}
