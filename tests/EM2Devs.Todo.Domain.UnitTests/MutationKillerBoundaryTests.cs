using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Boundary tests that assert exact exception messages, parameter names,
/// and ordering/filter specifics in order to eliminate surviving mutants
/// that string/statement/logical replacements would otherwise leave alive.
/// </summary>
public sealed class MutationKillerBoundaryTests
{
    private static readonly DateOnly _today = new(2026, 4, 15); // Wednesday

    // ---------- NaturalDateParser exception messages ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void NaturalDateParser_EmptyInput_MessageMentionsEmpty()
    {
        var ex = Should.Throw<DomainException>(() => NaturalDateParser.Parse("", _today));
        ex.Message.ShouldBe("Date expression cannot be empty.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void NaturalDateParser_UnrecognisedNextWeekday_MessageQuotesExpression()
    {
        var ex = Should.Throw<DomainException>(() => NaturalDateParser.Parse("next banana", _today));
        ex.Message.ShouldBe("Unrecognised weekday after 'next': 'banana'.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void NaturalDateParser_UnrecognisedInNDays_MessageQuotesExpression()
    {
        var ex = Should.Throw<DomainException>(() => NaturalDateParser.Parse("in three days", _today));
        ex.Message.ShouldBe("Unrecognised 'in N days' expression: 'in three days'.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void NaturalDateParser_UnrecognisedBare_MessageQuotesExpression()
    {
        var ex = Should.Throw<DomainException>(() => NaturalDateParser.Parse("banana", _today));
        ex.Message.ShouldBe("Unrecognised date expression: 'banana'.");
    }

    // ---------- QuickAddParser exception messages ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void QuickAddParser_EmptyInput_MessageMentionsEmpty()
    {
        var ex = Should.Throw<DomainException>(() => QuickAddParser.Parse("", _today));
        ex.Message.ShouldBe("Quick-add input cannot be empty.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void QuickAddParser_NoTitle_MessageMentionsTitle()
    {
        var ex = Should.Throw<DomainException>(() => QuickAddParser.Parse("#tag !high", _today));
        ex.Message.ShouldBe("Quick-add input must contain a title.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void QuickAddParser_UnknownPriority_MessageQuotesValue()
    {
        var ex = Should.Throw<DomainException>(() => QuickAddParser.Parse("Do thing !urgent", _today));
        ex.Message.ShouldBe("Unrecognised priority: 'urgent'.");
    }

    // ---------- QuickAddParser: IsDirective length boundary ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void QuickAddParser_BareHashDuringDateCollection_IsPartOfDateExpression()
    {
        // During date collection, a bare "#" (length 1) must NOT be treated as a
        // directive separator. If the IsDirective length-guard mutates from > 1
        // to >= 1, a bare "#" would terminate date collection early and leave
        // the "#" and trailing tokens to be appended as title content; the parse
        // would succeed with DueDate=tomorrow. Under the original > 1 guard,
        // "#" stays in the date tokens, making the expression "tomorrow # extra"
        // which NaturalDateParser rejects.
        Should.Throw<DomainException>(() =>
            QuickAddParser.Parse("Work ^tomorrow # extra", _today));
    }

    // ---------- QuickAddParser: month/day boundary (< vs <=) ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void QuickAddParser_MonthDayEqualToToday_StaysInCurrentYear()
    {
        // today is 2026-04-15; "^April 15" must stay in 2026, not advance to 2027.
        // This kills the mutation `<` -> `<=` on the rollover check.
        var result = QuickAddParser.Parse("Thing ^April 15", _today);
        result.DueDate.ShouldBe(new DateOnly(2026, 4, 15));
    }

    // ---------- Tag boundary ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void Tag_Empty_MessageIsExact()
    {
        var ex = Should.Throw<DomainException>(() => Tag.From(""));
        ex.Message.ShouldBe("Tag cannot be empty.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Tag_TooLong_MessageIsExact()
    {
        string tooLong = new('x', Tag.MaxLength + 1);
        var ex = Should.Throw<DomainException>(() => Tag.From(tooLong));
        ex.Message.ShouldBe($"Tag cannot exceed {Tag.MaxLength} characters.");
    }

    // ---------- SkillTreePerk exception message ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void SkillTreePerk_EmptyDescription_MessageIsExact()
    {
        var ex = Should.Throw<DomainException>(() =>
            new SkillTreePerk(SkillTreeType.Creator, new SkillTier(1), SkillTreePerkType.Tips, ""));
        ex.Message.ShouldBe("Perk description cannot be empty.");
    }

    // ---------- SkillTreeCatalog unlock hints: full content ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void SkillTreeCatalog_UnlockHint_IsExactFormatForEveryTree()
    {
        var catalog = SkillTreeCatalog.Build(Array.Empty<SkillTree>());

        var expectedCategories = new Dictionary<SkillTreeType, string>
        {
            [SkillTreeType.Creator] = "creative",
            [SkillTreeType.Guardian] = "health or fitness",
            [SkillTreeType.Scholar] = "learning or study",
            [SkillTreeType.Architect] = "work or career",
            [SkillTreeType.Connector] = "social",
            [SkillTreeType.Steward] = "home or organising",
            [SkillTreeType.Builder] = "side-project",
        };

        foreach (var entry in catalog.Entries)
        {
            int threshold = SkillTreeDiscovery.DiscoveryThreshold(entry.Type);
            string expected = $"Complete {threshold} {expectedCategories[entry.Type]} tasks to unlock";
            entry.UnlockHint.ShouldBe(expected);
        }
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void SkillTreeCatalog_LockedHint_MessageIsExact()
    {
        var ex = Should.Throw<DomainException>(() =>
            SkillTreeCatalogEntry.Locked(SkillTreeType.Creator, ""));
        ex.Message.ShouldBe("Unlock hint cannot be empty.");
    }

    // ---------- SkillTreePerkCatalog: descriptions exact and all trees ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void SkillTreePerkCatalog_EveryTreeAndTier_DescriptionIsExact()
    {
        foreach (SkillTreeType tree in Enum.GetValues<SkillTreeType>())
        {
            var perks = SkillTreePerkCatalog.AllPerksFor(tree);
            perks[0].Description.ShouldBe($"Personalised {tree} tips");
            perks[1].Description.ShouldBe($"Suggested {tree} quest templates");
            perks[2].Description.ShouldBe($"{tree} profile badge and themed colour palette");
        }
    }

    // ---------- TaskViewFilter: argument null param names ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void TaskViewFilter_ForInbox_NullParamNameIsTasks()
    {
        var ex = Should.Throw<ArgumentNullException>(() => TaskViewFilter.ForInbox(null!));
        ex.ParamName.ShouldBe("tasks");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void TaskViewFilter_ForToday_NullParamNameIsTasks()
    {
        var ex = Should.Throw<ArgumentNullException>(() => TaskViewFilter.ForToday(null!, _today));
        ex.ParamName.ShouldBe("tasks");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void TaskViewFilter_ForUpcoming_NullParamNameIsTasks()
    {
        var ex = Should.Throw<ArgumentNullException>(() => TaskViewFilter.ForUpcoming(null!, _today));
        ex.ParamName.ShouldBe("tasks");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void TaskViewFilter_ForCompleted_NullParamNameIsTasks()
    {
        var ex = Should.Throw<ArgumentNullException>(() => TaskViewFilter.ForCompleted(null!));
        ex.ParamName.ShouldBe("tasks");
    }

    // ---------- TaskViewFilter: ForUpcoming ordering + filter ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void TaskViewFilter_ForUpcoming_OrdersTasksAscendingByCreatedAt()
    {
        DateOnly target = _today.AddDays(3);
        var older = TodoTask.CreateFromRecurring(TestData.TestUserId, new TaskTitle("older"), RecurringTaskId.New(), target);
        // Introduce a clock gap so CreatedAt differs.
        System.Threading.Thread.Sleep(5);
        var newer = TodoTask.CreateFromRecurring(TestData.TestUserId, new TaskTitle("newer"), RecurringTaskId.New(), target);

        var groups = TaskViewFilter.ForUpcoming(new[] { newer, older }, _today);
        var dayTasks = groups.Single(g => g.Date == target).Tasks;

        // Ascending by CreatedAt -> older first, newer second.
        dayTasks[0].Title.Value.ShouldBe("older");
        dayTasks[1].Title.Value.ShouldBe("newer");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void TaskViewFilter_ForUpcoming_ExcludesDeletedTasksOnMatchingDay()
    {
        DateOnly target = _today.AddDays(3);
        var deleted = TodoTask.CreateFromRecurring(TestData.TestUserId, new TaskTitle("deleted"), RecurringTaskId.New(), target);
        deleted.Delete();
        var alive = TodoTask.CreateFromRecurring(TestData.TestUserId, new TaskTitle("alive"), RecurringTaskId.New(), target);

        var groups = TaskViewFilter.ForUpcoming(new[] { deleted, alive }, _today);
        var dayTasks = groups.Single(g => g.Date == target).Tasks;

        dayTasks.Count.ShouldBe(1);
        dayTasks[0].Title.Value.ShouldBe("alive");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void TaskViewFilter_ForUpcoming_ExcludesDoneTasksOnMatchingDay()
    {
        DateOnly target = _today.AddDays(3);
        var done = TodoTask.CreateFromRecurring(TestData.TestUserId, new TaskTitle("done"), RecurringTaskId.New(), target);
        done.MoveToInProgress();
        done.MarkAsDone();
        var alive = TodoTask.CreateFromRecurring(TestData.TestUserId, new TaskTitle("alive"), RecurringTaskId.New(), target);

        var groups = TaskViewFilter.ForUpcoming(new[] { done, alive }, _today);
        var dayTasks = groups.Single(g => g.Date == target).Tasks;

        dayTasks.Count.ShouldBe(1);
        dayTasks[0].Title.Value.ShouldBe("alive");
    }

    // ---------- TaskViewFilter: ForToday excludes Deleted ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void TaskViewFilter_ForToday_ExcludesDeletedTask()
    {
        var scheduled = TodoTask.CreateFromRecurring(TestData.TestUserId, new TaskTitle("x"), RecurringTaskId.New(), _today);
        scheduled.Delete();

        var result = TaskViewFilter.ForToday(new[] { scheduled }, _today);
        result.ShouldBeEmpty();
    }

    // ---------- TaskViewFilter: ForInbox excludes Deleted and Done separately ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void TaskViewFilter_ForInbox_ExcludesDoneTask()
    {
        var done = TodoTask.Create(TestData.TestUserId, new TaskTitle("done"));
        done.MoveToInProgress();
        done.MarkAsDone();
        var open = TodoTask.Create(TestData.TestUserId, new TaskTitle("open"));

        var inbox = TaskViewFilter.ForInbox(new[] { done, open });
        inbox.Count.ShouldBe(1);
        inbox[0].Title.Value.ShouldBe("open");
    }

    // ---------- TaskViewFilter: ForCompleted descending grouping ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void TaskViewFilter_ForCompleted_OrdersGroupsMostRecentDateFirst()
    {
        // Fabricate two completed tasks with CompletedAt on DIFFERENT UTC dates
        // by writing the backing property via reflection. Only the private
        // setter of CompletedAt is manipulated; this keeps production code
        // unchanged while guaranteeing two distinct group keys.
        var earlier = TodoTask.CreateFromRecurring(TestData.TestUserId, new TaskTitle("earlier"), RecurringTaskId.New(), _today);
        earlier.MoveToInProgress();
        earlier.MarkAsDone();
        SetCompletedAt(earlier, new DateTimeOffset(2026, 4, 10, 12, 0, 0, TimeSpan.Zero));

        var later = TodoTask.CreateFromRecurring(TestData.TestUserId, new TaskTitle("later"), RecurringTaskId.New(), _today);
        later.MoveToInProgress();
        later.MarkAsDone();
        SetCompletedAt(later, new DateTimeOffset(2026, 4, 14, 12, 0, 0, TimeSpan.Zero));

        var groups = TaskViewFilter.ForCompleted(new[] { earlier, later });

        groups.Count.ShouldBe(2);
        groups[0].Date.ShouldBe(new DateOnly(2026, 4, 14));
        groups[1].Date.ShouldBe(new DateOnly(2026, 4, 10));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void TaskViewFilter_ForCompleted_ExcludesTasksWithStatusNotDoneEvenIfCompletedAtSet()
    {
        // Kill the `==/&&` -> `||` mutation on `t.Status == Done && t.CompletedAt.HasValue`.
        // Construct a task that has CompletedAt populated but Status reverted via Reopen:
        // Reopen sets CompletedAt = null, so we instead set CompletedAt via reflection while
        // leaving Status as Todo.
        var weird = TodoTask.Create(TestData.TestUserId, new TaskTitle("weird"));
        SetCompletedAt(weird, new DateTimeOffset(2026, 4, 14, 12, 0, 0, TimeSpan.Zero));

        // Also include a genuinely completed task so the returned list is non-empty and
        // the filter's behaviour is unambiguous.
        var done = TodoTask.CreateFromRecurring(TestData.TestUserId, new TaskTitle("done"), RecurringTaskId.New(), _today);
        done.MoveToInProgress();
        done.MarkAsDone();

        var groups = TaskViewFilter.ForCompleted(new[] { weird, done });
        groups.SelectMany(g => g.Tasks).ShouldNotContain(t => t.Title.Value == "weird");
        groups.SelectMany(g => g.Tasks).ShouldContain(t => t.Title.Value == "done");
    }

    private static void SetCompletedAt(TodoTask task, DateTimeOffset value)
    {
        var prop = typeof(TodoTask).GetProperty(
            nameof(TodoTask.CompletedAt),
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        prop!.SetValue(task, value);
    }

    // ---------- TodoTask.RecordActualTime: exact exception messages ----------

    [Fact]
    [Trait("Category", "Domain")]
    public void TodoTask_RecordActualTime_NotDone_MessageIsExact()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("x"));
        task.UpdateEstimatedTime(TimeEstimate.FromMinutes(60));

        var ex = Should.Throw<DomainException>(() =>
            task.RecordActualTime(TimeEstimate.FromMinutes(60)));
        ex.Message.ShouldBe("Actual time can only be recorded for completed tasks.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void TodoTask_RecordActualTime_NoEstimate_MessageIsExact()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("x"));
        task.MoveToInProgress();
        task.MarkAsDone();

        var ex = Should.Throw<DomainException>(() =>
            task.RecordActualTime(TimeEstimate.FromMinutes(60)));
        ex.Message.ShouldBe("Actual time can only be recorded when an estimate exists.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void TodoTask_RecordActualTime_NullActual_ParamNameIsActual()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("x"));
        task.UpdateEstimatedTime(TimeEstimate.FromMinutes(60));
        task.MoveToInProgress();
        task.MarkAsDone();

        var ex = Should.Throw<ArgumentNullException>(() => task.RecordActualTime(null!));
        ex.ParamName.ShouldBe("actual");
    }
}
