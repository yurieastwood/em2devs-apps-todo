using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// Scenario-driven tests for GET /api/daily-brief.
/// Verifies the auth gate and the response shape match the contract.
/// </summary>
[Trait("Category", "Api")]
public sealed class DailyBriefControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _authedClient;
    private readonly HttpClient _anonymousClient;

    public DailyBriefControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _authedClient = _factory.CreateClient().Authenticated();
        _anonymousClient = _factory.CreateClient();
    }

    public void Dispose()
    {
        _authedClient.Dispose();
        _anonymousClient.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Should_Return401_When_Unauthenticated()
    {
        HttpResponseMessage response = await _anonymousClient.GetAsync("/api/daily-brief");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Return200WithCorrectShape_When_Authenticated()
    {
        HttpResponseMessage response = await _authedClient.GetAsync("/api/daily-brief");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        DailyBriefResponse? brief = await response.Content.ReadFromJsonAsync<DailyBriefResponse>();
        brief.ShouldNotBeNull();
        brief.Greeting.ShouldNotBeNullOrWhiteSpace();
        brief.CorePlan.ShouldNotBeNull();
        brief.IfTimeAllows.ShouldNotBeNull();
        brief.Overdue.ShouldNotBeNull();
        // No tasks seeded for this user → InsufficientTasks.
        brief.Status.ShouldBe("InsufficientTasks");
        brief.CorePlanCount.ShouldBe(0);
        brief.OverdueCount.ShouldBe(0);
    }

    private sealed record DailyBriefResponse(
        DateOnly Date,
        string Greeting,
        int CurrentStreakDays,
        int CorePlanCount,
        int IfTimeAllowsCount,
        int OverdueCount,
        IReadOnlyList<DailyBriefTaskResponse> CorePlan,
        IReadOnlyList<DailyBriefTaskResponse> IfTimeAllows,
        IReadOnlyList<DailyBriefTaskResponse> Overdue,
        string Status);

    private sealed record DailyBriefTaskResponse(
        Guid Id,
        string Title,
        string Difficulty,
        string Priority,
        int? EstimatedMinutes,
        DateOnly? ScheduledDate);
}
