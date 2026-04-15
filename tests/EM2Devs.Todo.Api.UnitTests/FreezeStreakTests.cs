using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// Scenario-driven API tests for POST /api/profile/streak/freeze.
/// Verifies authenticated success, double-freeze conflict, and validation bounds.
/// </summary>
[Trait("Category", "Api")]
public sealed class FreezeStreakTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public FreezeStreakTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_Return401_When_Unauthenticated()
    {
        // Given — anonymous client
        using HttpClient anon = _factory.CreateClient();

        // When
        HttpResponseMessage response = await anon.PostAsJsonAsync(
            "/api/profile/streak/freeze", new { days = 7 });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_FreezeStreakAndReturnUpdatedProfile_When_Authenticated()
    {
        // Given / When
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/profile/streak/freeze", new { days = 7 });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<ProfileShape>();
        profile.ShouldNotBeNull();
        profile.StreakFreeze.ShouldNotBeNull();
        profile.StreakFreeze!.Days.ShouldBe(7);
        profile.StreakFreeze.ExpiresAt.ShouldBe(profile.StreakFreeze.FrozenAt.AddDays(7));
    }

    [Fact]
    public async Task Should_Return409_When_StreakAlreadyFrozen()
    {
        // Given — first freeze succeeds
        HttpResponseMessage first = await _client.PostAsJsonAsync(
            "/api/profile/streak/freeze", new { days = 3 });
        first.StatusCode.ShouldBe(HttpStatusCode.OK);

        // When — second freeze on same profile
        HttpResponseMessage second = await _client.PostAsJsonAsync(
            "/api/profile/streak/freeze", new { days = 3 });

        // Then
        second.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_Return400_When_DaysOutOfRange()
    {
        // When
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/profile/streak/freeze", new { days = 100 });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_IncludeStreakFreezeOnProfileAfterFreeze()
    {
        // Given
        await _client.PostAsJsonAsync("/api/profile/streak/freeze", new { days = 5 });

        // When — subsequent GET /api/profile
        HttpResponseMessage response = await _client.GetAsync("/api/profile");

        // Then
        var profile = await response.Content.ReadFromJsonAsync<ProfileShape>();
        profile.ShouldNotBeNull();
        profile.StreakFreeze.ShouldNotBeNull();
        profile.StreakFreeze!.Days.ShouldBe(5);
    }

    private sealed record ProfileShape(StreakFreezeShape? StreakFreeze);

    private sealed record StreakFreezeShape(DateOnly FrozenAt, int Days, DateOnly ExpiresAt);
}
