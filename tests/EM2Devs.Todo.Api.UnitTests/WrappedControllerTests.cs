using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class WrappedControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WrappedControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnWrapped_When_Authenticated()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/wrapped");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("\"year\"");
        json.ShouldContain("\"slides\"");
    }

    [Fact]
    public async Task Should_ReturnWrappedForSpecificYear_When_YearProvided()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/wrapped?year=2025");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("2025");
    }

    [Fact]
    public async Task Should_Return401_When_Unauthenticated()
    {
        using HttpClient unauth = _factory.CreateClient();
        HttpResponseMessage response = await unauth.GetAsync("/api/wrapped");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
