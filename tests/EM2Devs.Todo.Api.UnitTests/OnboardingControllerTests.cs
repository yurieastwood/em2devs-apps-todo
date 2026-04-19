using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class OnboardingControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public OnboardingControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnOnboardingState_When_Authenticated()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/onboarding");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("\"unlockedFeatures\"");
        json.ShouldContain("\"isGamificationActive\"");
        json.ShouldContain("\"upcomingFeatures\"");
    }

    [Fact]
    public async Task Should_Return401_When_Unauthenticated()
    {
        using HttpClient unauth = _factory.CreateClient();
        HttpResponseMessage response = await unauth.GetAsync("/api/onboarding");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
