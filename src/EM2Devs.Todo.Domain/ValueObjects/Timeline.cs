using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Manages a user's journey timeline: ordering, grouping, filtering, and pagination.
/// Maps to: docs/features/reflection/journey-timeline.feature
/// </summary>
public sealed class Timeline
{
    public const int DefaultPageSize = 20;
    public const string EmptyTimelineMessage =
        "Your journey is just beginning! Complete quests, earn titles, and level up to build your timeline.";

    private readonly List<TimelineEvent> _events;

    public IReadOnlyList<TimelineEvent> Events => _events.AsReadOnly();
    public bool IsEmpty => _events.Count == 0;

    public Timeline(IEnumerable<TimelineEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        _events = events.OrderByDescending(e => e.OccurredAt).ToList();
    }

    public static Timeline Empty() => new([]);

    /// <summary>
    /// Returns events in reverse chronological order (newest first).
    /// </summary>
    public IReadOnlyList<TimelineEvent> GetChronologicalEvents()
    {
        return _events.AsReadOnly();
    }

    /// <summary>
    /// Groups events by month, returning groups ordered by most recent month first.
    /// Each group includes a summary count.
    /// </summary>
    public IReadOnlyList<MonthGroup> GetEventsByMonth()
    {
        return _events
            .GroupBy(e => new { e.OccurredAt.Year, e.OccurredAt.Month })
            .OrderByDescending(g => g.Key.Year)
            .ThenByDescending(g => g.Key.Month)
            .Select(g => new MonthGroup(g.Key.Year, g.Key.Month, g.ToList().AsReadOnly()))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Groups events by year, returning groups ordered by most recent year first.
    /// Each year contains month groups.
    /// </summary>
    public IReadOnlyList<YearGroup> GetEventsByYear()
    {
        return _events
            .GroupBy(e => e.OccurredAt.Year)
            .OrderByDescending(g => g.Key)
            .Select(g => new YearGroup(
                g.Key,
                g.GroupBy(e => e.OccurredAt.Month)
                 .OrderByDescending(mg => mg.Key)
                 .Select(mg => new MonthGroup(g.Key, mg.Key, mg.ToList().AsReadOnly()))
                 .ToList()
                 .AsReadOnly()))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Returns true when events span more than one year.
    /// </summary>
    public bool SpansMultipleYears()
    {
        if (_events.Count == 0)
        {
            return false;
        }
        return _events[0].OccurredAt.Year != _events[^1].OccurredAt.Year;
    }

    /// <summary>
    /// Filters the timeline to show only events of a specific type.
    /// Personal notes are preserved on filtered events.
    /// </summary>
    public Timeline FilterByEventType(TimelineEventType eventType)
    {
        IEnumerable<TimelineEvent> filtered = _events.Where(e => e.EventType == eventType);
        return new Timeline(filtered);
    }

    /// <summary>
    /// Returns the first page of events (cursor-based pagination).
    /// </summary>
    public TimelinePage GetFirstPage(int pageSize = DefaultPageSize)
    {
        if (pageSize <= 0)
        {
            throw new Exceptions.DomainException("Page size must be greater than zero.");
        }

        System.Collections.ObjectModel.ReadOnlyCollection<TimelineEvent> page = _events.Take(pageSize).ToList().AsReadOnly();
        bool hasMore = _events.Count > pageSize;
        TimelineEventId? cursor = hasMore ? page[^1].Id : null;
        return new TimelinePage(page, hasMore, cursor);
    }

    /// <summary>
    /// Returns the next page of events after the given cursor.
    /// </summary>
    public TimelinePage GetNextPage(TimelineEventId cursor, int pageSize = DefaultPageSize)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        if (pageSize <= 0)
        {
            throw new Exceptions.DomainException("Page size must be greater than zero.");
        }

        int cursorIndex = _events.FindIndex(e => e.Id == cursor);
        if (cursorIndex < 0)
        {
            throw new Exceptions.DomainException("Cursor not found in timeline.");
        }

        List<TimelineEvent> remaining = _events.Skip(cursorIndex + 1).ToList();
        System.Collections.ObjectModel.ReadOnlyCollection<TimelineEvent> page = remaining.Take(pageSize).ToList().AsReadOnly();
        bool hasMore = remaining.Count > pageSize;
        TimelineEventId? nextCursor = hasMore ? page[^1].Id : null;
        return new TimelinePage(page, hasMore, nextCursor);
    }

    /// <summary>
    /// Returns the encouraging message shown when the timeline is empty.
    /// </summary>
    public static string GetEmptyMessage()
    {
        return EmptyTimelineMessage;
    }

    /// <summary>
    /// Returns the event types that will appear on the timeline, for display on empty state.
    /// </summary>
    public static IReadOnlyList<TimelineEventType> GetAvailableEventTypes()
    {
        return Enum.GetValues<TimelineEventType>().ToList().AsReadOnly();
    }
}

/// <summary>
/// A group of timeline events within a single month.
/// </summary>
public sealed record MonthGroup(int Year, int Month, IReadOnlyList<TimelineEvent> Events)
{
    public int Count => Events.Count;
    public string Header => new DateOnly(Year, Month, 1).ToString("MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
}

/// <summary>
/// A group of timeline events within a single year, containing month sub-groups.
/// </summary>
public sealed record YearGroup(int Year, IReadOnlyList<MonthGroup> Months)
{
    public string Header => Year.ToString(System.Globalization.CultureInfo.InvariantCulture);
}

/// <summary>
/// A page of timeline events with cursor-based pagination support.
/// </summary>
public sealed record TimelinePage(
    IReadOnlyList<TimelineEvent> Events,
    bool HasMore,
    TimelineEventId? NextCursor);
