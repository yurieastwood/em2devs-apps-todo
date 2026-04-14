using Shouldly;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for in-app vs push delivery, deep links, and the disable-all toggle.
/// Based on notifications.feature: in-app notification, push notification, tap-to-navigate, disable all.
/// </summary>
public sealed class NotificationDeliveryTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultToInAppChannel_When_NotificationCreated()
    {
        Notification notification = Notification.Create(NotificationType.TaskReminder, "message");
        notification.Channel.ShouldBe(DeliveryChannel.InApp);
        notification.DeepLink.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BePushChannel_When_CreatedForPushDelivery()
    {
        Notification notification = Notification.Create(
            NotificationType.TaskReminder,
            "push",
            channel: DeliveryChannel.Push);
        notification.Channel.ShouldBe(DeliveryChannel.Push);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeEmailChannel_When_CreatedForEmailDelivery()
    {
        Notification notification = Notification.Create(
            NotificationType.TaskReminder,
            "email",
            channel: DeliveryChannel.Email);
        notification.Channel.ShouldBe(DeliveryChannel.Email);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StoreDeepLink_When_Provided()
    {
        DeepLink link = DeepLink.Create("task", "abc-123");
        Notification notification = Notification.Create(
            NotificationType.TaskReminder,
            "Submit report is due today",
            deepLink: link);
        notification.DeepLink.ShouldBe(link);
        notification.DeepLink!.ToPath().ShouldBe("/task/abc-123");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DisableAllCategories_When_GlobalToggleIsOff()
    {
        NotificationSettings settings = NotificationSettings.CreateDefault().WithDisableAll(true);
        settings.DisableAll.ShouldBeTrue();

        foreach (NotificationCategory category in Enum.GetValues<NotificationCategory>())
        {
            settings.IsCategoryEnabled(category).ShouldBeFalse($"category {category} should be disabled");
        }
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReEnableCategories_When_GlobalToggleFlippedBack()
    {
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithDisableAll(true)
            .WithDisableAll(false);
        settings.DisableAll.ShouldBeFalse();
        settings.IsCategoryEnabled(NotificationCategory.TaskReminders).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveCategoryTogglesUnderDisableAll()
    {
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithCategoryToggle(NotificationCategory.TaskReminders, true)
            .WithDisableAll(true)
            .WithDisableAll(false);
        settings.IsCategoryEnabled(NotificationCategory.TaskReminders).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultDisableAllFalse_OnCreateDefault()
    {
        NotificationSettings settings = NotificationSettings.CreateDefault();
        settings.DisableAll.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultDisableAllFalse_OnCreate()
    {
        NotificationSettings settings = NotificationSettings.Create(
            new Dictionary<NotificationCategory, bool>());
        settings.DisableAll.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveDisableAll_WhenTogglingCategory()
    {
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithDisableAll(true)
            .WithCategoryToggle(NotificationCategory.TaskReminders, true);
        settings.DisableAll.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveDisableAll_WhenSettingQuietHours()
    {
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithDisableAll(true)
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));
        settings.DisableAll.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveDisableAll_WhenChangingTimezone()
    {
        NotificationSettings settings = NotificationSettings.CreateDefault()
            .WithDisableAll(true)
            .WithTimeZone("Europe/London");
        settings.DisableAll.ShouldBeTrue();
    }
}
