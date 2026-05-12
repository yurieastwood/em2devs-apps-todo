using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Coverage for the Reconstitute(...) factories on entities that support data import.
/// Each factory takes a flat field set and rebuilds the entity bypassing the
/// incremental-state construction; the tests here cover the field assignments and
/// edge cases for mutation kill rate.
/// </summary>
public sealed class ReconstituteFactoryTests
{
    private static readonly Guid _userId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly DateTimeOffset _now = new(2026, 5, 1, 12, 0, 0, TimeSpan.Zero);

    // -- Epic --

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReconstituteEpic_When_AllFieldsProvided()
    {
        EpicId id = new(new Guid("11111111-1111-1111-1111-111111111111"));
        SagaId sagaId = new(new Guid("22222222-2222-2222-2222-222222222222"));
        Epic e = Epic.Reconstitute(id, new EpicTitle("Saga arc"), "desc", new DateOnly(2026, 6, 1), true, sagaId);

        e.Id.ShouldBe(id);
        e.Title.Value.ShouldBe("Saga arc");
        e.Description.ShouldBe("desc");
        e.TargetDate.ShouldBe(new DateOnly(2026, 6, 1));
        e.IsCompleted.ShouldBeTrue();
        e.SagaId.ShouldBe(sagaId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ReconstituteEpicWithNullId() =>
        Should.Throw<ArgumentNullException>(() =>
            Epic.Reconstitute(null!, new EpicTitle("X"), "d", null, false, null));

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ReconstituteEpicWithNullTitle() =>
        Should.Throw<ArgumentNullException>(() =>
            Epic.Reconstitute(EpicId.New(), null!, "d", null, false, null));

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ReconstituteEpicWithNullDescription() =>
        Should.Throw<ArgumentNullException>(() =>
            Epic.Reconstitute(EpicId.New(), new EpicTitle("X"), null!, null, false, null));

    // -- RecurringTask --

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReconstituteRecurringTask_When_AllFieldsProvided()
    {
        RecurringTaskId id = new(Guid.NewGuid());
        RecurringTask rt = RecurringTask.Reconstitute(
            id, _userId, new TaskTitle("Daily"), RecurrencePattern.Daily, isActive: false,
            endDate: new DateOnly(2026, 12, 1));

        rt.Id.ShouldBe(id);
        rt.UserId.ShouldBe(_userId);
        rt.Title.Value.ShouldBe("Daily");
        rt.Pattern.ShouldBe(RecurrencePattern.Daily);
        rt.IsActive.ShouldBeFalse();
        rt.EndDate.ShouldBe(new DateOnly(2026, 12, 1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ReconstituteRecurringTaskWithNullId() =>
        Should.Throw<ArgumentNullException>(() =>
            RecurringTask.Reconstitute(null!, _userId, new TaskTitle("X"), RecurrencePattern.Daily, true, null));

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ReconstituteRecurringTaskWithNullTitle() =>
        Should.Throw<ArgumentNullException>(() =>
            RecurringTask.Reconstitute(RecurringTaskId.New(), _userId, null!, RecurrencePattern.Daily, true, null));

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ReconstituteRecurringTaskWithEmptyUserId()
    {
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(() =>
            RecurringTask.Reconstitute(RecurringTaskId.New(), Guid.Empty, new TaskTitle("X"), RecurrencePattern.Daily, true, null));
        ex.Message.ShouldBe("UserId cannot be empty.");
    }

    // -- Notification --

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReconstituteNotification_When_AllFieldsProvided()
    {
        NotificationId id = new(Guid.NewGuid());
        Notification n = Notification.Reconstitute(
            id, _userId, NotificationType.AchievementAlert, "Level up!",
            NotificationStatus.Read, _now, _now.AddMinutes(5));

        n.Id.ShouldBe(id);
        n.UserId.ShouldBe(_userId);
        n.Type.ShouldBe(NotificationType.AchievementAlert);
        n.Message.ShouldBe("Level up!");
        n.Status.ShouldBe(NotificationStatus.Read);
        n.CreatedAt.ShouldBe(_now);
        n.ReadAt.ShouldBe(_now.AddMinutes(5));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ReconstituteNotificationWithNullId() =>
        Should.Throw<ArgumentNullException>(() =>
            Notification.Reconstitute(null!, _userId, NotificationType.AchievementAlert, "m", NotificationStatus.Unread, _now, null));

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ReconstituteNotificationWithNullMessage() =>
        Should.Throw<ArgumentNullException>(() =>
            Notification.Reconstitute(NotificationId.New(), _userId, NotificationType.AchievementAlert, null!, NotificationStatus.Unread, _now, null));

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ReconstituteNotificationWithEmptyUserId()
    {
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(() =>
            Notification.Reconstitute(NotificationId.New(), Guid.Empty, NotificationType.AchievementAlert, "m", NotificationStatus.Unread, _now, null));
        ex.Message.ShouldBe("UserId cannot be empty.");
    }

    // -- InsightCard --

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReconstituteInsightCard_When_AllFieldsProvided()
    {
        InsightCardId id = new(Guid.NewGuid());
        InsightCard c = InsightCard.Reconstitute(
            id, InsightType.MorningProductivityPeak, "msg", "data",
            InsightCardStatus.Saved, new DateOnly(2026, 5, 10), isValidated: true);

        c.Id.ShouldBe(id);
        c.Type.ShouldBe(InsightType.MorningProductivityPeak);
        c.Message.ShouldBe("msg");
        c.SupportingData.ShouldBe("data");
        c.Status.ShouldBe(InsightCardStatus.Saved);
        c.GeneratedAt.ShouldBe(new DateOnly(2026, 5, 10));
        c.IsValidated.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ReconstituteInsightCardWithNullId() =>
        Should.Throw<ArgumentNullException>(() =>
            InsightCard.Reconstitute(null!, InsightType.MorningProductivityPeak, "m", "d", InsightCardStatus.Unread, new DateOnly(2026, 5, 1), true));

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ReconstituteInsightCardWithNullMessage() =>
        Should.Throw<ArgumentNullException>(() =>
            InsightCard.Reconstitute(InsightCardId.New(), InsightType.MorningProductivityPeak, null!, "d", InsightCardStatus.Unread, new DateOnly(2026, 5, 1), true));

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ReconstituteInsightCardWithNullSupportingData() =>
        Should.Throw<ArgumentNullException>(() =>
            InsightCard.Reconstitute(InsightCardId.New(), InsightType.MorningProductivityPeak, "m", null!, InsightCardStatus.Unread, new DateOnly(2026, 5, 1), true));

    // -- EnergyCheckIn --

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReconstituteEnergyCheckIn_When_AllFieldsProvided()
    {
        EnergyCheckInId id = new(Guid.NewGuid());
        EnergyCheckIn e = EnergyCheckIn.Reconstitute(
            id, EnergyLevel.High, _now, previousLevel: EnergyLevel.Low, hasFluctuated: true);

        e.Id.ShouldBe(id);
        e.Level.ShouldBe(EnergyLevel.High);
        e.RecordedAt.ShouldBe(_now);
        e.PreviousLevel.ShouldBe(EnergyLevel.Low);
        e.HasFluctuated.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReconstituteEnergyCheckIn_When_FirstCheckIn()
    {
        EnergyCheckInId id = new(Guid.NewGuid());
        EnergyCheckIn e = EnergyCheckIn.Reconstitute(id, EnergyLevel.Medium, _now, null, false);

        e.PreviousLevel.ShouldBeNull();
        e.HasFluctuated.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ReconstituteEnergyCheckInWithNullId() =>
        Should.Throw<ArgumentNullException>(() =>
            EnergyCheckIn.Reconstitute(null!, EnergyLevel.Medium, _now, null, false));

    // -- TimelineEvent --

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReconstituteTimelineEvent_WithoutNote_When_NoteNull()
    {
        TimelineEventId id = new(Guid.NewGuid());
        TimelineEvent ev = TimelineEvent.Reconstitute(
            id, TimelineEventType.LevelUp, _now, "Reached level 3", note: null);

        ev.Id.ShouldBe(id);
        ev.EventType.ShouldBe(TimelineEventType.LevelUp);
        ev.OccurredAt.ShouldBe(_now);
        ev.Details.ShouldBe("Reached level 3");
        ev.Note.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReconstituteTimelineEvent_WithNote_When_NoteProvided()
    {
        TimelineEventId id = new(Guid.NewGuid());
        PersonalNote note = new("My reflection", _now);
        TimelineEvent ev = TimelineEvent.Reconstitute(
            id, TimelineEventType.LevelUp, _now, "Reached level 3", note);

        ev.Note.ShouldNotBeNull();
        ev.Note!.Text.ShouldBe("My reflection");
    }
}
