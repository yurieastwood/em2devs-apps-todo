using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class AuthTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_IssueToken_When_ValidCredentialsSubmitted()
    {
        // When
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "demo@waypoint.dev", password = "demo1234" });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        AuthResponse? body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.ShouldNotBeNullOrWhiteSpace();
        body.UserId.ShouldBe(new Guid("00000000-0000-0000-0000-000000000001"));
        body.DisplayName.ShouldBe("Demo User");
        body.ExpiresAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_When_EmailUnknown()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "nobody@waypoint.dev", password = "demo1234" });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_When_PasswordInvalid()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "demo@waypoint.dev", password = "wrong-password" });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_When_MeCalledWithoutToken()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/auth/me");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_ReturnCurrentUser_When_ValidTokenProvided()
    {
        // Given
        HttpResponseMessage loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "demo@waypoint.dev", password = "demo1234" });
        AuthResponse? login = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        HttpRequestMessage req = new(HttpMethod.Get, "/api/auth/me");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);

        // When
        HttpResponseMessage response = await _client.SendAsync(req);

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        MeResponse? me = await response.Content.ReadFromJsonAsync<MeResponse>();
        me!.UserId.ShouldBe(new Guid("00000000-0000-0000-0000-000000000001"));
        me.DisplayName.ShouldBe("Demo User");
        me.Email.ShouldBe("demo@waypoint.dev");
    }

    [Fact]
    public async Task Should_ReturnNoContent_When_LogoutCalled()
    {
        HttpResponseMessage response = await _client.PostAsync("/api/auth/logout", null);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Should_RegisterNewUser_When_NewEmailSubmitted()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = "fresh@waypoint.dev", password = "sekret12", displayName = "Fresh User" });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        AuthResponse? body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.DisplayName.ShouldBe("Fresh User");
        body.Token.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_ReturnConflict_When_RegisteringExistingEmail()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = "demo@waypoint.dev", password = "demo1234", displayName = "Dup" });

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    private sealed record AuthResponse(string Token, Guid UserId, string DisplayName, DateTimeOffset ExpiresAt);
    private sealed record MeResponse(Guid UserId, string DisplayName, string Email);
}
