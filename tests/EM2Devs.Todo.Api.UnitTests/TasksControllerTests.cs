using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// Gate 4: Scenario-driven API contract tests.
/// Verifies controller behavior matches the OpenAPI contract (ADR-0004).
/// Each test gets a fresh factory to ensure an isolated in-memory store.
/// </summary>
[Trait("Category", "Api")]
public sealed class TasksControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TasksControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnEmptyList_When_NoTasksExist()
    {
        // When
        var response = await _client.GetAsync("/api/tasks");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
        tasks.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_ReturnCreatedTask_When_ValidTitleProvided()
    {
        // When
        var response = await _client.PostAsJsonAsync("/api/tasks", new { title = "Write tests" });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var task = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        task!.Id.ShouldNotBe(Guid.Empty);
        task.Title.ShouldBe("Write tests");
        task.Status.ShouldBe("Todo");
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TitleIsEmpty()
    {
        // When
        var response = await _client.PostAsJsonAsync("/api/tasks", new { title = "" });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_ReturnTask_When_TaskExists()
    {
        // Given
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new { title = "Find me" });
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        // When
        var response = await _client.GetAsync($"/api/tasks/{created!.Id}");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        task!.Id.ShouldBe(created.Id);
        task.Title.ShouldBe("Find me");
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TaskDoesNotExist()
    {
        // When
        var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_UpdateStatusToInProgress_When_TaskIsTodo()
    {
        // Given
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new { title = "Start me" });
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        // When
        var response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{created!.Id}/status",
            new { status = "InProgress" });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        task!.Status.ShouldBe("InProgress");
    }

    [Fact]
    public async Task Should_UpdateStatusToDone_When_TaskIsInProgress()
    {
        // Given
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new { title = "Complete me" });
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
        await _client.PatchAsJsonAsync($"/api/tasks/{created!.Id}/status", new { status = "InProgress" });

        // When
        var response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{created.Id}/status",
            new { status = "Done" });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        task!.Status.ShouldBe("Done");
    }

    [Fact]
    public async Task Should_ReturnConflict_When_StatusTransitionIsInvalid()
    {
        // Given
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new { title = "Skip ahead" });
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        // When — Todo directly to Done is not allowed
        var response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{created!.Id}/status",
            new { status = "Done" });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TaskAlreadyInRequestedStatus()
    {
        // Given
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new { title = "Same status" });
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        // When — Task is already Todo
        var response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{created!.Id}/status",
            new { status = "Todo" });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_UpdatingStatusOfNonexistentTask()
    {
        // When
        var response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{Guid.NewGuid()}/status",
            new { status = "InProgress" });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_StatusValueIsInvalid()
    {
        // Given
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new { title = "Bad status" });
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        // When
        var response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{created!.Id}/status",
            new { status = "InvalidStatus" });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_ReturnNoContent_When_DeletingExistingTask()
    {
        // Given
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new { title = "Delete me" });
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        // When
        var response = await _client.DeleteAsync($"/api/tasks/{created!.Id}");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_DeletingNonExistentTask()
    {
        // When
        var response = await _client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_RemoveTask_When_Deleted()
    {
        // Given
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new { title = "Gone forever" });
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
        await _client.DeleteAsync($"/api/tasks/{created!.Id}");

        // When
        var getResponse = await _client.GetAsync($"/api/tasks/{created.Id}");

        // Then
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnOnlyMatchingTasks_When_FilteredByStatus()
    {
        // Given
        await _client.PostAsJsonAsync("/api/tasks", new { title = "Task one" });
        await _client.PostAsJsonAsync("/api/tasks", new { title = "Task two" });

        // When
        var response = await _client.GetAsync("/api/tasks?status=Todo");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
        tasks!.Count.ShouldBe(2);
        tasks.ShouldAllBe(t => t.Status == "Todo");
    }

    [Fact]
    public async Task Should_ReturnEmptyList_When_FilteredByStatusWithNoMatches()
    {
        // Given
        await _client.PostAsJsonAsync("/api/tasks", new { title = "A todo task" });

        // When
        var response = await _client.GetAsync("/api/tasks?status=InProgress");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
        tasks.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_ReturnAllTasks_When_NoStatusFilterProvided()
    {
        // Given
        await _client.PostAsJsonAsync("/api/tasks", new { title = "First" });
        await _client.PostAsJsonAsync("/api/tasks", new { title = "Second" });

        // When
        var response = await _client.GetAsync("/api/tasks");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
        tasks!.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_StatusFilterIsInvalid()
    {
        // When
        var response = await _client.GetAsync("/api/tasks?status=InvalidStatus");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_UpdatePriority_When_ValidPriorityProvided()
    {
        // Given
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/tasks",
            new { title = "Priority test" });
        TaskResponseDto? created = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        // When
        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{created!.Id}",
            new { priority = "Critical" });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        TaskDetailDto? updated = await response.Content.ReadFromJsonAsync<TaskDetailDto>();
        updated!.Priority.ShouldBe("Critical");
        updated.Difficulty.ShouldBe("Normal"); // unchanged
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_InvalidPriorityProvided()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/tasks",
            new { title = "Bad priority" });
        TaskResponseDto? created = await createResponse.Content.ReadFromJsonAsync<TaskResponseDto>();

        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{created!.Id}",
            new { priority = "Urgent" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private sealed record TaskDetailDto(Guid Id, string Title, string Status, string Difficulty, string Priority);
}

internal sealed record TaskResponseDto(Guid Id, string Title, string Status);
