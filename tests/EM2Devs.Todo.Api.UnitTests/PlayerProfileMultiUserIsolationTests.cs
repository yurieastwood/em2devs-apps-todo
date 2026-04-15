using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// Slice 3 multi-user data isolation: verify that one user's XP earnings and
/// progression state do not bleed into another user's profile. User A completes
/// a task and earns XP; user B's profile must still be pristine (level 1, 0 XP,
/// empty xp history).
/// </summary>
[Trait("Category", "Api")]
public sealed class PlayerProfileMultiUserIsolationTests : IDisposable
{
    private static readonly Guid _userA = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid _userB = new("00000000-0000-0000-0000-000000000002");

    private readonly WebApplicationFactory<Program> _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ScopeXpAndProgressionPerUser_When_OneUserCompletesTaskAndAnotherReadsProfile()
    {
        // Given — user A creates and completes a task, earning XP
        HttpClient clientA = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTestFixture.GetTokenFor(_userA));

        HttpResponseMessage created = await clientA.PostAsJsonAsync(
            "/api/tasks", new { title = "earn xp" });
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        TaskResponseDto? task = await created.Content.ReadFromJsonAsync<TaskResponseDto>();
        task.ShouldNotBeNull();

        HttpResponseMessage moveToInProgress = await clientA.PatchAsJsonAsync(
            $"/api/tasks/{task.Id}/status", new { status = "InProgress" });
        moveToInProgress.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage complete = await clientA.PatchAsJsonAsync(
            $"/api/tasks/{task.Id}/status", new { status = "Done" });
        complete.StatusCode.ShouldBe(HttpStatusCode.OK);

        // When — user A reads their profile
        HttpResponseMessage profileAResponse = await clientA.GetAsync("/api/profile");
        profileAResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        ProfileReadDto? profileA = await profileAResponse.Content.ReadFromJsonAsync<ProfileReadDto>();
        profileA.ShouldNotBeNull();

        // Then — A has earned XP and at least one xp history entry
        profileA.TotalXp.ShouldBeGreaterThan(0);
        profileA.XpHistory.ShouldNotBeNull();
        profileA.XpHistory.Count.ShouldBeGreaterThanOrEqualTo(1);

        // And — user B's profile is pristine
        HttpClient clientB = _factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTestFixture.GetTokenFor(_userB));

        HttpResponseMessage profileBResponse = await clientB.GetAsync("/api/profile");
        profileBResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        ProfileReadDto? profileB = await profileBResponse.Content.ReadFromJsonAsync<ProfileReadDto>();
        profileB.ShouldNotBeNull();

        profileB.Level.ShouldBe(1);
        profileB.TotalXp.ShouldBe(0);
        profileB.CurrentStreak.ShouldBe(0);
        profileB.XpHistory.ShouldNotBeNull();
        profileB.XpHistory.ShouldBeEmpty();
    }

    private sealed record ProfileReadDto(
        int TotalXp,
        int Level,
        int XpToNextLevel,
        int CurrentStreak,
        int LongestStreak,
        List<XpHistoryEntryDto> XpHistory);

    private sealed record XpHistoryEntryDto(
        DateOnly Date,
        int XpEarned,
        string Source,
        int CumulativeTotal);
}
