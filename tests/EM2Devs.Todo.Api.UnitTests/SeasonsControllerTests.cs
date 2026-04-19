using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class SeasonsControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SeasonsControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnCurrentSeason_When_Authenticated()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/seasons/current");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("\"name\"");
        json.ShouldContain("\"theme\"");
        json.ShouldContain("\"daysRemaining\"");
        json.ShouldContain("\"questLine\"");
    }

    [Fact]
    public async Task Should_Return401_When_Unauthenticated()
    {
        using HttpClient unauth = _factory.CreateClient();
        HttpResponseMessage response = await unauth.GetAsync("/api/seasons/current");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
