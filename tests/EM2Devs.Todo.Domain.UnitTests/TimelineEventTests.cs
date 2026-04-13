using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for TimelineEvent entity and related value objects.
/// Maps to: docs/features/reflection/journey-timeline.feature
/// Rule: "Significant events are automatically added to the timeline"
/// Rule: "Users can browse, filter, and annotate their timeline"
/// </summary>
public sealed class TimelineEventTests
{
    private static readonly DateTimeOffset _now = new(2026, 3, 15, 14, 30, 0, TimeSpan.Zero);

    // --- Scenario: Event types appear on the timeline (14 event types) ---

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(TimelineEventType.LevelUp)]
    [InlineData(TimelineEventType.QuestCompleted)]
    [InlineData(TimelineEventType.EpicCompleted)]
    [InlineData(TimelineEventType.SagaCompleted)]
    [InlineData(TimelineEventType.BossTaskDefeated)]
    [InlineData(TimelineEventType.TitleEarned)]
    [InlineData(TimelineEventType.SkillTreeUnlocked)]
    [InlineData(TimelineEventType.SkillTreeTierAdvanced)]
    [InlineData(TimelineEventType.StreakMilestone)]
    [InlineData(TimelineEventType.SeasonalQuestLineCompleted)]
    [InlineData(TimelineEventType.GuildJoined)]
    [InlineData(TimelineEventType.GuildQuestCompleted)]
    [InlineData(TimelineEventType.ChallengeWon)]
    [InlineData(TimelineEventType.WeeklyReviewStreakMilestone)]
    public void Should_CreateTimelineEvent_When_EventTypeIsValid(TimelineEventType eventType)
    {
        // Given / When
        TimelineEvent evt = TimelineEvent.Create(eventType, _now, "Test event details");

        // Then
        evt.Id.ShouldNotBeNull();
        evt.EventType.ShouldBe(eventType);
        evt.OccurredAt.ShouldBe(_now);
        evt.Details.ShouldBe("Test event details");
        evt.Note.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DetailsAreEmpty()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(
            () => TimelineEvent.Create(TimelineEventType.LevelUp, _now, ""));
        ex.Message.ShouldContain("details cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DetailsAreWhitespace()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(
            () => TimelineEvent.Create(TimelineEventType.LevelUp, _now, "   "));
        ex.Message.ShouldContain("details cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_IdIsNull()
    {
        // Given — the private ctor guards against null ID; access it via reflection
        System.Reflection.ConstructorInfo? ctor = typeof(TimelineEvent).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            [typeof(TimelineEventId), typeof(TimelineEventType), typeof(DateTimeOffset), typeof(string)],
            null);
        ctor.ShouldNotBeNull();

        // When / Then — TargetInvocationException wraps the ArgumentNullException
        System.Reflection.TargetInvocationException ex = Should.Throw<System.Reflection.TargetInvocationException>(
            () => ctor!.Invoke([null!, TimelineEventType.LevelUp, _now, "details"]));
        ex.InnerException.ShouldBeOfType<ArgumentNullException>();
    }

    // --- Scenario: Add a personal note to a timeline event ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddNote_When_NoteIsValid()
    {
        // Given
        TimelineEvent evt = TimelineEvent.Create(TimelineEventType.QuestCompleted, _now, "Prepare conference talk");
        PersonalNote note = new("First ever conference talk - terrifying but worth it!", _now);

        // When
        evt.AddNote(note);

        // Then
        evt.Note.ShouldNotBeNull();
        evt.Note!.Text.ShouldBe("First ever conference talk - terrifying but worth it!");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_NoteIsNull()
    {
        // Given
        TimelineEvent evt = TimelineEvent.Create(TimelineEventType.LevelUp, _now, "Level 10 reached");

        // When / Then
        Should.Throw<ArgumentNullException>(() => evt.AddNote(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReplaceExistingNote_When_AddingNewNote()
    {
        // Given
        TimelineEvent evt = TimelineEvent.Create(TimelineEventType.QuestCompleted, _now, "Some quest");
        evt.AddNote(new PersonalNote("Old note", _now));

        // When
        PersonalNote newNote = new("Updated note", _now.AddMinutes(5));
        evt.AddNote(newNote);

        // Then
        evt.Note!.Text.ShouldBe("Updated note");
    }

    // --- Scenario: Timeline events display in the user's local timezone ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_FormatInTimezone_When_ConvertingToEasternTime()
    {
        // Given — event at 2026-01-15T03:00:00Z (UTC)
        DateTimeOffset utcTime = new(2026, 1, 15, 3, 0, 0, TimeSpan.Zero);
        TimelineEvent evt = TimelineEvent.Create(TimelineEventType.LevelUp, utcTime, "Reached level 10");
        TimeZoneInfo eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

        // When
        string formatted = evt.FormatInTimezone(eastern);

        // Then — UTC 03:00 = EST 22:00 (previous day)
        formatted.ShouldBe("January 14, 2026 at 10:00 PM");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_TimezoneIsNull()
    {
        // Given
        TimelineEvent evt = TimelineEvent.Create(TimelineEventType.LevelUp, _now, "Level up");

        // When / Then — kills Statement mutation on ArgumentNullException.ThrowIfNull
        ArgumentNullException ex = Should.Throw<ArgumentNullException>(() => evt.FormatInTimezone(null!));
        ex.ParamName.ShouldBe("timeZone");
    }

    // --- Scenario: View timeline event details ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeDateAndDetails_When_ViewingEvent()
    {
        // Given / When
        TimelineEvent evt = TimelineEvent.Create(
            TimelineEventType.LevelUp, _now, "Reached level 10 with 150 XP trigger");

        // Then
        evt.OccurredAt.ShouldBe(_now);
        evt.Details.ShouldContain("level 10");
        evt.Details.ShouldContain("150 XP");
        evt.EventType.ShouldBe(TimelineEventType.LevelUp);
    }

    // --- PersonalNote value object tests ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreatePersonalNote_When_TextIsValid()
    {
        // Given / When
        PersonalNote note = new("My best quest yet!", _now);

        // Then
        note.Text.ShouldBe("My best quest yet!");
        note.CreatedAt.ShouldBe(_now);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NoteTextIsEmpty()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() => new PersonalNote("", _now));
        ex.Message.ShouldContain("note text cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NoteTextIsWhitespace()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() => new PersonalNote("   ", _now));
        ex.Message.ShouldContain("note text cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NoteTextExceedsMaxLength()
    {
        // Given
        string longText = new('a', PersonalNote.MaxLength + 1);

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => new PersonalNote(longText, _now));
        ex.Message.ShouldContain($"cannot exceed {PersonalNote.MaxLength}");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptNote_When_TextIsExactlyMaxLength()
    {
        // Given
        string maxText = new('a', PersonalNote.MaxLength);

        // When
        PersonalNote note = new(maxText, _now);

        // Then
        note.Text.Length.ShouldBe(PersonalNote.MaxLength);
    }

    // --- TimelineEventId tests ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateUniqueIds_When_GeneratingNewTimelineEventIds()
    {
        // Given / When
        TimelineEventId id1 = TimelineEventId.New();
        TimelineEventId id2 = TimelineEventId.New();

        // Then
        id1.ShouldNotBe(id2);
        id1.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeEqual_When_TimelineEventIdsHaveSameGuid()
    {
        // Given
        Guid guid = Guid.NewGuid();

        // When
        TimelineEventId id1 = new(guid);
        TimelineEventId id2 = new(guid);

        // Then
        id1.ShouldBe(id2);
    }

    // --- TimelineEventType enum completeness ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Have14EventTypes_When_EnumeratingTimelineEventTypes()
    {
        // Given / When
        TimelineEventType[] types = Enum.GetValues<TimelineEventType>();

        // Then — matches the 14 event types in the scenario outline
        types.Length.ShouldBe(14);
    }
}
