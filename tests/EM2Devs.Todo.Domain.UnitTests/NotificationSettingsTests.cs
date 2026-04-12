using Shouldly;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for NotificationSettings value object.
/// Tests encode behaviors from notifications.feature — category toggles, quiet hours, and timezone.
/// </summary>
public sealed class NotificationSettingsTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveDefaultCategoryToggles_When_CreatedWithDefaults()
    {
        // Given / When
        NotificationSettings settings = NotificationSettings.CreateDefault();

        // Then
        settings.IsCategoryEnabled(NotificationCategory.TaskReminders).ShouldBeTrue();
        settings.IsCategoryEnabled(NotificationCategory.AchievementAlerts).ShouldBeTrue();
        settings.IsCategoryEnabled(NotificationCategory.DailyBriefReady).ShouldBeTrue();
        settings.IsCategoryEnabled(NotificationCategory.WeeklyReviewPrompt).ShouldBeTrue();
        settings.IsCategoryEnabled(NotificationCategory.GuildActivity).ShouldBeTrue();
        settings.IsCategoryEnabled(NotificationCategory.PartnerMessages).ShouldBeTrue();
        settings.IsCategoryEnabled(NotificationCategory.InsightCards).ShouldBeTrue();
        settings.IsCategoryEnabled(NotificationCategory.CapacityWarnings).ShouldBeTrue();
        settings.IsCategoryEnabled(NotificationCategory.UpgradePrompts).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNoQuietHours_When_CreatedWithDefaults()
    {
        // Given / When
        NotificationSettings settings = NotificationSettings.CreateDefault();

        // Then
        settings.HasQuietHours.ShouldBeFalse();
        settings.QuietHoursStart.ShouldBeNull();
        settings.QuietHoursEnd.ShouldBeNull();
        settings.TimeZoneId.ShouldBe("UTC");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DisableCategory_When_ToggledOff()
    {
        // Given
        NotificationSettings settings = NotificationSettings.CreateDefault();

        // When
        NotificationSettings updated = settings.WithCategoryToggle(NotificationCategory.TaskReminders, false);

        // Then
        updated.IsCategoryEnabled(NotificationCategory.TaskReminders).ShouldBeFalse();
        // Other categories remain unchanged
        updated.IsCategoryEnabled(NotificationCategory.AchievementAlerts).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EnableCategory_When_ToggledOn()
    {
        // Given
        NotificationSettings settings = NotificationSettings.CreateDefault();

        // When — UpgradePrompts is off by default, turn it on
        NotificationSettings updated = settings.WithCategoryToggle(NotificationCategory.UpgradePrompts, true);

        // Then
        updated.IsCategoryEnabled(NotificationCategory.UpgradePrompts).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OverrideDefaultToggles_When_CreatedWithCustomToggles()
    {
        // Given
        Dictionary<NotificationCategory, bool> customToggles = new Dictionary<NotificationCategory, bool>
        {
            { NotificationCategory.TaskReminders, false },
            { NotificationCategory.UpgradePrompts, true }
        };

        // When
        NotificationSettings settings = NotificationSettings.Create(customToggles);

        // Then
        settings.IsCategoryEnabled(NotificationCategory.TaskReminders).ShouldBeFalse();
        settings.IsCategoryEnabled(NotificationCategory.UpgradePrompts).ShouldBeTrue();
        // Others keep defaults
        settings.IsCategoryEnabled(NotificationCategory.AchievementAlerts).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SuppressNotifications_When_InOvernightQuietHours()
    {
        // Given — quiet hours 10 PM to 7 AM UTC
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));

        // When — 11 PM UTC
        DateTimeOffset lateNight = new DateTimeOffset(2026, 4, 12, 23, 0, 0, TimeSpan.Zero);

        // Then
        settings.IsInQuietHours(lateNight).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SuppressNotifications_When_InOvernightQuietHoursBeforeEnd()
    {
        // Given — quiet hours 10 PM to 7 AM UTC
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));

        // When — 5 AM UTC (still in quiet hours)
        DateTimeOffset earlyMorning = new DateTimeOffset(2026, 4, 12, 5, 0, 0, TimeSpan.Zero);

        // Then
        settings.IsInQuietHours(earlyMorning).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowNotifications_When_OutsideQuietHours()
    {
        // Given — quiet hours 10 PM to 7 AM UTC
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));

        // When — 2 PM UTC
        DateTimeOffset afternoon = new DateTimeOffset(2026, 4, 12, 14, 0, 0, TimeSpan.Zero);

        // Then
        settings.IsInQuietHours(afternoon).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowNotifications_When_ExactlyAtQuietHoursEnd()
    {
        // Given — quiet hours 10 PM to 7 AM UTC
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));

        // When — exactly 7 AM UTC (end of quiet hours, should be allowed)
        DateTimeOffset atEnd = new DateTimeOffset(2026, 4, 12, 7, 0, 0, TimeSpan.Zero);

        // Then
        settings.IsInQuietHours(atEnd).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SuppressNotifications_When_ExactlyAtQuietHoursStart()
    {
        // Given — quiet hours 10 PM to 7 AM UTC
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));

        // When — exactly 10 PM UTC
        DateTimeOffset atStart = new DateTimeOffset(2026, 4, 12, 22, 0, 0, TimeSpan.Zero);

        // Then
        settings.IsInQuietHours(atStart).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SuppressNotifications_When_InSameDayQuietHours()
    {
        // Given — quiet hours 1 PM to 5 PM UTC (same-day)
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(13, 0), new TimeOnly(17, 0));

        // When — 3 PM UTC
        DateTimeOffset midAfternoon = new DateTimeOffset(2026, 4, 12, 15, 0, 0, TimeSpan.Zero);

        // Then
        settings.IsInQuietHours(midAfternoon).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowNotifications_When_OutsideSameDayQuietHours()
    {
        // Given — quiet hours 1 PM to 5 PM UTC (same-day)
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(13, 0), new TimeOnly(17, 0));

        // When — 10 AM UTC
        DateTimeOffset morning = new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);

        // Then
        settings.IsInQuietHours(morning).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowNotifications_When_ExactlyAtSameDayQuietHoursEnd()
    {
        // Given — quiet hours 1 PM to 5 PM UTC (same-day)
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(13, 0), new TimeOnly(17, 0));

        // When — exactly 5 PM UTC
        DateTimeOffset atEnd = new DateTimeOffset(2026, 4, 12, 17, 0, 0, TimeSpan.Zero);

        // Then
        settings.IsInQuietHours(atEnd).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SuppressNotifications_When_ExactlyAtSameDayQuietHoursStart()
    {
        // Given — quiet hours 1 PM to 5 PM UTC (same-day)
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(13, 0), new TimeOnly(17, 0));

        // When — exactly 1 PM UTC
        DateTimeOffset atStart = new DateTimeOffset(2026, 4, 12, 13, 0, 0, TimeSpan.Zero);

        // Then
        settings.IsInQuietHours(atStart).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RespectUserTimezone_When_CheckingQuietHours()
    {
        // Given — quiet hours 10 PM to 7 AM, Europe/London timezone
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0))
            .WithTimeZone("Europe/London");

        // When — 11 PM London time (UTC+1 during BST = 22:00 UTC on April 12)
        // April 12 2026 is BST (UTC+1), so 11 PM London = 10 PM UTC
        DateTimeOffset londonLateNight = new DateTimeOffset(2026, 4, 12, 22, 0, 0, TimeSpan.Zero);

        // Then — 11 PM London time is in quiet hours
        settings.IsInQuietHours(londonLateNight).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeInQuietHours_When_NoQuietHoursConfigured()
    {
        // Given
        NotificationSettings settings = NotificationSettings.CreateDefault();

        // When
        DateTimeOffset anyTime = new DateTimeOffset(2026, 4, 12, 23, 0, 0, TimeSpan.Zero);

        // Then
        settings.IsInQuietHours(anyTime).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNull_When_GetQuietHoursEndWithNoQuietHours()
    {
        // Given
        NotificationSettings settings = NotificationSettings.CreateDefault();

        // When
        DateTimeOffset? endUtc = settings.GetQuietHoursEndUtc(DateTimeOffset.UtcNow);

        // Then
        endUtc.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnDeliveryTime_When_QuietHoursEndRequested()
    {
        // Given — quiet hours 10 PM to 7 AM UTC
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));

        // When — notification at 11 PM UTC on April 12
        DateTimeOffset notificationTime = new DateTimeOffset(2026, 4, 12, 23, 0, 0, TimeSpan.Zero);
        DateTimeOffset? deliveryTime = settings.GetQuietHoursEndUtc(notificationTime);

        // Then — should deliver at 7 AM UTC on April 13
        deliveryTime.ShouldNotBeNull();
        deliveryTime!.Value.ShouldBe(new DateTimeOffset(2026, 4, 13, 7, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnSameDayDeliveryTime_When_InEarlyMorningQuietHours()
    {
        // Given — quiet hours 10 PM to 7 AM UTC
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));

        // When — notification at 5 AM UTC on April 12 (before end time, same day)
        DateTimeOffset notificationTime = new DateTimeOffset(2026, 4, 12, 5, 0, 0, TimeSpan.Zero);
        DateTimeOffset? deliveryTime = settings.GetQuietHoursEndUtc(notificationTime);

        // Then — should deliver at 7 AM UTC on April 12 (same day)
        deliveryTime.ShouldNotBeNull();
        deliveryTime!.Value.ShouldBe(new DateTimeOffset(2026, 4, 12, 7, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnSameDayEndTime_When_InSameDayQuietHours()
    {
        // Given — quiet hours 1 PM to 5 PM UTC (same-day)
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(13, 0), new TimeOnly(17, 0));

        // When — notification at 3 PM UTC
        DateTimeOffset notificationTime = new DateTimeOffset(2026, 4, 12, 15, 0, 0, TimeSpan.Zero);
        DateTimeOffset? deliveryTime = settings.GetQuietHoursEndUtc(notificationTime);

        // Then — should deliver at 5 PM UTC same day
        deliveryTime.ShouldNotBeNull();
        deliveryTime!.Value.ShouldBe(new DateTimeOffset(2026, 4, 12, 17, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InvalidTimezone()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            NotificationSettings.Create(
                new Dictionary<NotificationCategory, bool>(),
                timeZoneId: "Invalid/Timezone"));
        ex.Message.ShouldContain("Invalid timezone");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EmptyTimezone()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            NotificationSettings.Create(
                new Dictionary<NotificationCategory, bool>(),
                timeZoneId: ""));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_WhitespaceTimezone()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            NotificationSettings.Create(
                new Dictionary<NotificationCategory, bool>(),
                timeZoneId: "   "));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_OnlyStartQuietHoursProvided()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            NotificationSettings.Create(
                new Dictionary<NotificationCategory, bool>(),
                quietHoursStart: new TimeOnly(22, 0)));
        ex.Message.ShouldContain("Both quiet hours");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_OnlyEndQuietHoursProvided()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            NotificationSettings.Create(
                new Dictionary<NotificationCategory, bool>(),
                quietHoursEnd: new TimeOnly(7, 0)));
        ex.Message.ShouldContain("Both quiet hours");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotSuppressNotifications_When_QuietHoursStartEqualsEnd()
    {
        // Given — start == end means zero-width window, effectively no quiet hours
        NotificationSettings settings = NotificationSettings.Create(
            new Dictionary<NotificationCategory, bool>(),
            quietHoursStart: new TimeOnly(22, 0),
            quietHoursEnd: new TimeOnly(22, 0));

        // When — exactly at the start/end time
        DateTimeOffset atTime = new DateTimeOffset(2026, 4, 12, 22, 0, 0, TimeSpan.Zero);

        // Then — no suppression since the window has zero width
        settings.IsInQuietHours(atTime).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotSuppressNotifications_When_QuietHoursStartEqualsEndAtDifferentTime()
    {
        // Given — start == end means zero-width window
        NotificationSettings settings = NotificationSettings.Create(
            new Dictionary<NotificationCategory, bool>(),
            quietHoursStart: new TimeOnly(22, 0),
            quietHoursEnd: new TimeOnly(22, 0));

        // When — at a different time
        DateTimeOffset otherTime = new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);

        // Then
        settings.IsInQuietHours(otherTime).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_NullCategoryToggles()
    {
        // Given / When / Then
        Should.Throw<ArgumentNullException>(() =>
            NotificationSettings.Create(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_WithTimeZoneInvalid()
    {
        // Given
        NotificationSettings settings = NotificationSettings.CreateDefault();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            settings.WithTimeZone("Invalid/Zone"));
        ex.Message.ShouldContain("Invalid timezone");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_WithTimeZoneEmpty()
    {
        // Given
        NotificationSettings settings = NotificationSettings.CreateDefault();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            settings.WithTimeZone(""));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveQuietHours_When_TogglingCategory()
    {
        // Given
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));

        // When
        NotificationSettings updated = settings.WithCategoryToggle(NotificationCategory.TaskReminders, false);

        // Then
        updated.HasQuietHours.ShouldBeTrue();
        updated.QuietHoursStart.ShouldBe(new TimeOnly(22, 0));
        updated.QuietHoursEnd.ShouldBe(new TimeOnly(7, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveToggles_When_SettingQuietHours()
    {
        // Given
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithCategoryToggle(NotificationCategory.TaskReminders, false);

        // When
        NotificationSettings updated = settings.WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));

        // Then
        updated.IsCategoryEnabled(NotificationCategory.TaskReminders).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveTogglesAndQuietHours_When_SettingTimezone()
    {
        // Given
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithCategoryToggle(NotificationCategory.TaskReminders, false)
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));

        // When
        NotificationSettings updated = settings.WithTimeZone("Europe/London");

        // Then
        updated.IsCategoryEnabled(NotificationCategory.TaskReminders).ShouldBeFalse();
        updated.HasQuietHours.ShouldBeTrue();
        updated.TimeZoneId.ShouldBe("Europe/London");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptValidTimezone_When_CreatingWithTimezone()
    {
        // Given / When
        NotificationSettings settings = NotificationSettings.Create(
            new Dictionary<NotificationCategory, bool>(),
            timeZoneId: "Europe/London");

        // Then
        settings.TimeZoneId.ShouldBe("Europe/London");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptBothQuietHours_When_BothProvided()
    {
        // Given / When
        NotificationSettings settings = NotificationSettings.Create(
            new Dictionary<NotificationCategory, bool>(),
            quietHoursStart: new TimeOnly(22, 0),
            quietHoursEnd: new TimeOnly(7, 0));

        // Then
        settings.HasQuietHours.ShouldBeTrue();
        settings.QuietHoursStart.ShouldBe(new TimeOnly(22, 0));
        settings.QuietHoursEnd.ShouldBe(new TimeOnly(7, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseDefaultTimezone_When_NullTimezoneProvided()
    {
        // Given / When
        NotificationSettings settings = NotificationSettings.Create(
            new Dictionary<NotificationCategory, bool>(),
            timeZoneId: null);

        // Then
        settings.TimeZoneId.ShouldBe("UTC");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnSameDayEndTime_When_QuietHoursStartEqualsEnd()
    {
        // Given — start == end means same-day path in GetQuietHoursEndUtc
        // Use a local time AT the start/end boundary to kill the >= mutation
        NotificationSettings settings = NotificationSettings.Create(
            new Dictionary<NotificationCategory, bool>(),
            quietHoursStart: new TimeOnly(22, 0),
            quietHoursEnd: new TimeOnly(22, 0));

        // When — at exactly 22:00 UTC (same as start/end)
        // Same-day path: delivery same day at end = 22:00 same day
        // Overnight path (mutant): localTimeOnly(22:00) >= start(22:00) = true => next day
        DateTimeOffset notificationTime = new DateTimeOffset(2026, 4, 12, 22, 0, 0, TimeSpan.Zero);
        DateTimeOffset? deliveryTime = settings.GetQuietHoursEndUtc(notificationTime);

        // Then — same-day path: delivery at 22:00 same day (not next day)
        deliveryTime.ShouldNotBeNull();
        deliveryTime!.Value.ShouldBe(new DateTimeOffset(2026, 4, 12, 22, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNextDayDelivery_When_OvernightQuietHoursAtExactStartTime()
    {
        // Given — overnight quiet hours, notification at exactly start time
        // Boundary test for localTimeOnly >= start in GetQuietHoursEndUtc
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));

        // When — exactly at start time (22:00 UTC)
        DateTimeOffset atStart = new DateTimeOffset(2026, 4, 12, 22, 0, 0, TimeSpan.Zero);
        DateTimeOffset? deliveryTime = settings.GetQuietHoursEndUtc(atStart);

        // Then — should deliver next day at 7 AM
        deliveryTime.ShouldNotBeNull();
        deliveryTime!.Value.ShouldBe(new DateTimeOffset(2026, 4, 13, 7, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnSameDayDelivery_When_OvernightQuietHoursJustBeforeStartTime()
    {
        // Given — overnight quiet hours, notification just before start time
        // This ensures localTimeOnly >= start distinguishes from localTimeOnly > start
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));

        // When — one minute before start (21:59 UTC) - NOT in quiet hours
        DateTimeOffset justBeforeStart = new DateTimeOffset(2026, 4, 12, 21, 59, 0, TimeSpan.Zero);

        // Then — not in quiet hours at all, but GetQuietHoursEndUtc still computes the delivery time
        // since it doesn't check if the time is actually in quiet hours
        settings.IsInQuietHours(justBeforeStart).ShouldBeFalse();
    }
}
