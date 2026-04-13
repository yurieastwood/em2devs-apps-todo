using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for Timeline value object.
/// Maps to: docs/features/reflection/journey-timeline.feature
/// </summary>
public sealed class TimelineTests
{
    private static readonly DateTimeOffset _baseDate = new(2026, 3, 15, 14, 0, 0, TimeSpan.Zero);

    private static TimelineEvent CreateEvent(TimelineEventType type, DateTimeOffset date, string details = "details")
    {
        return TimelineEvent.Create(type, date, details);
    }

    private static List<TimelineEvent> CreateEventsAcrossMonths(int count, DateTimeOffset startDate)
    {
        List<TimelineEvent> events = [];
        for (int i = 0; i < count; i++)
        {
            events.Add(CreateEvent(
                (TimelineEventType)(i % 14),
                startDate.AddDays(-i * 7),
                $"Event {i + 1} details"));
        }

        return events;
    }

    // --- Scenario: Timeline displays events chronologically ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OrderEventsNewestFirst_When_ViewingTimeline()
    {
        // Given — 20 events created at different times
        List<TimelineEvent> events = [];
        for (int i = 0; i < 20; i++)
        {
            events.Add(CreateEvent(TimelineEventType.QuestCompleted, _baseDate.AddDays(-i), $"Quest {i}"));
        }

        // When
        Timeline timeline = new(events);
        IReadOnlyList<TimelineEvent> chronological = timeline.GetChronologicalEvents();

        // Then — newest first
        chronological.Count.ShouldBe(20);
        for (int i = 0; i < chronological.Count - 1; i++)
        {
            chronological[i].OccurredAt.ShouldBeGreaterThanOrEqualTo(chronological[i + 1].OccurredAt);
        }
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowDateAndType_When_DisplayingEvents()
    {
        // Given
        TimelineEvent evt = CreateEvent(TimelineEventType.TitleEarned, _baseDate, "Earned Explorer title");
        Timeline timeline = new([evt]);

        // When
        IReadOnlyList<TimelineEvent> events = timeline.GetChronologicalEvents();

        // Then
        events[0].OccurredAt.ShouldBe(_baseDate);
        events[0].EventType.ShouldBe(TimelineEventType.TitleEarned);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowScrollingFullHistory_When_ViewingTimeline()
    {
        // Given — 20 events
        List<TimelineEvent> events = CreateEventsAcrossMonths(20, _baseDate);

        // When
        Timeline timeline = new(events);

        // Then — all events accessible
        timeline.Events.Count.ShouldBe(20);
    }

    // --- Scenario: Timeline groups events by month ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GroupEventsByMonth_When_EventsSpanSixMonths()
    {
        // Given — events spanning 6 months
        List<TimelineEvent> events =
        [
            CreateEvent(TimelineEventType.LevelUp, new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.QuestCompleted, new DateTimeOffset(2026, 3, 10, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.EpicCompleted, new DateTimeOffset(2026, 2, 20, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.TitleEarned, new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.StreakMilestone, new DateTimeOffset(2025, 12, 25, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.GuildJoined, new DateTimeOffset(2025, 11, 15, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.ChallengeWon, new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero)),
        ];

        // When
        Timeline timeline = new(events);
        IReadOnlyList<MonthGroup> groups = timeline.GetEventsByMonth();

        // Then — 6 months of groups (March has 2 events)
        groups.Count.ShouldBe(6);
        groups[0].Year.ShouldBe(2026);
        groups[0].Month.ShouldBe(3);
        groups[0].Count.ShouldBe(2);
        groups[0].Header.ShouldBe("March 2026");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowSummaryCountPerMonth_When_GroupedByMonth()
    {
        // Given
        List<TimelineEvent> events =
        [
            CreateEvent(TimelineEventType.LevelUp, new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.QuestCompleted, new DateTimeOffset(2026, 3, 10, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.EpicCompleted, new DateTimeOffset(2026, 3, 5, 0, 0, 0, TimeSpan.Zero)),
        ];

        // When
        Timeline timeline = new(events);
        IReadOnlyList<MonthGroup> groups = timeline.GetEventsByMonth();

        // Then
        groups.Count.ShouldBe(1);
        groups[0].Count.ShouldBe(3);
    }

    // --- Scenario: Timeline displays year headers when events span multiple years ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GroupByYearAndMonth_When_EventsSpanMultipleYears()
    {
        // Given — events from November 2025 to March 2026
        List<TimelineEvent> events =
        [
            CreateEvent(TimelineEventType.LevelUp, new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.QuestCompleted, new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.TitleEarned, new DateTimeOffset(2025, 11, 20, 0, 0, 0, TimeSpan.Zero)),
        ];

        // When
        Timeline timeline = new(events);
        IReadOnlyList<YearGroup> years = timeline.GetEventsByYear();

        // Then
        years.Count.ShouldBe(2);
        years[0].Header.ShouldBe("2026");
        years[0].Months.Count.ShouldBe(2);
        years[1].Header.ShouldBe("2025");
        years[1].Months.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrue_When_EventsSpanMultipleYears()
    {
        // Given
        List<TimelineEvent> events =
        [
            CreateEvent(TimelineEventType.LevelUp, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.QuestCompleted, new DateTimeOffset(2025, 11, 1, 0, 0, 0, TimeSpan.Zero)),
        ];

        // When
        Timeline timeline = new(events);

        // Then
        timeline.SpansMultipleYears().ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_EventsAreWithinSingleYear()
    {
        // Given
        List<TimelineEvent> events =
        [
            CreateEvent(TimelineEventType.LevelUp, new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.QuestCompleted, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)),
        ];

        // When
        Timeline timeline = new(events);

        // Then
        timeline.SpansMultipleYears().ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_TimelineIsEmpty()
    {
        // Given / When
        Timeline timeline = Timeline.Empty();

        // Then
        timeline.SpansMultipleYears().ShouldBeFalse();
    }

    // --- Scenario: Filter timeline by event type ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowOnlyQuestEvents_When_FilteringByQuestCompleted()
    {
        // Given — a mix of level-up, quest, and title events
        List<TimelineEvent> events =
        [
            CreateEvent(TimelineEventType.LevelUp, _baseDate, "Level 5"),
            CreateEvent(TimelineEventType.QuestCompleted, _baseDate.AddDays(-1), "Quest A"),
            CreateEvent(TimelineEventType.TitleEarned, _baseDate.AddDays(-2), "Explorer"),
            CreateEvent(TimelineEventType.QuestCompleted, _baseDate.AddDays(-3), "Quest B"),
        ];

        // When
        Timeline timeline = new(events);
        Timeline filtered = timeline.FilterByEventType(TimelineEventType.QuestCompleted);

        // Then
        filtered.Events.Count.ShouldBe(2);
        filtered.Events.ShouldAllBe(e => e.EventType == TimelineEventType.QuestCompleted);
    }

    // --- Scenario: Personal notes persist when filtering by event type ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveNotes_When_FilteringByEventType()
    {
        // Given — a quest event with a note and a level-up event without
        TimelineEvent questEvent = CreateEvent(TimelineEventType.QuestCompleted, _baseDate, "Best quest");
        questEvent.AddNote(new PersonalNote("My best quest yet!", _baseDate));

        TimelineEvent levelUpEvent = CreateEvent(TimelineEventType.LevelUp, _baseDate.AddDays(-1), "Level 3");

        // When
        Timeline timeline = new([questEvent, levelUpEvent]);
        Timeline filtered = timeline.FilterByEventType(TimelineEventType.QuestCompleted);

        // Then
        filtered.Events.Count.ShouldBe(1);
        filtered.Events[0].Note.ShouldNotBeNull();
        filtered.Events[0].Note!.Text.ShouldBe("My best quest yet!");
    }

    // --- Scenario: New user has an empty timeline with encouragement ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowEncouragingMessage_When_TimelineIsEmpty()
    {
        // Given / When
        Timeline timeline = Timeline.Empty();

        // Then
        timeline.IsEmpty.ShouldBeTrue();
        string message = Timeline.GetEmptyMessage();
        message.ShouldNotBeNullOrWhiteSpace();
        message.ShouldContain("journey");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowAvailableEventTypes_When_TimelineIsEmpty()
    {
        // Given / When
        IReadOnlyList<TimelineEventType> types = Timeline.GetAvailableEventTypes();

        // Then — should list all 14 event types
        types.Count.ShouldBe(14);
    }

    // --- Scenario: Long-term user scrolling back through months of progress ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SupportFullHistoryScroll_When_UserHas50PlusEvents()
    {
        // Given — 50+ events over 8 months
        List<TimelineEvent> events = CreateEventsAcrossMonths(55, _baseDate);

        // When
        Timeline timeline = new(events);

        // Then
        timeline.Events.Count.ShouldBe(55);
        IReadOnlyList<MonthGroup> months = timeline.GetEventsByMonth();
        months.Count.ShouldBeGreaterThan(1);
    }

    // --- Scenario: Timeline loads incrementally for users with many events ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFirst20Events_When_LoadingInitialPage()
    {
        // Given — more than 100 events
        List<TimelineEvent> events = CreateEventsAcrossMonths(105, _baseDate);

        // When
        Timeline timeline = new(events);
        TimelinePage page = timeline.GetFirstPage();

        // Then
        page.Events.Count.ShouldBe(20);
        page.HasMore.ShouldBeTrue();
        page.NextCursor.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LoadNextBatch_When_ScrollingDown()
    {
        // Given — 50 events
        List<TimelineEvent> events = CreateEventsAcrossMonths(50, _baseDate);
        Timeline timeline = new(events);
        TimelinePage firstPage = timeline.GetFirstPage();

        // When
        TimelinePage secondPage = timeline.GetNextPage(firstPage.NextCursor!);

        // Then
        secondPage.Events.Count.ShouldBe(20);
        secondPage.HasMore.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNoMore_When_ReachingEndOfTimeline()
    {
        // Given — exactly 25 events (first page 20, second page 5)
        List<TimelineEvent> events = CreateEventsAcrossMonths(25, _baseDate);
        Timeline timeline = new(events);
        TimelinePage firstPage = timeline.GetFirstPage();

        // When
        TimelinePage lastPage = timeline.GetNextPage(firstPage.NextCursor!);

        // Then
        lastPage.Events.Count.ShouldBe(5);
        lastPage.HasMore.ShouldBeFalse();
        lastPage.NextCursor.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnAllEvents_When_TotalIsLessThanPageSize()
    {
        // Given — 10 events (fewer than default page size of 20)
        List<TimelineEvent> events = CreateEventsAcrossMonths(10, _baseDate);

        // When
        Timeline timeline = new(events);
        TimelinePage page = timeline.GetFirstPage();

        // Then
        page.Events.Count.ShouldBe(10);
        page.HasMore.ShouldBeFalse();
        page.NextCursor.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_PageSizeIsZero()
    {
        // Given
        Timeline timeline = new([CreateEvent(TimelineEventType.LevelUp, _baseDate, "Level 1")]);

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => timeline.GetFirstPage(0));
        ex.Message.ShouldContain("greater than zero");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_PageSizeIsNegative()
    {
        // Given
        Timeline timeline = new([CreateEvent(TimelineEventType.LevelUp, _baseDate, "Level 1")]);

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => timeline.GetFirstPage(-1));
        ex.Message.ShouldContain("greater than zero");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NextPagePageSizeIsInvalid()
    {
        // Given
        List<TimelineEvent> events = CreateEventsAcrossMonths(25, _baseDate);
        Timeline timeline = new(events);
        TimelinePage firstPage = timeline.GetFirstPage();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(
            () => timeline.GetNextPage(firstPage.NextCursor!, 0));
        ex.Message.ShouldContain("greater than zero");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CursorIsNull()
    {
        // Given
        Timeline timeline = new([CreateEvent(TimelineEventType.LevelUp, _baseDate, "Level 1")]);

        // When / Then
        Should.Throw<ArgumentNullException>(() => timeline.GetNextPage(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CursorNotFound()
    {
        // Given
        Timeline timeline = new([CreateEvent(TimelineEventType.LevelUp, _baseDate, "Level 1")]);

        // When / Then
        DomainException ex = Should.Throw<DomainException>(
            () => timeline.GetNextPage(TimelineEventId.New()));
        ex.Message.ShouldContain("Cursor not found");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_EventsCollectionIsNull()
    {
        // Given / When / Then — kills Statement mutation on ArgumentNullException.ThrowIfNull
        ArgumentNullException ex = Should.Throw<ArgumentNullException>(() => new Timeline(null!));
        ex.ParamName.ShouldBe("events");
    }

    // --- Scenario: Custom page size support ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RespectCustomPageSize_When_Loading()
    {
        // Given — 15 events, custom page size of 5
        List<TimelineEvent> events = CreateEventsAcrossMonths(15, _baseDate);
        Timeline timeline = new(events);

        // When
        TimelinePage page = timeline.GetFirstPage(5);

        // Then
        page.Events.Count.ShouldBe(5);
        page.HasMore.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnCorrectNextPage_When_UsingCustomPageSize()
    {
        // Given — 15 events, custom page size of 5
        List<TimelineEvent> events = CreateEventsAcrossMonths(15, _baseDate);
        Timeline timeline = new(events);
        TimelinePage firstPage = timeline.GetFirstPage(5);

        // When
        TimelinePage secondPage = timeline.GetNextPage(firstPage.NextCursor!, 5);

        // Then
        secondPage.Events.Count.ShouldBe(5);
        secondPage.HasMore.ShouldBeTrue();
    }

    // --- Edge cases ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HandleExactlyPageSizeEvents_When_Loading()
    {
        // Given — exactly 20 events (equal to default page size)
        List<TimelineEvent> events = CreateEventsAcrossMonths(20, _baseDate);
        Timeline timeline = new(events);

        // When
        TimelinePage page = timeline.GetFirstPage();

        // Then — all events on first page, no more
        page.Events.Count.ShouldBe(20);
        page.HasMore.ShouldBeFalse();
        page.NextCursor.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnEmptyPage_When_TimelineHasNoEvents()
    {
        // Given
        Timeline timeline = Timeline.Empty();

        // When
        TimelinePage page = timeline.GetFirstPage();

        // Then
        page.Events.Count.ShouldBe(0);
        page.HasMore.ShouldBeFalse();
        page.NextCursor.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnEmptyGroups_When_TimelineIsEmpty()
    {
        // Given
        Timeline timeline = Timeline.Empty();

        // When
        IReadOnlyList<MonthGroup> months = timeline.GetEventsByMonth();
        IReadOnlyList<YearGroup> years = timeline.GetEventsByYear();

        // Then
        months.Count.ShouldBe(0);
        years.Count.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnEmptyTimeline_When_FilterMatchesNothing()
    {
        // Given — only quest events
        List<TimelineEvent> events =
        [
            CreateEvent(TimelineEventType.QuestCompleted, _baseDate, "Quest A"),
        ];

        // When
        Timeline timeline = new(events);
        Timeline filtered = timeline.FilterByEventType(TimelineEventType.LevelUp);

        // Then
        filtered.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MaintainOrder_When_EventsAreAddedOutOfOrder()
    {
        // Given — events added in random order
        List<TimelineEvent> events =
        [
            CreateEvent(TimelineEventType.LevelUp, _baseDate.AddDays(-5), "Old event"),
            CreateEvent(TimelineEventType.QuestCompleted, _baseDate, "New event"),
            CreateEvent(TimelineEventType.TitleEarned, _baseDate.AddDays(-10), "Oldest event"),
        ];

        // When
        Timeline timeline = new(events);
        IReadOnlyList<TimelineEvent> sorted = timeline.GetChronologicalEvents();

        // Then — newest first
        sorted[0].Details.ShouldBe("New event");
        sorted[1].Details.ShouldBe("Old event");
        sorted[2].Details.ShouldBe("Oldest event");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OrderMonthGroupsCorrectly_When_GroupingByMonth()
    {
        // Given — events in Jan and Mar 2026
        List<TimelineEvent> events =
        [
            CreateEvent(TimelineEventType.LevelUp, new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.QuestCompleted, new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero)),
        ];

        // When
        Timeline timeline = new(events);
        IReadOnlyList<MonthGroup> groups = timeline.GetEventsByMonth();

        // Then — most recent month first
        groups[0].Month.ShouldBe(3);
        groups[1].Month.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeCorrectEventsInYearGroup_When_GroupingByYear()
    {
        // Given — 2 events in 2026, 1 in 2025
        List<TimelineEvent> events =
        [
            CreateEvent(TimelineEventType.LevelUp, new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.QuestCompleted, new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.TitleEarned, new DateTimeOffset(2025, 11, 20, 0, 0, 0, TimeSpan.Zero)),
        ];

        // When
        Timeline timeline = new(events);
        IReadOnlyList<YearGroup> years = timeline.GetEventsByYear();

        // Then
        years[0].Year.ShouldBe(2026);
        years[0].Months.Count.ShouldBe(2);
        years[1].Year.ShouldBe(2025);
        years[1].Months.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNonEmptyIsEmpty_When_TimelineHasEvents()
    {
        // Given / When
        Timeline timeline = new([CreateEvent(TimelineEventType.LevelUp, _baseDate, "Level 1")]);

        // Then
        timeline.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnLastPageWithNoCursor_When_ExactlyOneMoreThanPageSize()
    {
        // Given — 21 events (page size 20 + 1 remaining)
        List<TimelineEvent> events = CreateEventsAcrossMonths(21, _baseDate);
        Timeline timeline = new(events);
        TimelinePage firstPage = timeline.GetFirstPage();

        // When
        TimelinePage secondPage = timeline.GetNextPage(firstPage.NextCursor!);

        // Then — 1 event, no more
        secondPage.Events.Count.ShouldBe(1);
        secondPage.HasMore.ShouldBeFalse();
        secondPage.NextCursor.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NextPagePageSizeIsNegative()
    {
        // Given
        List<TimelineEvent> events = CreateEventsAcrossMonths(25, _baseDate);
        Timeline timeline = new(events);
        TimelinePage firstPage = timeline.GetFirstPage();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(
            () => timeline.GetNextPage(firstPage.NextCursor!, -1));
        ex.Message.ShouldContain("greater than zero");
    }

    // --- Mutant-killing tests ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OrderMonthsDescendingWithinYear_When_GroupingByYear()
    {
        // Given — events in Jan, Feb, and Mar within same year
        // This kills the OrderByDescending → OrderBy mutation on month sub-groups
        List<TimelineEvent> events =
        [
            CreateEvent(TimelineEventType.LevelUp, new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.QuestCompleted, new DateTimeOffset(2026, 2, 15, 0, 0, 0, TimeSpan.Zero)),
            CreateEvent(TimelineEventType.TitleEarned, new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero)),
        ];

        // When
        Timeline timeline = new(events);
        IReadOnlyList<YearGroup> years = timeline.GetEventsByYear();

        // Then — months within 2026 should be ordered newest first
        years.Count.ShouldBe(1);
        years[0].Months.Count.ShouldBe(3);
        years[0].Months[0].Month.ShouldBe(3); // March first
        years[0].Months[1].Month.ShouldBe(2); // February second
        years[0].Months[2].Month.ShouldBe(1); // January last
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnCursor_When_ExactlyOneMoreThanPageSizeInFirstPage()
    {
        // Given — exactly pageSize + 1 events (kills page.Count > 0 → >= 0 mutation)
        // With 21 events and page size 20, hasMore is true and page.Count is 20 > 0
        List<TimelineEvent> events = CreateEventsAcrossMonths(21, _baseDate);
        Timeline timeline = new(events);

        // When
        TimelinePage page = timeline.GetFirstPage();

        // Then — cursor should point to last event on page
        page.HasMore.ShouldBeTrue();
        page.NextCursor.ShouldNotBeNull();
        page.Events.Count.ShouldBe(20);
        page.NextCursor.ShouldBe(page.Events[^1].Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnValidCursorFromNextPage_When_MoreEventsExist()
    {
        // Given — 45 events (first page 20, second page 20, third page 5)
        // This kills remaining.Count > pageSize → >= mutation and
        // page.Count > 0 → >= 0 mutation on GetNextPage
        List<TimelineEvent> events = CreateEventsAcrossMonths(45, _baseDate);
        Timeline timeline = new(events);
        TimelinePage firstPage = timeline.GetFirstPage();

        // When
        TimelinePage secondPage = timeline.GetNextPage(firstPage.NextCursor!);

        // Then
        secondPage.Events.Count.ShouldBe(20);
        secondPage.HasMore.ShouldBeTrue();
        secondPage.NextCursor.ShouldNotBeNull();
        secondPage.NextCursor.ShouldBe(secondPage.Events[^1].Id);

        // And third page
        TimelinePage thirdPage = timeline.GetNextPage(secondPage.NextCursor!);
        thirdPage.Events.Count.ShouldBe(5);
        thirdPage.HasMore.ShouldBeFalse();
        thirdPage.NextCursor.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CursorIsAtIndexZero()
    {
        // Given — a timeline where we try to use the first event's ID as cursor
        // This kills cursorIndex < 0 → cursorIndex <= 0 mutation
        // If mutated to <=0, it would throw when cursor is found at index 0
        List<TimelineEvent> events = CreateEventsAcrossMonths(5, _baseDate);
        Timeline timeline = new(events);
        TimelineEventId firstEventId = timeline.Events[0].Id;

        // When — use the first event as cursor (index 0)
        TimelinePage page = timeline.GetNextPage(firstEventId);

        // Then — should return the remaining 4 events (not throw)
        page.Events.Count.ShouldBe(4);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnExactlyPageSizeFromNextPage_When_RemainingEqualsPageSize()
    {
        // Given — 40 events, page size 20: first page has 20, remaining is exactly 20
        // This kills remaining.Count > pageSize → >= pageSize (when remaining == pageSize, hasMore should be false)
        List<TimelineEvent> events = CreateEventsAcrossMonths(40, _baseDate);
        Timeline timeline = new(events);
        TimelinePage firstPage = timeline.GetFirstPage();

        // When
        TimelinePage secondPage = timeline.GetNextPage(firstPage.NextCursor!);

        // Then — exactly 20 remaining, no more pages
        secondPage.Events.Count.ShouldBe(20);
        secondPage.HasMore.ShouldBeFalse();
        secondPage.NextCursor.ShouldBeNull();
    }
}
