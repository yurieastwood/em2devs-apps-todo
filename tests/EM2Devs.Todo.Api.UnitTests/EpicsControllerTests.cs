using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class EpicsControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EpicsControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnEmptyList_When_NoEpicsExist()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/epics");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        List<EpicDto>? epics = await response.Content.ReadFromJsonAsync<List<EpicDto>>();
        epics.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_ReturnCreatedEpic_When_ValidDataProvided()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/epics",
            new { title = "My Epic", description = "An epic quest" });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        EpicDto? epic = await response.Content.ReadFromJsonAsync<EpicDto>();
        epic!.Id.ShouldNotBe(Guid.Empty);
        epic.Title.ShouldBe("My Epic");
        epic.Progress.ShouldBe(0m);
    }

    [Fact]
    public async Task Should_ReturnEpic_When_EpicExists()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/epics",
            new { title = "Find epic", description = "Epic to find" });
        EpicDto? created = await createResponse.Content.ReadFromJsonAsync<EpicDto>();

        HttpResponseMessage response = await _client.GetAsync($"/api/epics/{created!.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        EpicDto? epic = await response.Content.ReadFromJsonAsync<EpicDto>();
        epic!.Title.ShouldBe("Find epic");
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_EpicDoesNotExist()
    {
        HttpResponseMessage response = await _client.GetAsync($"/api/epics/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_AssignQuestToEpic_When_BothExist()
    {
        HttpResponseMessage epicResponse = await _client.PostAsJsonAsync("/api/epics",
            new { title = "Epic", description = "Test" });
        EpicDto? epic = await epicResponse.Content.ReadFromJsonAsync<EpicDto>();

        HttpResponseMessage questResponse = await _client.PostAsJsonAsync("/api/quests",
            new { title = "Quest for epic", description = "Test" });
        EpicQuestDto? quest = await questResponse.Content.ReadFromJsonAsync<EpicQuestDto>();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            $"/api/epics/{epic!.Id}/quests", new { questId = quest!.Id });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        EpicDto? updated = await response.Content.ReadFromJsonAsync<EpicDto>();
        updated!.Quests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Should_RemoveQuestFromEpic_When_QuestAssigned()
    {
        HttpResponseMessage epicResponse = await _client.PostAsJsonAsync("/api/epics",
            new { title = "Epic", description = "Test" });
        EpicDto? epic = await epicResponse.Content.ReadFromJsonAsync<EpicDto>();

        HttpResponseMessage questResponse = await _client.PostAsJsonAsync("/api/quests",
            new { title = "Removable quest", description = "Test" });
        EpicQuestDto? quest = await questResponse.Content.ReadFromJsonAsync<EpicQuestDto>();

        await _client.PostAsJsonAsync($"/api/epics/{epic!.Id}/quests", new { questId = quest!.Id });

        HttpResponseMessage response = await _client.DeleteAsync(
            $"/api/epics/{epic.Id}/quests/{quest.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        EpicDto? updated = await response.Content.ReadFromJsonAsync<EpicDto>();
        updated!.Quests.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_ReturnNoContent_When_DeletingExistingEpic()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/epics",
            new { title = "Delete me", description = "To delete" });
        EpicDto? created = await createResponse.Content.ReadFromJsonAsync<EpicDto>();

        HttpResponseMessage response = await _client.DeleteAsync($"/api/epics/{created!.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_DeletingNonExistentEpic()
    {
        HttpResponseMessage response = await _client.DeleteAsync($"/api/epics/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_CompleteEpic_When_AllQuestsAreDone()
    {
        // Given — create epic, quest, task; complete task; assign quest to epic
        HttpResponseMessage epicResponse = await _client.PostAsJsonAsync("/api/epics",
            new { title = "Completable", description = "Test" });
        EpicDto? epic = await epicResponse.Content.ReadFromJsonAsync<EpicDto>();

        HttpResponseMessage questResponse = await _client.PostAsJsonAsync("/api/quests",
            new { title = "Done quest", description = "Test" });
        EpicQuestDto? quest = await questResponse.Content.ReadFromJsonAsync<EpicQuestDto>();

        HttpResponseMessage taskResponse = await _client.PostAsJsonAsync("/api/tasks",
            new { title = "Done task" });
        TaskResponseDto? task = await taskResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        await _client.PostAsJsonAsync($"/api/quests/{quest!.Id}/tasks", new { taskId = task!.Id });
        await _client.PatchAsJsonAsync($"/api/tasks/{task.Id}/status", new { status = "InProgress" });
        await _client.PatchAsJsonAsync($"/api/tasks/{task.Id}/status", new { status = "Done" });
        await _client.PostAsJsonAsync($"/api/epics/{epic!.Id}/quests", new { questId = quest.Id });

        // When
        HttpResponseMessage response = await _client.PostAsync($"/api/epics/{epic.Id}/complete", null);

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        EpicDto? completed = await response.Content.ReadFromJsonAsync<EpicDto>();
        completed!.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_ReturnConflict_When_CompletingEpicWithIncompleteQuests()
    {
        // Given
        HttpResponseMessage epicResponse = await _client.PostAsJsonAsync("/api/epics",
            new { title = "Incomplete", description = "Test" });
        EpicDto? epic = await epicResponse.Content.ReadFromJsonAsync<EpicDto>();

        HttpResponseMessage questResponse = await _client.PostAsJsonAsync("/api/quests",
            new { title = "Empty quest", description = "No tasks" });
        EpicQuestDto? quest = await questResponse.Content.ReadFromJsonAsync<EpicQuestDto>();

        await _client.PostAsJsonAsync($"/api/epics/{epic!.Id}/quests", new { questId = quest!.Id });

        // When
        HttpResponseMessage response = await _client.PostAsync($"/api/epics/{epic.Id}/complete", null);

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_CompletingNonExistentEpic()
    {
        HttpResponseMessage response = await _client.PostAsync($"/api/epics/{Guid.NewGuid()}/complete", null);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private sealed record EpicQuestDto(Guid Id, string Title, int Progress);
    private sealed record EpicDto(Guid Id, string Title, string Description, DateOnly? TargetDate, decimal Progress, bool IsCompleted, List<EpicQuestDto> Quests);
}
