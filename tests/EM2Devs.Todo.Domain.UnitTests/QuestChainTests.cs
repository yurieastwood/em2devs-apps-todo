using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for the QuestChain entity and QuestChainAdapter service.
/// Encodes behaviours from recurring-tasks.feature (Quest Chains rule).
/// </summary>
public sealed class QuestChainTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateChain_When_ValidDetails()
    {
        QuestChain chain = QuestChain.Create(
            new QuestTitle("Weekly Meal Prep"),
            RecurrencePattern.Weekly,
            [new TaskTitle("Plan meals"), new TaskTitle("Write shopping list"),
             new TaskTitle("Buy ingredients"), new TaskTitle("Prep ingredients")],
            DayOfWeek.Saturday);

        chain.Id.Value.ShouldNotBe(Guid.Empty);
        chain.Title.Value.ShouldBe("Weekly Meal Prep");
        chain.Cadence.ShouldBe(RecurrencePattern.Weekly);
        chain.DayOfWeek.ShouldBe(DayOfWeek.Saturday);
        chain.TaskTemplate.Count.ShouldBe(4);
        chain.History.ShouldBeEmpty();
        chain.TotalXpEarned.Value.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateChainWithoutDayOfWeek_When_Omitted()
    {
        QuestChain chain = QuestChain.Create(
            new QuestTitle("Daily Review"),
            RecurrencePattern.Daily,
            [new TaskTitle("Journal")]);

        chain.DayOfWeek.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_NullTitle()
    {
        Should.Throw<ArgumentNullException>(() =>
            QuestChain.Create(null!, RecurrencePattern.Weekly, [new TaskTitle("t")]));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_NullTemplate()
    {
        Should.Throw<ArgumentNullException>(() =>
            QuestChain.Create(new QuestTitle("t"), RecurrencePattern.Weekly, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TemplateEmpty()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            QuestChain.Create(new QuestTitle("t"), RecurrencePattern.Weekly, Array.Empty<TaskTitle>()));
        ex.Message.ShouldContain("at least one task template");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateQuestInstance_With_TemplateTasksAnd24HourDeadline()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        DateOnly saturday = new(2026, 3, 7);

        Quest quest = chain.GenerateInstance(saturday);

        quest.Title.Value.ShouldBe("Weekly Meal Prep");
        quest.Tasks.Count.ShouldBe(4);
        quest.DueDate.ShouldBe(saturday.AddDays(1));
        chain.History.Count.ShouldBe(1);
        chain.History[0].QuestId.ShouldBe(quest.Id);
        chain.History[0].ScheduledOn.ShouldBe(saturday);
        chain.History[0].Completed.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecordInstanceOutcome_When_QuestCompleted()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        Quest q = chain.GenerateInstance(new DateOnly(2026, 3, 7));

        chain.RecordInstanceOutcome(q.Id, completed: true, timeToComplete: TimeSpan.FromHours(2));

        chain.History[0].Completed.ShouldBeTrue();
        chain.History[0].TimeToComplete.ShouldBe(TimeSpan.FromHours(2));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_RecordingWithNullQuestId()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        Should.Throw<ArgumentNullException>(() => chain.RecordInstanceOutcome(null!, true, null));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RecordingUnknownInstance()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        DomainException ex = Should.Throw<DomainException>(() =>
            chain.RecordInstanceOutcome(QuestId.New(), true, null));
        ex.Message.ShouldContain("not part of this chain");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveZeroStreak_When_NoInstances()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        chain.ConsecutiveCompletionStreak.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ComputeStreak_FromTrailingCompletedInstances()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        Quest q1 = chain.GenerateInstance(new DateOnly(2026, 1, 3));
        Quest q2 = chain.GenerateInstance(new DateOnly(2026, 1, 10));
        Quest q3 = chain.GenerateInstance(new DateOnly(2026, 1, 17));
        chain.RecordInstanceOutcome(q1.Id, true, TimeSpan.FromHours(1));
        chain.RecordInstanceOutcome(q2.Id, true, TimeSpan.FromHours(1));
        chain.RecordInstanceOutcome(q3.Id, true, TimeSpan.FromHours(1));

        chain.ConsecutiveCompletionStreak.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_OnMissedInstance()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        Quest q1 = chain.GenerateInstance(new DateOnly(2026, 1, 3));
        Quest q2 = chain.GenerateInstance(new DateOnly(2026, 1, 10));
        Quest q3 = chain.GenerateInstance(new DateOnly(2026, 1, 17));
        chain.RecordInstanceOutcome(q1.Id, true, TimeSpan.FromHours(1));
        chain.RecordInstanceOutcome(q2.Id, false, null);
        chain.RecordInstanceOutcome(q3.Id, true, TimeSpan.FromHours(1));

        chain.ConsecutiveCompletionStreak.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnBaseMultiplier_When_NoStreak()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        chain.GetConsistencyBonusMultiplier().ShouldBe(1.0m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncreaseMultiplier_WithStreak()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        for (int i = 0; i < 4; i++)
        {
            Quest q = chain.GenerateInstance(new DateOnly(2026, 1, 3).AddDays(i * 7));
            chain.RecordInstanceOutcome(q.Id, true, TimeSpan.FromHours(1));
        }

        // 4-week streak → multiplier 1.4
        chain.GetConsistencyBonusMultiplier().ShouldBe(1.4m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CapMultiplier_AtTwo()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        for (int i = 0; i < 15; i++)
        {
            Quest q = chain.GenerateInstance(new DateOnly(2026, 1, 3).AddDays(i * 7));
            chain.RecordInstanceOutcome(q.Id, true, TimeSpan.FromHours(1));
        }

        chain.GetConsistencyBonusMultiplier().ShouldBe(2.0m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportStats_ForHistory()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        Quest q1 = chain.GenerateInstance(new DateOnly(2026, 1, 3));
        Quest q2 = chain.GenerateInstance(new DateOnly(2026, 1, 10));
        chain.RecordInstanceOutcome(q1.Id, true, TimeSpan.FromHours(2));
        chain.RecordInstanceOutcome(q2.Id, false, null);
        chain.AddXpEarned(new ExperiencePoints(50));

        QuestChainStats stats = chain.GetStats();

        stats.TotalInstances.ShouldBe(2);
        stats.CompletedInstances.ShouldBe(1);
        stats.CompletionRate.ShouldBe(0.5m);
        stats.AverageTimeToComplete.ShouldBe(TimeSpan.FromHours(2));
        stats.ConsecutiveCompletionStreak.ShouldBe(0);
        stats.TotalXpEarned.Value.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportZeroStats_When_NoHistory()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        QuestChainStats stats = chain.GetStats();
        stats.TotalInstances.ShouldBe(0);
        stats.CompletedInstances.ShouldBe(0);
        stats.CompletionRate.ShouldBe(0m);
        stats.AverageTimeToComplete.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddXpEarned_When_Accumulating()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        chain.AddXpEarned(new ExperiencePoints(10));
        chain.AddXpEarned(new ExperiencePoints(5));
        chain.TotalXpEarned.Value.ShouldBe(15);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AddingNullXp()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        Should.Throw<ArgumentNullException>(() => chain.AddXpEarned(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddTemplateTask_When_New()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        chain.AddTemplateTask(new TaskTitle("Clean kitchen"));

        chain.TaskTemplate.Count.ShouldBe(5);
        chain.TaskTemplate[4].Value.ShouldBe("Clean kitchen");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AddingNullTemplateTask()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        Should.Throw<ArgumentNullException>(() => chain.AddTemplateTask(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AddingDuplicateTemplateTask()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        DomainException ex = Should.Throw<DomainException>(() =>
            chain.AddTemplateTask(new TaskTitle("Plan meals")));
        ex.Message.ShouldContain("already exists");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateNewQuestChainId_PerInvocation()
    {
        QuestChainId a = QuestChainId.New();
        QuestChainId b = QuestChainId.New();
        a.ShouldNotBe(b);
        a.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExposeDefaultDeadline_AsTwentyFourHours()
    {
        QuestChain.DefaultInstanceDeadline.ShouldBe(TimeSpan.FromHours(24));
    }

    // QuestChainAdapter — pattern detection

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DetectWeeklyPattern_When_ThreeConsecutiveWeeklyCompletions()
    {
        QuestChainPattern? detected = QuestChainAdapter.DetectPattern(
        [
            new QuestCompletionRecord(new QuestTitle("Weekly meal prep"), new DateOnly(2026, 3, 1)),
            new QuestCompletionRecord(new QuestTitle("Weekly meal prep"), new DateOnly(2026, 3, 8)),
            new QuestCompletionRecord(new QuestTitle("Weekly meal prep"), new DateOnly(2026, 3, 15))
        ]);

        detected.ShouldNotBeNull();
        detected!.Title.Value.ShouldBe("Weekly meal prep");
        detected.Cadence.ShouldBe(RecurrencePattern.Weekly);
        detected.OccurrenceCount.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNull_When_InsufficientOccurrences()
    {
        QuestChainPattern? detected = QuestChainAdapter.DetectPattern(
        [
            new QuestCompletionRecord(new QuestTitle("meal prep"), new DateOnly(2026, 3, 1)),
            new QuestCompletionRecord(new QuestTitle("meal prep"), new DateOnly(2026, 3, 8))
        ]);
        detected.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNull_When_GapsAreNotWeekly()
    {
        QuestChainPattern? detected = QuestChainAdapter.DetectPattern(
        [
            new QuestCompletionRecord(new QuestTitle("Random"), new DateOnly(2026, 3, 1)),
            new QuestCompletionRecord(new QuestTitle("Random"), new DateOnly(2026, 3, 3)),
            new QuestCompletionRecord(new QuestTitle("Random"), new DateOnly(2026, 3, 20))
        ]);
        detected.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNull_When_HistoryIsEmpty()
    {
        QuestChainAdapter.DetectPattern(Array.Empty<QuestCompletionRecord>()).ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_DetectPatternCalledWithNull()
    {
        Should.Throw<ArgumentNullException>(() => QuestChainAdapter.DetectPattern(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DetectPattern_EvenWhenOtherQuestsInHistory()
    {
        QuestChainPattern? detected = QuestChainAdapter.DetectPattern(
        [
            new QuestCompletionRecord(new QuestTitle("Other"), new DateOnly(2026, 3, 1)),
            new QuestCompletionRecord(new QuestTitle("Weekly"), new DateOnly(2026, 3, 1)),
            new QuestCompletionRecord(new QuestTitle("Weekly"), new DateOnly(2026, 3, 8)),
            new QuestCompletionRecord(new QuestTitle("Weekly"), new DateOnly(2026, 3, 15))
        ]);

        detected.ShouldNotBeNull();
        detected!.Title.Value.ShouldBe("Weekly");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptSixAndEightDayGaps_AsWeekly()
    {
        QuestChainPattern? detected = QuestChainAdapter.DetectPattern(
        [
            new QuestCompletionRecord(new QuestTitle("Flexible"), new DateOnly(2026, 3, 1)),
            new QuestCompletionRecord(new QuestTitle("Flexible"), new DateOnly(2026, 3, 7)),   // +6
            new QuestCompletionRecord(new QuestTitle("Flexible"), new DateOnly(2026, 3, 15))   // +8
        ]);
        detected.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RejectFiveDayGap_NotWeekly()
    {
        QuestChainPattern? detected = QuestChainAdapter.DetectPattern(
        [
            new QuestCompletionRecord(new QuestTitle("FiveDay"), new DateOnly(2026, 3, 1)),
            new QuestCompletionRecord(new QuestTitle("FiveDay"), new DateOnly(2026, 3, 6)),   // +5
            new QuestCompletionRecord(new QuestTitle("FiveDay"), new DateOnly(2026, 3, 13))
        ]);
        detected.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RejectNineDayGap_NotWeekly()
    {
        QuestChainPattern? detected = QuestChainAdapter.DetectPattern(
        [
            new QuestCompletionRecord(new QuestTitle("NineDay"), new DateOnly(2026, 3, 1)),
            new QuestCompletionRecord(new QuestTitle("NineDay"), new DateOnly(2026, 3, 10)),   // +9
            new QuestCompletionRecord(new QuestTitle("NineDay"), new DateOnly(2026, 3, 17))
        ]);
        detected.ShouldBeNull();
    }

    // QuestChainAdapter — template suggestions

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SuggestTemplateAddition_When_TaskAppearsInAllRecentInstances()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        List<IReadOnlyList<TaskTitle>> recent =
        [
            [new TaskTitle("Clean kitchen")],
            [new TaskTitle("Clean kitchen")],
            [new TaskTitle("Clean kitchen")],
            [new TaskTitle("Clean kitchen")]
        ];

        IReadOnlyList<TaskTitle> suggestions = QuestChainAdapter.SuggestTemplateAdditions(chain, recent, minOccurrences: 4);

        suggestions.Count.ShouldBe(1);
        suggestions[0].Value.ShouldBe("Clean kitchen");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotSuggestTask_AlreadyInTemplate()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        List<IReadOnlyList<TaskTitle>> recent =
        [
            [new TaskTitle("Plan meals")],
            [new TaskTitle("Plan meals")],
            [new TaskTitle("Plan meals")],
            [new TaskTitle("Plan meals")]
        ];

        QuestChainAdapter.SuggestTemplateAdditions(chain, recent, minOccurrences: 4).ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotSuggestTask_When_BelowMinimumOccurrences()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        List<IReadOnlyList<TaskTitle>> recent =
        [
            [new TaskTitle("Clean kitchen")],
            [new TaskTitle("Clean kitchen")],
            [new TaskTitle("Clean kitchen")],
            [new TaskTitle("Something else")]
        ];

        QuestChainAdapter.SuggestTemplateAdditions(chain, recent, minOccurrences: 4).ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnEmpty_When_FewerInstancesThanMin()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        List<IReadOnlyList<TaskTitle>> recent =
        [
            [new TaskTitle("Clean kitchen")]
        ];

        QuestChainAdapter.SuggestTemplateAdditions(chain, recent, minOccurrences: 4).ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CountTaskOnlyOncePerInstance()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        List<IReadOnlyList<TaskTitle>> recent =
        [
            [new TaskTitle("Clean kitchen"), new TaskTitle("Clean kitchen"), new TaskTitle("Clean kitchen")],
            [new TaskTitle("Clean kitchen")],
        ];

        // Even with duplicates in the single instance, it counts as 1 occurrence; total = 2
        IReadOnlyList<TaskTitle> suggestions = QuestChainAdapter.SuggestTemplateAdditions(chain, recent, minOccurrences: 2);
        suggestions.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MinOccurrencesZero()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        DomainException ex = Should.Throw<DomainException>(() =>
            QuestChainAdapter.SuggestTemplateAdditions(chain, [], minOccurrences: 0));
        ex.Message.ShouldContain("positive");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MinOccurrencesNegative()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        DomainException ex = Should.Throw<DomainException>(() =>
            QuestChainAdapter.SuggestTemplateAdditions(chain, [], minOccurrences: -1));
        ex.Message.ShouldContain("positive");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_SuggestCalledWithNullChain()
    {
        Should.Throw<ArgumentNullException>(() =>
            QuestChainAdapter.SuggestTemplateAdditions(null!, [], minOccurrences: 1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_SuggestCalledWithNullInstances()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        Should.Throw<ArgumentNullException>(() =>
            QuestChainAdapter.SuggestTemplateAdditions(chain, null!, minOccurrences: 1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_WithParamName_TaskTemplate_When_NullTemplate()
    {
        ArgumentNullException ex = Should.Throw<ArgumentNullException>(() =>
            QuestChain.Create(new QuestTitle("t"), RecurrencePattern.Weekly, null!));
        ex.ParamName.ShouldBe("taskTemplate");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_WithParamName_Xp_When_AddingNullXp()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        ArgumentNullException ex = Should.Throw<ArgumentNullException>(() => chain.AddXpEarned(null!));
        ex.ParamName.ShouldBe("xp");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeChainTitle_InGeneratedQuestDescription()
    {
        QuestChain chain = CreateWeeklyMealPrep();
        Quest q = chain.GenerateInstance(new DateOnly(2026, 3, 7));
        q.Description.ShouldBe("Instance of chain Weekly Meal Prep");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CapMultiplier_AtTwo_When_StreakIsExactlyTen()
    {
        // Boundary: streak of 10 yields exactly 2.0 (not capped below, not > 2.0)
        QuestChain chain = CreateWeeklyMealPrep();
        for (int i = 0; i < 10; i++)
        {
            Quest q = chain.GenerateInstance(new DateOnly(2026, 1, 3).AddDays(i * 7));
            chain.RecordInstanceOutcome(q.Id, true, TimeSpan.FromHours(1));
        }

        chain.ConsecutiveCompletionStreak.ShouldBe(10);
        chain.GetConsistencyBonusMultiplier().ShouldBe(2.0m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IgnoreNotCompletedEntries_InAverageTimeToComplete_EvenIfTimeRecorded()
    {
        // Kills mutation: completed && HasValue → completed || HasValue.
        // Under OR, a not-completed entry with TimeToComplete set would pollute the average.
        QuestChain chain = CreateWeeklyMealPrep();
        Quest q1 = chain.GenerateInstance(new DateOnly(2026, 1, 3));
        Quest q2 = chain.GenerateInstance(new DateOnly(2026, 1, 10));
        chain.RecordInstanceOutcome(q1.Id, completed: true, timeToComplete: TimeSpan.FromHours(10));
        // Record "not completed" but with a huge time value (e.g. abandoned mid-way)
        chain.RecordInstanceOutcome(q2.Id, completed: false, timeToComplete: TimeSpan.FromHours(1000));

        QuestChainStats stats = chain.GetStats();
        stats.AverageTimeToComplete.ShouldBe(TimeSpan.FromHours(10));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ComputeTrueAverage_NotMinOrMax_InStats()
    {
        // Kills mutation: Average→Min/Max. With times 2h and 6h, avg=4h, min=2h, max=6h.
        QuestChain chain = CreateWeeklyMealPrep();
        Quest q1 = chain.GenerateInstance(new DateOnly(2026, 1, 3));
        Quest q2 = chain.GenerateInstance(new DateOnly(2026, 1, 10));
        chain.RecordInstanceOutcome(q1.Id, true, TimeSpan.FromHours(2));
        chain.RecordInstanceOutcome(q2.Id, true, TimeSpan.FromHours(6));

        QuestChainStats stats = chain.GetStats();
        stats.AverageTimeToComplete.ShouldBe(TimeSpan.FromHours(4));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_WithParamName_Source_When_DetectPatternHistoryIsNull()
    {
        // history is passed to Linq GroupBy; either the explicit ThrowIfNull fires first (paramName=history)
        // or, if removed, GroupBy throws with paramName=source. Asserting "history" kills the mutation.
        ArgumentNullException ex = Should.Throw<ArgumentNullException>(() => QuestChainAdapter.DetectPattern(null!));
        ex.ParamName.ShouldBe("history");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CountEachTaskOnlyOncePerInstance_NotIncludingDuplicates()
    {
        // Kills mutation: removing the `continue;` in the dedup guard causes duplicates within an
        // instance to each bump the count. With 4 copies of "A" in one instance and "B" in another,
        // with min=2: original records A=1,B=1 → no suggestions; mutated records A=4 → suggests A.
        QuestChain chain = CreateWeeklyMealPrep();
        List<IReadOnlyList<TaskTitle>> recent =
        [
            [new TaskTitle("A"), new TaskTitle("A"), new TaskTitle("A"), new TaskTitle("A")],
            [new TaskTitle("B")]
        ];

        IReadOnlyList<TaskTitle> suggestions = QuestChainAdapter.SuggestTemplateAdditions(chain, recent, minOccurrences: 2);
        suggestions.ShouldBeEmpty();
    }

    private static QuestChain CreateWeeklyMealPrep()
    {
        return QuestChain.Create(
            new QuestTitle("Weekly Meal Prep"),
            RecurrencePattern.Weekly,
            [new TaskTitle("Plan meals"), new TaskTitle("Write shopping list"),
             new TaskTitle("Buy ingredients"), new TaskTitle("Prep ingredients")],
            DayOfWeek.Saturday);
    }
}
