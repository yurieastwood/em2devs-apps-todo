using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// API-level tests for the quick-add endpoint. Verifies the raw string is parsed
/// by QuickAddParser and the resulting task is created with the parsed title,
/// tags, priority and scheduled date.
/// </summary>
[Trait("Category", "Api")]
public sealed class QuickAddTaskTests : IDisposable
{
    private static readonly string[] _expectedTags = ["groceries"];

    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public QuickAddTaskTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_CreateTask_When_OnlyTitleProvided()
    {
        // When
        var response = await _client.PostAsJsonAsync(
            "/api/tasks/quick-add",
            new { input = "write blog post" });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var task = await response.Content.ReadFromJsonAsync<QuickAddResponseDto>();
        task!.Title.ShouldBe("write blog post");
        task.Tags.ShouldBeEmpty();
        task.ScheduledDate.ShouldBeNull();
    }

    [Fact]
    public async Task Should_ParseDirectives_When_InputHasTagPriorityAndDate()
    {
        // When
        var response = await _client.PostAsJsonAsync(
            "/api/tasks/quick-add",
            new { input = "buy milk #groceries !High ^tomorrow" });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var task = await response.Content.ReadFromJsonAsync<QuickAddResponseDto>();
        task!.Title.ShouldBe("buy milk");
        task.Tags.ShouldBe(_expectedTags);
        task.Priority.ShouldBe("High");
        task.ScheduledDate.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_InputIsEmpty()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks/quick-add", new { input = "" });
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_InputExceedsMaxLength()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/tasks/quick-add",
            new { input = new string('x', 501) });
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NoToken()
    {
        HttpClient anon = _factory.CreateClient();
        var response = await anon.PostAsJsonAsync("/api/tasks/quick-add", new { input = "hi" });
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private sealed record QuickAddResponseDto(
        Guid Id,
        string Title,
        string Status,
        string Priority,
        DateOnly? ScheduledDate,
        string[] Tags);
}
