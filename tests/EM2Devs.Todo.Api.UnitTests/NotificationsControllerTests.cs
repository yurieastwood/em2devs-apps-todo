using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// Inbox end-to-end: verifies per-user isolation of notifications, listing with
/// includeRead, and the mark-as-read / dismiss state transitions.
/// </summary>
[Trait("Category", "Api")]
public sealed class NotificationsControllerTests : IDisposable
{
    private static readonly Guid _userA = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid _userB = new("00000000-0000-0000-0000-000000000002");

    private readonly WebApplicationFactory<Program> _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_IsolateNotificationsPerUser()
    {
        // Given — seed one notification for each user directly into the in-memory store
        InMemoryNotificationStore store = _factory.Services.GetRequiredService<InMemoryNotificationStore>();
        Notification a = Notification.CreateForUser(_userA, NotificationType.AchievementAlert, "A's note");
        Notification b = Notification.CreateForUser(_userB, NotificationType.AchievementAlert, "B's note");
        store.Notifications[a.Id.Value] = a;
        store.Notifications[b.Id.Value] = b;

        // When — user A lists notifications
        HttpClient clientA = AuthenticatedClient(_userA);
        List<NotificationDto>? listA = await clientA.GetFromJsonAsync<List<NotificationDto>>("/api/notifications");

        // Then — user A sees only their own notification
        listA.ShouldNotBeNull();
        listA.Count.ShouldBe(1);
        listA[0].Message.ShouldBe("A's note");

        // And — user B can't mark A's notification as read
        HttpClient clientB = AuthenticatedClient(_userB);
        HttpResponseMessage crossReadResponse = await clientB.PostAsync(
            $"/api/notifications/{a.Id.Value}/read", content: null);
        crossReadResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_MarkAsRead_When_PostReadEndpointCalled()
    {
        // Given
        InMemoryNotificationStore store = _factory.Services.GetRequiredService<InMemoryNotificationStore>();
        Notification n = Notification.CreateForUser(_userA, NotificationType.AchievementAlert, "x");
        store.Notifications[n.Id.Value] = n;

        HttpClient client = AuthenticatedClient(_userA);

        // When
        HttpResponseMessage response = await client.PostAsync($"/api/notifications/{n.Id.Value}/read", null);

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        NotificationDto? dto = await response.Content.ReadFromJsonAsync<NotificationDto>();
        dto.ShouldNotBeNull();
        dto.Status.ShouldBe("Read");
        dto.ReadAt.ShouldNotBeNull();

        // And — default list (unread only) should not include it
        List<NotificationDto>? unread = await client.GetFromJsonAsync<List<NotificationDto>>("/api/notifications");
        unread.ShouldNotBeNull();
        unread.ShouldNotContain(x => x.Id == n.Id.Value);

        // But includeRead=true should
        List<NotificationDto>? all = await client.GetFromJsonAsync<List<NotificationDto>>(
            "/api/notifications?includeRead=true");
        all.ShouldNotBeNull();
        all.ShouldContain(x => x.Id == n.Id.Value);
    }

    [Fact]
    public async Task Should_HideDismissedFromAllLists_When_Dismissed()
    {
        InMemoryNotificationStore store = _factory.Services.GetRequiredService<InMemoryNotificationStore>();
        Notification n = Notification.CreateForUser(_userA, NotificationType.AchievementAlert, "y");
        store.Notifications[n.Id.Value] = n;

        HttpClient client = AuthenticatedClient(_userA);

        HttpResponseMessage response = await client.PostAsync(
            $"/api/notifications/{n.Id.Value}/dismiss", null);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        List<NotificationDto>? all = await client.GetFromJsonAsync<List<NotificationDto>>(
            "/api/notifications?includeRead=true");
        all.ShouldNotBeNull();
        all.ShouldNotContain(x => x.Id == n.Id.Value);
    }

    [Fact]
    public async Task Should_RequireAuthentication()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync("/api/notifications");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private HttpClient AuthenticatedClient(Guid userId)
    {
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTestFixture.GetTokenFor(userId));
        return client;
    }

    private sealed record NotificationDto(
        Guid Id,
        string Type,
        string Message,
        DateTimeOffset CreatedAt,
        string Status,
        DateTimeOffset? ReadAt);
}
