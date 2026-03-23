using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class QuestsControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public QuestsControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnEmptyList_When_NoQuestsExist()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/quests");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        List<QuestDto>? quests = await response.Content.ReadFromJsonAsync<List<QuestDto>>();
        quests.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_ReturnCreatedQuest_When_ValidDataProvided()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/quests",
            new { title = "My Quest", description = "A test quest" });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        QuestDto? quest = await response.Content.ReadFromJsonAsync<QuestDto>();
        quest!.Id.ShouldNotBe(Guid.Empty);
        quest.Title.ShouldBe("My Quest");
        quest.Description.ShouldBe("A test quest");
        quest.Progress.ShouldBe(0);
    }

    [Fact]
    public async Task Should_ReturnQuest_When_QuestExists()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/quests",
            new { title = "Find me", description = "Quest to find" });
        QuestDto? created = await createResponse.Content.ReadFromJsonAsync<QuestDto>();

        HttpResponseMessage response = await _client.GetAsync($"/api/quests/{created!.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        QuestDto? quest = await response.Content.ReadFromJsonAsync<QuestDto>();
        quest!.Title.ShouldBe("Find me");
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_QuestDoesNotExist()
    {
        HttpResponseMessage response = await _client.GetAsync($"/api/quests/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_AddTaskToQuest_When_BothExist()
    {
        HttpResponseMessage questResponse = await _client.PostAsJsonAsync("/api/quests",
            new { title = "Quest", description = "Test" });
        QuestDto? quest = await questResponse.Content.ReadFromJsonAsync<QuestDto>();

        HttpResponseMessage taskResponse = await _client.PostAsJsonAsync("/api/tasks",
            new { title = "Task for quest" });
        TaskResponseDto? task = await taskResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            $"/api/quests/{quest!.Id}/tasks", new { taskId = task!.Id });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        QuestDto? updated = await response.Content.ReadFromJsonAsync<QuestDto>();
        updated!.Tasks.Count.ShouldBe(1);
        updated.Tasks[0].Title.ShouldBe("Task for quest");
    }

    [Fact]
    public async Task Should_RemoveTaskFromQuest_When_TaskAssigned()
    {
        HttpResponseMessage questResponse = await _client.PostAsJsonAsync("/api/quests",
            new { title = "Quest", description = "Test" });
        QuestDto? quest = await questResponse.Content.ReadFromJsonAsync<QuestDto>();

        HttpResponseMessage taskResponse = await _client.PostAsJsonAsync("/api/tasks",
            new { title = "Removable task" });
        TaskResponseDto? task = await taskResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        await _client.PostAsJsonAsync($"/api/quests/{quest!.Id}/tasks", new { taskId = task!.Id });

        HttpResponseMessage response = await _client.DeleteAsync(
            $"/api/quests/{quest.Id}/tasks/{task.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        QuestDto? updated = await response.Content.ReadFromJsonAsync<QuestDto>();
        updated!.Tasks.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_ReturnNoContent_When_DeletingExistingQuest()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/quests",
            new { title = "Delete me", description = "To delete" });
        QuestDto? created = await createResponse.Content.ReadFromJsonAsync<QuestDto>();

        HttpResponseMessage response = await _client.DeleteAsync($"/api/quests/{created!.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_DeletingNonExistentQuest()
    {
        HttpResponseMessage response = await _client.DeleteAsync($"/api/quests/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_CompleteQuest_When_AllTasksAreDone()
    {
        // Given
        HttpResponseMessage questResponse = await _client.PostAsJsonAsync("/api/quests",
            new { title = "Completable", description = "Test" });
        QuestDto? quest = await questResponse.Content.ReadFromJsonAsync<QuestDto>();

        HttpResponseMessage taskResponse = await _client.PostAsJsonAsync("/api/tasks",
            new { title = "Only task" });
        TaskResponseDto? task = await taskResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        await _client.PostAsJsonAsync($"/api/quests/{quest!.Id}/tasks", new { taskId = task!.Id });
        await _client.PatchAsJsonAsync($"/api/tasks/{task.Id}/status", new { status = "InProgress" });
        await _client.PatchAsJsonAsync($"/api/tasks/{task.Id}/status", new { status = "Done" });

        // When
        HttpResponseMessage response = await _client.PostAsync($"/api/quests/{quest.Id}/complete", null);

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        QuestDto? completed = await response.Content.ReadFromJsonAsync<QuestDto>();
        completed!.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_ReturnConflict_When_CompletingQuestWithIncompleteTasks()
    {
        // Given
        HttpResponseMessage questResponse = await _client.PostAsJsonAsync("/api/quests",
            new { title = "Incomplete", description = "Test" });
        QuestDto? quest = await questResponse.Content.ReadFromJsonAsync<QuestDto>();

        HttpResponseMessage taskResponse = await _client.PostAsJsonAsync("/api/tasks",
            new { title = "Undone task" });
        TaskResponseDto? task = await taskResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        await _client.PostAsJsonAsync($"/api/quests/{quest!.Id}/tasks", new { taskId = task!.Id });

        // When
        HttpResponseMessage response = await _client.PostAsync($"/api/quests/{quest.Id}/complete", null);

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_CompletingNonExistentQuest()
    {
        HttpResponseMessage response = await _client.PostAsync($"/api/quests/{Guid.NewGuid()}/complete", null);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private sealed record QuestTaskDto(Guid Id, string Title, string Status);
    private sealed record QuestDto(Guid Id, string Title, string Description, DateOnly? DueDate, int Progress, bool IsCompleted, List<QuestTaskDto> Tasks);
}
