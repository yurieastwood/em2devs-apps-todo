using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// Integration-style tests for the weekly-review endpoints. Verifies the auth gate,
/// the GET response shape, and that a POST reflection round-trips through the GET.
/// </summary>
[Trait("Category", "Api")]
public sealed class WeeklyReviewControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _authedClient;
    private readonly HttpClient _anonymousClient;

    public WeeklyReviewControllerTests()
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
    public async Task Get_Should_Return401_When_Unauthenticated()
    {
        HttpResponseMessage response = await _anonymousClient.GetAsync("/api/weekly-review");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Should_Return401_When_Unauthenticated()
    {
        HttpResponseMessage response = await _anonymousClient.PostAsJsonAsync(
            "/api/weekly-review",
            new SaveWeeklyReviewRequestDto("a", "b", "c", null));
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Should_Return200_With_NullReflection_When_NoneSaved()
    {
        HttpResponseMessage response = await _authedClient.GetAsync("/api/weekly-review");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        WeeklyReviewResponseDto? model = await response.Content.ReadFromJsonAsync<WeeklyReviewResponseDto>();
        model.ShouldNotBeNull();
        model!.Reflection.ShouldBeNull();
        model.TasksCompleted.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Post_Then_Get_Should_RoundTripReflection_ForCurrentWeek()
    {
        HttpResponseMessage save = await _authedClient.PostAsJsonAsync(
            "/api/weekly-review",
            new SaveWeeklyReviewRequestDto(
                "Shipped the slice",
                "Too many context switches",
                "Block deep work",
                null));

        save.StatusCode.ShouldBe(HttpStatusCode.OK);
        WeeklyReflectionResponseDto? saved = await save.Content.ReadFromJsonAsync<WeeklyReflectionResponseDto>();
        saved.ShouldNotBeNull();
        saved!.WhatWentWell.ShouldBe("Shipped the slice");

        HttpResponseMessage get = await _authedClient.GetAsync("/api/weekly-review");
        get.StatusCode.ShouldBe(HttpStatusCode.OK);
        WeeklyReviewResponseDto? model = await get.Content.ReadFromJsonAsync<WeeklyReviewResponseDto>();
        model.ShouldNotBeNull();
        model!.Reflection.ShouldNotBeNull();
        model.Reflection!.WhatWentWell.ShouldBe("Shipped the slice");
        model.Reflection.WhatDragged.ShouldBe("Too many context switches");
        model.Reflection.Adjustment.ShouldBe("Block deep work");
    }

    [Fact]
    public async Task Post_Should_Return400_When_ReflectionFieldIsEmpty()
    {
        HttpResponseMessage response = await _authedClient.PostAsJsonAsync(
            "/api/weekly-review",
            new SaveWeeklyReviewRequestDto("", "dragged", "adjust", null));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private sealed record SaveWeeklyReviewRequestDto(
        string WhatWentWell,
        string WhatDragged,
        string Adjustment,
        DateOnly? WeekOf);

    private sealed record WeeklyReviewResponseDto(
        DateOnly WeekOf,
        int TasksCompleted,
        int XpEarned,
        int StreakStart,
        int StreakEnd,
        IReadOnlyList<string> NotableEvents,
        WeeklyReflectionResponseDto? Reflection);

    private sealed record WeeklyReflectionResponseDto(
        string WhatWentWell,
        string WhatDragged,
        string Adjustment,
        DateTimeOffset SavedAt);
}
