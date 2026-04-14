using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// Scenario-driven API tests for the profile endpoint.
/// Verifies GET /api/profile returns progression data.
/// </summary>
[Trait("Category", "Api")]
public sealed class ProfileControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProfileControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnDefaultProfile_When_NewUser()
    {
        // Given / When
        HttpResponseMessage response = await _client.GetAsync("/api/profile");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>();
        profile.ShouldNotBeNull();
        profile.TotalXp.ShouldBe(0);
        profile.Level.ShouldBe(1);
        profile.XpToNextLevel.ShouldBe(50);
        profile.CurrentStreak.ShouldBe(0);
        profile.LongestStreak.ShouldBe(0);
        profile.XpHistory.ShouldNotBeNull();
        profile.XpHistory.ShouldBeEmpty();
        profile.Titles.ShouldNotBeNull();
        profile.Titles!.Earned.ShouldBeEmpty();
        profile.Titles.Active.ShouldBeNull();
        profile.Titles.Progress.ShouldBeEmpty();
        profile.SkillTrees.ShouldNotBeNull();
        // All seven skill tree types appear as locked entries by default.
        profile.SkillTrees.Count.ShouldBe(7);
        profile.SkillTrees.ShouldAllBe(t => t.Tier == null && !string.IsNullOrEmpty(t.UnlockHint));
    }

    [Fact]
    public async Task Should_ReturnJsonContentType_When_ProfileRequested()
    {
        // Given / When
        HttpResponseMessage response = await _client.GetAsync("/api/profile");

        // Then
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }

    private sealed record ProfileResponse(
        int TotalXp,
        int Level,
        int XpToNextLevel,
        int CurrentStreak,
        int LongestStreak,
        IReadOnlyList<XpHistoryEntryResponse> XpHistory,
        TitlesResponse Titles,
        IReadOnlyList<SkillTreeResponse> SkillTrees);

    private sealed record XpHistoryEntryResponse(
        DateOnly Date,
        int XpEarned,
        string Source,
        int CumulativeTotal);

    private sealed record TitleResponse(string Type, string DisplayName, DateOnly EarnedOn);

    private sealed record TitleProgressResponse(string Type, int ProgressPercentage, string RemainingDescription);

    private sealed record TitlesResponse(
        IReadOnlyList<TitleResponse> Earned,
        string? Active,
        IReadOnlyList<TitleProgressResponse> Progress);

    private sealed record SkillTreePerkResponse(int Tier, string PerkType, string Description);

    private sealed record SkillTreeResponse(
        string Type,
        int? Tier,
        int? TasksCompletedInTier,
        int? TasksToNextTier,
        string? UnlockHint,
        IReadOnlyList<SkillTreePerkResponse> Perks);
}
