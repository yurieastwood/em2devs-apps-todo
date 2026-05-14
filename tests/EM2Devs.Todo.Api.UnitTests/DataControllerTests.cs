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
    public async Task Should_AllowBothExportFormats_When_UserIsFreeTier()
    {
        // Regression lock for the "export available regardless of subscription" scenario:
        // a free-tier user must be able to download both the full JSON envelope and the
        // tasks CSV without tier gating. The seeded demo user is Free by default.
        HttpResponseMessage subscription = await _client.GetAsync("/api/subscription");
        subscription.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonElement sub = await subscription.Content.ReadFromJsonAsync<JsonElement>();
        sub.GetProperty("tier").GetString().ShouldBe("Free", "demo user is Free tier by default");
        sub.GetProperty("isPremium").GetBoolean().ShouldBeFalse();

        HttpResponseMessage json = await _client.GetAsync("/api/data/export?format=json&scope=all");
        json.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage csv = await _client.GetAsync("/api/data/export?format=csv&scope=tasksOnly");
        csv.StatusCode.ShouldBe(HttpStatusCode.OK);
        csv.Content.Headers.ContentType!.MediaType.ShouldBe("text/csv");
    }

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
    public async Task Should_ReturnCsv_When_FormatCsvAndScopeTasksOnly()
    {
        HttpResponseMessage create = await _client.PostAsJsonAsync("/api/tasks", new { title = "Buy milk" });
        create.IsSuccessStatusCode.ShouldBeTrue();

        HttpResponseMessage response = await _client.GetAsync("/api/data/export?format=csv&scope=tasksOnly");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType.ShouldNotBeNull();
        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/csv");
        response.Content.Headers.ContentDisposition.ShouldNotBeNull();
        response.Content.Headers.ContentDisposition!.DispositionType.ShouldBe("attachment");
        response.Content.Headers.ContentDisposition.FileName.ShouldNotBeNull();
        response.Content.Headers.ContentDisposition.FileName!.ShouldStartWith("waypoint-tasks-");
        response.Content.Headers.ContentDisposition.FileName.ShouldEndWith(".csv");

        string csv = await response.Content.ReadAsStringAsync();
        string[] lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBeGreaterThan(1, "CSV must include header plus at least one row");
        lines[0].Trim().ShouldBe("id,title,description,status,difficulty,priority,baseXp,tags,dueDate,scheduledDate,completedAt,createdAt,assignedQuestId");

        bool hasOurRow = lines.Skip(1).Any(l => l.Contains("Buy milk", StringComparison.Ordinal));
        hasOurRow.ShouldBeTrue("Created task must appear as a data row");
    }

    [Fact]
    public async Task Should_QuoteFields_When_TaskContainsCommasOrQuotesOrNewlines()
    {
        await _client.PostAsJsonAsync("/api/tasks", new { title = "Hello, world" });

        HttpResponseMessage withQuotes = await _client.PostAsJsonAsync("/api/tasks", new { title = "Quoted task" });
        Guid quotedId = (await withQuotes.Content.ReadFromJsonAsync<TaskIdResponse>())!.Id;
        await _client.PatchAsJsonAsync($"/api/tasks/{quotedId}", new { description = "She said \"hi\"" });

        HttpResponseMessage withNewline = await _client.PostAsJsonAsync("/api/tasks", new { title = "Multiline task" });
        Guid multilineId = (await withNewline.Content.ReadFromJsonAsync<TaskIdResponse>())!.Id;
        await _client.PatchAsJsonAsync($"/api/tasks/{multilineId}", new { description = "Line1\nLine2" });

        HttpResponseMessage response = await _client.GetAsync("/api/data/export?format=csv&scope=tasksOnly");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string csv = await response.Content.ReadAsStringAsync();

        // RFC 4180: fields with commas, quotes, or newlines are double-quoted;
        // embedded quotes are doubled.
        csv.ShouldContain("\"Hello, world\"");
        csv.ShouldContain("\"She said \"\"hi\"\"\"");
        csv.ShouldContain("\"Line1\nLine2\"");
    }

    private sealed record TaskIdResponse(Guid Id);

    [Fact]
    public async Task Should_RestoreData_When_ExportIsRoundTripped()
    {
        await _client.PostAsJsonAsync("/api/tasks", new { title = "Round-trip me" });

        HttpResponseMessage exportResp = await _client.GetAsync("/api/data/export?format=json&scope=all");
        exportResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonElement envelope = await exportResp.Content.ReadFromJsonAsync<JsonElement>();
        envelope.GetProperty("tasks").GetArrayLength().ShouldBeGreaterThan(0);

        // Wipe state via the delete endpoint
        HttpResponseMessage del = await _client.PostAsJsonAsync(
            "/api/data/delete", new { confirmation = "DELETE MY DATA" });
        del.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Confirm tasks are gone
        HttpResponseMessage after = await _client.GetAsync("/api/tasks");
        JsonElement listAfter = await after.Content.ReadFromJsonAsync<JsonElement>();
        listAfter.GetArrayLength().ShouldBe(0);

        // Import the envelope back
        using var content = new StringContent(envelope.GetRawText(), System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage imp = await _client.PostAsync("/api/data/import", content);
        imp.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonElement importResp = await imp.Content.ReadFromJsonAsync<JsonElement>();
        importResp.GetProperty("recordsImported").GetInt32().ShouldBeGreaterThan(0);

        // Tasks should be back
        HttpResponseMessage restored = await _client.GetAsync("/api/tasks");
        JsonElement listRestored = await restored.Content.ReadFromJsonAsync<JsonElement>();
        listRestored.GetArrayLength().ShouldBeGreaterThan(0);
        bool foundTitle = false;
        foreach (JsonElement t in listRestored.EnumerateArray())
        {
            if (t.TryGetProperty("title", out JsonElement title) && title.GetString() == "Round-trip me")
            {
                foundTitle = true;
                break;
            }
        }
        foundTitle.ShouldBeTrue("Imported task should reappear in the task list");
    }

    [Fact]
    public async Task Should_Return400_When_ImportBodyMissing()
    {
        using var emptyContent = new StringContent("", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync("/api/data/import", emptyContent);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return401_When_ImportUnauthenticated()
    {
        using HttpClient unauth = _factory.CreateClient();
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await unauth.PostAsync("/api/data/import", content);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Return204_When_DeleteAllDataConfirmed()
    {
        await _client.PostAsJsonAsync("/api/tasks", new { title = "soon to be deleted" });

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/data/delete", new { confirmation = "DELETE MY DATA" });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Should_RemoveUserTasksAndNotifications_When_DeleteConfirmed()
    {
        await _client.PostAsJsonAsync("/api/tasks", new { title = "task A" });
        await _client.PostAsJsonAsync("/api/tasks", new { title = "task B" });

        HttpResponseMessage tasksBefore = await _client.GetAsync("/api/tasks");
        JsonElement listBefore = await tasksBefore.Content.ReadFromJsonAsync<JsonElement>();
        listBefore.GetArrayLength().ShouldBeGreaterThan(0);

        HttpResponseMessage del = await _client.PostAsJsonAsync(
            "/api/data/delete", new { confirmation = "DELETE MY DATA" });
        del.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage tasksAfter = await _client.GetAsync("/api/tasks");
        JsonElement listAfter = await tasksAfter.Content.ReadFromJsonAsync<JsonElement>();
        listAfter.GetArrayLength().ShouldBe(0);
    }

    [Fact]
    public async Task Should_KeepAccountActive_When_DeleteConfirmed()
    {
        await _client.PostAsJsonAsync("/api/tasks", new { title = "before purge" });

        await _client.PostAsJsonAsync(
            "/api/data/delete", new { confirmation = "DELETE MY DATA" });

        // /api/auth/me should still succeed — the User row is preserved.
        HttpResponseMessage me = await _client.GetAsync("/api/auth/me");
        me.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Return400_When_ConfirmationMissing()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/data/delete", new { });
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_When_ConfirmationWrong()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/data/delete", new { confirmation = "delete my data" });
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return401_When_DeleteUnauthenticated()
    {
        using HttpClient unauth = _factory.CreateClient();
        HttpResponseMessage response = await unauth.PostAsJsonAsync(
            "/api/data/delete", new { confirmation = "DELETE MY DATA" });
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_NotDeleteOtherUsersData_When_OneUserPurges()
    {
        Guid userA = AuthTestFixture.DefaultUserId;
        Guid userB = Guid.NewGuid();

        using HttpClient clientA = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthTestFixture.GetTokenFor(userA));
        using HttpClient clientB = _factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthTestFixture.GetTokenFor(userB));

        await clientB.PostAsJsonAsync("/api/tasks", new { title = "B's task — must survive" });

        await clientA.PostAsJsonAsync(
            "/api/data/delete", new { confirmation = "DELETE MY DATA" });

        HttpResponseMessage bTasks = await clientB.GetAsync("/api/tasks");
        bTasks.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonElement bList = await bTasks.Content.ReadFromJsonAsync<JsonElement>();
        bList.GetArrayLength().ShouldBe(1);
    }

    [Fact]
    public async Task Should_Return400_When_CsvFormatPairedWithAllScope()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/data/export?format=csv&scope=all");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_When_JsonFormatPairedWithTasksOnlyScope()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/data/export?format=json&scope=tasksOnly");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
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

    [Fact]
    public async Task Should_AcceptImport_When_LevelFieldsAreAbsent()
    {
        // The OpenAPI contract documents level fields as optional: absence is treated as
        // the starting-state defaults (1, 0, 0). An envelope with `level: {}` is therefore
        // schema-compliant and must be accepted.
        string envelope = """
            {
              "meta": {"exportedAt": "2026-05-13T00:00:00Z", "format": "json", "scope": "all", "recordCount": 0},
              "tasks": [], "quests": [], "epics": [], "sagas": [],
              "xpHistory": [], "level": {}, "skillTreeProgress": [], "titlesEarned": [],
              "weeklyReviews": [], "timelineEvents": [], "insightCards": [],
              "settings": {"dataPrivacy": null, "notifications": null, "sync": null, "leaderboard": null}
            }
            """;
        using var content = new StringContent(envelope, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync("/api/data/import", content);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Return400_When_ImportLevelCurrentIsBelowOne()
    {
        // Explicit out-of-range values stay rejected; only absence is forgiven.
        string envelope = """
            {
              "meta": {"exportedAt": "2026-05-13T00:00:00Z", "format": "json", "scope": "all", "recordCount": 0},
              "tasks": [], "quests": [], "epics": [], "sagas": [],
              "xpHistory": [], "level": {"current": 0}, "skillTreeProgress": [], "titlesEarned": [],
              "weeklyReviews": [], "timelineEvents": [], "insightCards": [],
              "settings": {"dataPrivacy": null, "notifications": null, "sync": null, "leaderboard": null}
            }
            """;
        using var content = new StringContent(envelope, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync("/api/data/import", content);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_When_ImportLevelLongestStreakIsNegative()
    {
        string envelope = """
            {
              "meta": {"exportedAt": "2026-05-13T00:00:00Z", "format": "json", "scope": "all", "recordCount": 0},
              "tasks": [], "quests": [], "epics": [], "sagas": [],
              "xpHistory": [], "level": {"longestStreak": -1}, "skillTreeProgress": [], "titlesEarned": [],
              "weeklyReviews": [], "timelineEvents": [], "insightCards": [],
              "settings": {"dataPrivacy": null, "notifications": null, "sync": null, "leaderboard": null}
            }
            """;
        using var content = new StringContent(envelope, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync("/api/data/import", content);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_ApplyDefaultLevelOnRoundTrip_When_LevelFieldsAreAbsent()
    {
        // Round-trip: import envelope with `level: {}`, then export — the export must report
        // current=1, xp=0, longestStreak=0 (the documented starting-state defaults).
        string envelope = """
            {
              "meta": {"exportedAt": "2026-05-13T00:00:00Z", "format": "json", "scope": "all", "recordCount": 0},
              "tasks": [], "quests": [], "epics": [], "sagas": [],
              "xpHistory": [], "level": {}, "skillTreeProgress": [], "titlesEarned": [],
              "weeklyReviews": [], "timelineEvents": [], "insightCards": [],
              "settings": {"dataPrivacy": null, "notifications": null, "sync": null, "leaderboard": null}
            }
            """;
        using var content = new StringContent(envelope, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage imp = await _client.PostAsync("/api/data/import", content);
        imp.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage exp = await _client.GetAsync("/api/data/export?format=json&scope=all");
        exp.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonElement root = await exp.Content.ReadFromJsonAsync<JsonElement>();
        JsonElement level = root.GetProperty("level");
        level.GetProperty("current").GetInt32().ShouldBe(1);
        level.GetProperty("xp").GetInt32().ShouldBe(0);
        level.GetProperty("longestStreak").GetInt32().ShouldBe(0);
    }
}
