using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class AccountControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AccountControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_Return204_When_AccountDeleteConfirmed()
    {
        await _client.PostAsJsonAsync("/api/tasks", new { title = "owned task" });

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/account/delete", new { confirmation = "DELETE MY ACCOUNT" });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Should_PurgeUserData_When_AccountDeleted()
    {
        await _client.PostAsJsonAsync("/api/tasks", new { title = "doomed task" });

        HttpResponseMessage del = await _client.PostAsJsonAsync(
            "/api/account/delete", new { confirmation = "DELETE MY ACCOUNT" });
        del.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Same token (still valid for 1h) — but data should be empty.
        HttpResponseMessage tasks = await _client.GetAsync("/api/tasks");
        tasks.StatusCode.ShouldBe(HttpStatusCode.OK);
        System.Text.Json.JsonElement list = await tasks.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        list.GetArrayLength().ShouldBe(0);
    }

    [Fact]
    public async Task Should_RejectLogin_When_AccountDeactivated()
    {
        // Demo seeded user — login works first
        HttpResponseMessage login1 = await _factory.CreateClient().PostAsJsonAsync(
            "/api/auth/login",
            new { email = "demo@waypoint.dev", password = "demo1234" });
        login1.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Authenticate as the demo user, delete the account
        HttpResponseMessage del = await _client.PostAsJsonAsync(
            "/api/account/delete", new { confirmation = "DELETE MY ACCOUNT" });
        del.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Login should now fail
        HttpResponseMessage login2 = await _factory.CreateClient().PostAsJsonAsync(
            "/api/auth/login",
            new { email = "demo@waypoint.dev", password = "demo1234" });
        login2.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Return400_When_AccountDeleteConfirmationMissing()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/account/delete", new { });
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_When_AccountDeleteConfirmationWrong()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/account/delete", new { confirmation = "DELETE MY DATA" });
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return401_When_AccountDeleteUnauthenticated()
    {
        using HttpClient unauth = _factory.CreateClient();
        HttpResponseMessage response = await unauth.PostAsJsonAsync(
            "/api/account/delete", new { confirmation = "DELETE MY ACCOUNT" });
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_BeIdempotent_When_DeleteCalledTwice()
    {
        HttpResponseMessage first = await _client.PostAsJsonAsync(
            "/api/account/delete", new { confirmation = "DELETE MY ACCOUNT" });
        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Same JWT still valid for the natural 1-hour expiry — a second call should not
        // 500 by tripping User.Deactivate's "already deactivated" invariant.
        HttpResponseMessage second = await _client.PostAsJsonAsync(
            "/api/account/delete", new { confirmation = "DELETE MY ACCOUNT" });
        second.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Should_NotAffectOtherUsers_When_OneAccountDeleted()
    {
        Guid userA = AuthTestFixture.DefaultUserId;

        using HttpClient clientA = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthTestFixture.GetTokenFor(userA));
        using HttpClient clientB = _factory.CreateClient();
        // The second seeded demo user (demo2)
        Guid demo2 = new("00000000-0000-0000-0000-000000000002");
        clientB.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthTestFixture.GetTokenFor(demo2));

        await clientB.PostAsJsonAsync("/api/tasks", new { title = "user B's task" });

        HttpResponseMessage del = await clientA.PostAsJsonAsync(
            "/api/account/delete", new { confirmation = "DELETE MY ACCOUNT" });
        del.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // User B's login still works
        HttpResponseMessage bLogin = await _factory.CreateClient().PostAsJsonAsync(
            "/api/auth/login",
            new { email = "demo2@waypoint.dev", password = "demo1234" });
        bLogin.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage bTasks = await clientB.GetAsync("/api/tasks");
        bTasks.StatusCode.ShouldBe(HttpStatusCode.OK);
        System.Text.Json.JsonElement bList = await bTasks.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        bList.GetArrayLength().ShouldBe(1);
    }
}
