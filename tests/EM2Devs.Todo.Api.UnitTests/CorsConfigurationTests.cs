using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class CorsConfigurationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CorsConfigurationTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnCorsHeaders_When_RequestIncludesAllowedOrigin()
    {
        // Given
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/tasks");
        request.Headers.Add("Origin", "http://localhost:5173");

        // When
        var response = await _client.SendAsync(request);

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.Contains("Access-Control-Allow-Origin").ShouldBeTrue();
        response.Headers.GetValues("Access-Control-Allow-Origin")
            .ShouldContain("http://localhost:5173");
        response.Headers.Contains("Access-Control-Allow-Credentials").ShouldBeTrue();
    }

    [Fact]
    public async Task Should_NotReturnCorsHeaders_When_OriginIsNotAllowed()
    {
        // Given
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/tasks");
        request.Headers.Add("Origin", "http://malicious-site.com");

        // When
        var response = await _client.SendAsync(request);

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.Contains("Access-Control-Allow-Origin").ShouldBeFalse();
    }

    [Fact]
    public async Task Should_ReturnPreflightHeaders_When_PreflightRequestSent()
    {
        // Given
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/tasks");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

        // When
        var response = await _client.SendAsync(request);

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.Contains("Access-Control-Allow-Origin").ShouldBeTrue();
        response.Headers.GetValues("Access-Control-Allow-Origin")
            .ShouldContain("http://localhost:5173");
        response.Headers.Contains("Access-Control-Allow-Methods").ShouldBeTrue();
        response.Headers.GetValues("Access-Control-Allow-Methods")
            .ShouldContain("POST");
        response.Headers.Contains("Access-Control-Allow-Headers").ShouldBeTrue();
        response.Headers.GetValues("Access-Control-Allow-Headers")
            .ShouldContain("Content-Type");
        response.Headers.Contains("Access-Control-Allow-Credentials").ShouldBeTrue();
    }
}
