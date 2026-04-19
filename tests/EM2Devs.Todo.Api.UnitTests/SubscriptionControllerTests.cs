using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class SubscriptionControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SubscriptionControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnFreeSubscription_When_NewUser()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/subscription");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var sub = await response.Content.ReadFromJsonAsync<SubscriptionDto>();
        sub.ShouldNotBeNull();
        sub.Tier.ShouldBe("Free");
        sub.IsActive.ShouldBeTrue();
        sub.IsPremium.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Return401_When_Unauthenticated()
    {
        using HttpClient unauth = _factory.CreateClient();
        HttpResponseMessage response = await unauth.GetAsync("/api/subscription");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private sealed record SubscriptionDto(
        string Tier, string Status, bool IsPremium, bool IsActive,
        DateTimeOffset? ExpiresAt, bool AutoRenew);
}
