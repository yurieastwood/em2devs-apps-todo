using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class TimelineControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TimelineControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnEmptyTimeline_When_NewUser()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/timeline");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var timeline = await response.Content.ReadFromJsonAsync<TimelineResponse>();
        timeline.ShouldNotBeNull();
        timeline.Events.ShouldBeEmpty();
        timeline.HasMore.ShouldBeFalse();
        timeline.NextCursor.ShouldBeNull();
    }

    [Fact]
    public async Task Should_RejectUnknownQueryParameter_When_ExtraParamSent()
    {
        HttpResponseMessage response = await _client.GetAsync(
            "/api/timeline?pageSize=20&unknownParam=42");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        string body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("unknownParam");
    }

    [Fact]
    public async Task Should_Return401_When_Unauthenticated()
    {
        using HttpClient unauth = _factory.CreateClient();
        HttpResponseMessage response = await unauth.GetAsync("/api/timeline");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_ReturnTimelineEvents_When_TaskCompletedTriggersLevelUp()
    {
        for (int i = 0; i < 3; i++)
        {
            var createResponse = await _client.PostAsJsonAsync("/api/tasks", new { title = $"Timeline task {i}" });
            var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>();
            await _client.PatchAsJsonAsync($"/api/tasks/{created!.Id}/status", new { status = "Completed" });
        }

        HttpResponseMessage response = await _client.GetAsync("/api/timeline");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var timeline = await response.Content.ReadFromJsonAsync<TimelineResponse>();
        timeline.ShouldNotBeNull();
    }

    private sealed record TaskDto(Guid Id);
    private sealed record TimelineResponse(
        IReadOnlyList<TimelineEventDto> Events,
        bool HasMore,
        Guid? NextCursor);
    private sealed record TimelineEventDto(
        Guid Id, string EventType, DateTimeOffset OccurredAt, string Details, string? Note);
}
