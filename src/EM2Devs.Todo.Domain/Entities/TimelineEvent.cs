using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// A single event on a user's journey timeline, recording a significant accomplishment or milestone.
/// Maps to: docs/features/reflection/journey-timeline.feature
/// </summary>
public sealed class TimelineEvent
{
    public TimelineEventId Id { get; }
    public TimelineEventType EventType { get; }
    public DateTimeOffset OccurredAt { get; }
    public string Details { get; }
    public PersonalNote? Note { get; private set; }

    private TimelineEvent(TimelineEventId id, TimelineEventType eventType, DateTimeOffset occurredAt, string details)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(details))
        {
            throw new DomainException("Timeline event details cannot be empty.");
        }

        Id = id;
        EventType = eventType;
        OccurredAt = occurredAt;
        Details = details;
    }

    public static TimelineEvent Create(TimelineEventType eventType, DateTimeOffset occurredAt, string details)
    {
        return new TimelineEvent(TimelineEventId.New(), eventType, occurredAt, details);
    }

    /// <summary>
    /// Rebuilds a timeline event from a persisted snapshot. Details validation is preserved
    /// (empty details is never a valid snapshot, so let the constructor's check catch it).
    /// </summary>
    public static TimelineEvent Reconstitute(
        TimelineEventId id,
        TimelineEventType eventType,
        DateTimeOffset occurredAt,
        string details,
        PersonalNote? note)
    {
        TimelineEvent ev = new(id, eventType, occurredAt, details);
        if (note is not null)
        {
            ev.Note = note;
        }
        return ev;
    }

    public void AddNote(PersonalNote note)
    {
        ArgumentNullException.ThrowIfNull(note);
        Note = note;
    }

    /// <summary>
    /// Converts the event's UTC timestamp to a display string in the user's local timezone.
    /// </summary>
    public string FormatInTimezone(TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        var local = TimeZoneInfo.ConvertTime(OccurredAt, timeZone);
        return local.ToString("MMMM d, yyyy 'at' h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
    }
}
