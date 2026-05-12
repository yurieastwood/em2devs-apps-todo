using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// Multi-user isolation for Quest and Epic aggregates. Previously these were globally
/// keyed at the repo layer (a known data-model gap noted in IUserDataPurger). After
/// adding the shadow UserId column + scoped repos, each user must see only their own
/// quests/epics, cannot fetch another user's by id, and account-data delete must purge
/// them.
/// </summary>
[Trait("Category", "Api")]
public sealed class QuestEpicMultiUserIsolationTests : IDisposable
{
    private static readonly Guid _userA = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid _userB = new("00000000-0000-0000-0000-000000000002");

    private readonly WebApplicationFactory<Program> _factory = new();

    public void Dispose() => _factory.Dispose();

    private HttpClient AsUser(Guid userId)
    {
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTestFixture.GetTokenFor(userId));
        return client;
    }

    [Fact]
    public async Task Should_IsolateQuestListsPerUser()
    {
        using HttpClient clientA = AsUser(_userA);
        using HttpClient clientB = AsUser(_userB);

        HttpResponseMessage created = await clientA.PostAsJsonAsync(
            "/api/quests", new { title = "User A quest", description = "private" });
        created.IsSuccessStatusCode.ShouldBeTrue();

        HttpResponseMessage listB = await clientB.GetAsync("/api/quests");
        listB.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonElement listBJson = await listB.Content.ReadFromJsonAsync<JsonElement>();
        listBJson.GetArrayLength().ShouldBe(0);

        HttpResponseMessage listA = await clientA.GetAsync("/api/quests");
        JsonElement listAJson = await listA.Content.ReadFromJsonAsync<JsonElement>();
        listAJson.GetArrayLength().ShouldBe(1);
    }

    [Fact]
    public async Task Should_Return404_When_UserFetchesAnotherUsersQuestById()
    {
        using HttpClient clientA = AsUser(_userA);
        using HttpClient clientB = AsUser(_userB);

        HttpResponseMessage created = await clientA.PostAsJsonAsync(
            "/api/quests", new { title = "private", description = "x" });
        JsonElement createdJson = await created.Content.ReadFromJsonAsync<JsonElement>();
        Guid questId = createdJson.GetProperty("id").GetGuid();

        HttpResponseMessage fetch = await clientB.GetAsync($"/api/quests/{questId}");
        fetch.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_IsolateEpicListsPerUser()
    {
        using HttpClient clientA = AsUser(_userA);
        using HttpClient clientB = AsUser(_userB);

        HttpResponseMessage created = await clientA.PostAsJsonAsync(
            "/api/epics", new { title = "User A epic", description = "private" });
        created.IsSuccessStatusCode.ShouldBeTrue();

        HttpResponseMessage listB = await clientB.GetAsync("/api/epics");
        listB.StatusCode.ShouldBe(HttpStatusCode.OK);
        JsonElement listBJson = await listB.Content.ReadFromJsonAsync<JsonElement>();
        listBJson.GetArrayLength().ShouldBe(0);

        HttpResponseMessage listA = await clientA.GetAsync("/api/epics");
        JsonElement listAJson = await listA.Content.ReadFromJsonAsync<JsonElement>();
        listAJson.GetArrayLength().ShouldBe(1);
    }

    [Fact]
    public async Task Should_Return404_When_UserFetchesAnotherUsersEpicById()
    {
        using HttpClient clientA = AsUser(_userA);
        using HttpClient clientB = AsUser(_userB);

        HttpResponseMessage created = await clientA.PostAsJsonAsync(
            "/api/epics", new { title = "private", description = "x" });
        JsonElement createdJson = await created.Content.ReadFromJsonAsync<JsonElement>();
        Guid epicId = createdJson.GetProperty("id").GetGuid();

        HttpResponseMessage fetch = await clientB.GetAsync($"/api/epics/{epicId}");
        fetch.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_PurgeQuestsAndEpics_When_AccountDataDeleted()
    {
        using HttpClient clientA = AsUser(_userA);
        using HttpClient clientB = AsUser(_userB);

        await clientA.PostAsJsonAsync("/api/quests", new { title = "A quest", description = "x" });
        await clientA.PostAsJsonAsync("/api/epics", new { title = "A epic", description = "x" });
        await clientB.PostAsJsonAsync("/api/quests", new { title = "B quest", description = "x" });
        await clientB.PostAsJsonAsync("/api/epics", new { title = "B epic", description = "x" });

        HttpResponseMessage del = await clientA.PostAsJsonAsync(
            "/api/data/delete", new { confirmation = "DELETE MY DATA" });
        del.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // A's quests/epics gone.
        JsonElement aQuests = await (await clientA.GetAsync("/api/quests")).Content.ReadFromJsonAsync<JsonElement>();
        aQuests.GetArrayLength().ShouldBe(0);
        JsonElement aEpics = await (await clientA.GetAsync("/api/epics")).Content.ReadFromJsonAsync<JsonElement>();
        aEpics.GetArrayLength().ShouldBe(0);

        // B's still there.
        JsonElement bQuests = await (await clientB.GetAsync("/api/quests")).Content.ReadFromJsonAsync<JsonElement>();
        bQuests.GetArrayLength().ShouldBe(1);
        JsonElement bEpics = await (await clientB.GetAsync("/api/epics")).Content.ReadFromJsonAsync<JsonElement>();
        bEpics.GetArrayLength().ShouldBe(1);
    }
}
