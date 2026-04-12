using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
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
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnUser_When_LoginCalled()
    {
        // When
        HttpResponseMessage response = await _client.PostAsync("/api/auth/login", null);

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        AuthResponse? user = await response.Content.ReadFromJsonAsync<AuthResponse>();
        user!.UserId.ShouldBe(new Guid("00000000-0000-0000-0000-000000000001"));
        user.DisplayName.ShouldBe("Demo User");
    }

    [Fact]
    public async Task Should_SetCookie_When_LoginCalled()
    {
        // When
        HttpResponseMessage response = await _client.PostAsync("/api/auth/login", null);

        // Then
        response.Headers.TryGetValues("Set-Cookie", out System.Collections.Generic.IEnumerable<string>? cookies).ShouldBeTrue();
        string cookieHeader = string.Join("; ", cookies!);
        cookieHeader.ShouldContain("demo-user=true");
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NotLoggedIn()
    {
        // When
        HttpResponseMessage response = await _client.GetAsync("/api/auth/me");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_ReturnCurrentUser_When_LoggedIn()
    {
        // Given
        await _client.PostAsync("/api/auth/login", null);

        // When
        HttpResponseMessage response = await _client.GetAsync("/api/auth/me");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        AuthResponse? user = await response.Content.ReadFromJsonAsync<AuthResponse>();
        user!.DisplayName.ShouldBe("Demo User");
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_When_LoggedOut()
    {
        // Given
        await _client.PostAsync("/api/auth/login", null);
        await _client.PostAsync("/api/auth/logout", null);

        // When
        HttpResponseMessage response = await _client.GetAsync("/api/auth/me");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_ReturnNoContent_When_LogoutCalled()
    {
        // When
        HttpResponseMessage response = await _client.PostAsync("/api/auth/logout", null);

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    private sealed record AuthResponse(Guid UserId, string DisplayName);
}
