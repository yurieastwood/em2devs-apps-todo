using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class EnergyControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EnergyControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_RecordEnergyLevel_When_CheckingIn()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/energy/check-in", new { level = "High" });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CheckInResult>();
        result!.Level.ShouldBe("High");
        result.IsUpdate.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_UpdateLevel_When_CheckingInTwiceSameDay()
    {
        await _client.PostAsJsonAsync("/api/energy/check-in", new { level = "High" });
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/energy/check-in", new { level = "Low" });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CheckInResult>();
        result!.Level.ShouldBe("Low");
        result.IsUpdate.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_InvalidLevel()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/energy/check-in", new { level = "SuperHigh" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_ReturnProfile_When_Authenticated()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/energy/profile");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("hasSufficientData");
        json.ShouldContain("confidenceLevel");
    }

    [Fact]
    public async Task Should_Return401_When_Unauthenticated()
    {
        using HttpClient unauth = _factory.CreateClient();
        HttpResponseMessage response = await unauth.GetAsync("/api/energy/profile");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private sealed record CheckInResult(string Level, bool IsUpdate, bool HasFluctuated);
}
