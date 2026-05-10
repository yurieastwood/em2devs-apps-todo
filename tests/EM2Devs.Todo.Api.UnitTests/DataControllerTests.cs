using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class DataControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DataControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnJsonExportEnvelope_When_AuthenticatedUserRequestsAllAsJson()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/data/export?format=json&scope=all");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        response.Content.Headers.ContentDisposition.ShouldNotBeNull();
        response.Content.Headers.ContentDisposition!.DispositionType.ShouldBe("attachment");
        response.Content.Headers.ContentDisposition.FileName.ShouldNotBeNull();
        response.Content.Headers.ContentDisposition.FileName!.ShouldStartWith("waypoint-export-");
        response.Content.Headers.ContentDisposition.FileName.ShouldEndWith(".json");

        JsonElement root = await response.Content.ReadFromJsonAsync<JsonElement>();
        string[] requiredKeys =
        [
            "meta", "tasks", "quests", "epics", "sagas",
            "xpHistory", "level", "skillTreeProgress",
            "titlesEarned", "weeklyReviews", "timelineEvents",
            "insightCards", "settings",
        ];
        foreach (string key in requiredKeys)
        {
            root.TryGetProperty(key, out _).ShouldBeTrue($"Export envelope must contain '{key}'");
        }

        JsonElement meta = root.GetProperty("meta");
        meta.GetProperty("format").GetString().ShouldBe("json");
        meta.GetProperty("scope").GetString().ShouldBe("all");
        meta.GetProperty("recordCount").GetInt32().ShouldBeGreaterThanOrEqualTo(0);
        meta.GetProperty("exportedAt").GetDateTimeOffset().ShouldBeGreaterThan(DateTimeOffset.MinValue);

        JsonElement settings = root.GetProperty("settings");
        settings.TryGetProperty("dataPrivacy", out _).ShouldBeTrue();
        settings.TryGetProperty("notifications", out _).ShouldBeTrue();
        settings.TryGetProperty("sync", out _).ShouldBeTrue();
        settings.TryGetProperty("leaderboard", out _).ShouldBeTrue();

        JsonElement level = root.GetProperty("level");
        level.GetProperty("current").GetInt32().ShouldBeGreaterThanOrEqualTo(1);
        level.TryGetProperty("xp", out _).ShouldBeTrue();
        level.TryGetProperty("longestStreak", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_IncludeCreatedTaskInExport_When_UserHasTasks()
    {
        HttpResponseMessage create = await _client.PostAsJsonAsync("/api/tasks", new { title = "Export me" });
        create.IsSuccessStatusCode.ShouldBeTrue();

        HttpResponseMessage response = await _client.GetAsync("/api/data/export?format=json&scope=all");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        JsonElement root = await response.Content.ReadFromJsonAsync<JsonElement>();
        JsonElement tasks = root.GetProperty("tasks");
        tasks.GetArrayLength().ShouldBeGreaterThan(0);

        bool foundCreatedTask = false;
        foreach (JsonElement t in tasks.EnumerateArray())
        {
            if (t.TryGetProperty("title", out JsonElement title) && title.GetString() == "Export me")
            {
                foundCreatedTask = true;
                break;
            }
        }
        foundCreatedTask.ShouldBeTrue("The newly created task must appear in the export");
    }

    [Fact]
    public async Task Should_Return401_When_Unauthenticated()
    {
        using HttpClient unauth = _factory.CreateClient();
        HttpResponseMessage response = await unauth.GetAsync("/api/data/export?format=json&scope=all");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Return400_When_FormatQueryMissing()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/data/export?scope=all");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_When_ScopeQueryMissing()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/data/export?format=json");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_When_FormatUnsupported()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/data/export?format=yaml&scope=all");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_When_ScopeIsNumericIndex()
    {
        // Guards against ASP.NET's enum binder accepting integer aliases for enum values
        // (e.g. scope=0 mapping to DataExportScope.All). Only the literal "all" is valid.
        HttpResponseMessage response = await _client.GetAsync("/api/data/export?format=json&scope=0");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_When_FormatIsNumericIndex()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/data/export?format=0&scope=all");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_RejectUnknownQueryParameter_When_ExtraParamSent()
    {
        HttpResponseMessage response = await _client.GetAsync(
            "/api/data/export?format=json&scope=all&unknownParam=42");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        string body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("unknownParam");
    }

    [Fact]
    public async Task Should_NotIncludeAnotherUsersData_When_ExportRequested()
    {
        Guid userA = AuthTestFixture.DefaultUserId;
        Guid userB = Guid.NewGuid();

        using HttpClient clientA = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthTestFixture.GetTokenFor(userA));
        using HttpClient clientB = _factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthTestFixture.GetTokenFor(userB));

        await clientB.PostAsJsonAsync("/api/tasks", new { title = "User B private task" });

        HttpResponseMessage response = await clientA.GetAsync("/api/data/export?format=json&scope=all");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        JsonElement root = await response.Content.ReadFromJsonAsync<JsonElement>();
        JsonElement tasks = root.GetProperty("tasks");
        foreach (JsonElement t in tasks.EnumerateArray())
        {
            if (t.TryGetProperty("title", out JsonElement title))
            {
                title.GetString().ShouldNotBe("User B private task");
            }
        }
    }
}
