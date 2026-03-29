using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class RecurringTasksControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RecurringTasksControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnEmptyList_When_NoRecurringTasksExist()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/recurring-tasks");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        List<RecurringTaskDto>? tasks = await response.Content.ReadFromJsonAsync<List<RecurringTaskDto>>();
        tasks.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_ReturnCreatedRecurringTask_When_ValidDataProvided()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Daily standup", pattern = "Daily" });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        RecurringTaskDto? task = await response.Content.ReadFromJsonAsync<RecurringTaskDto>();
        task!.Id.ShouldNotBe(Guid.Empty);
        task.Title.ShouldBe("Daily standup");
        task.Pattern.ShouldBe("Daily");
        task.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_PatternIsInvalid()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Bad pattern", pattern = "Hourly" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_ReturnRecurringTask_When_TaskExists()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Weekly review", pattern = "Weekly" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        HttpResponseMessage response = await _client.GetAsync($"/api/recurring-tasks/{created!.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        RecurringTaskDto? task = await response.Content.ReadFromJsonAsync<RecurringTaskDto>();
        task!.Title.ShouldBe("Weekly review");
    }

    [Fact]
    public async Task Should_PauseRecurringTask_When_TaskIsActive()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Pausable", pattern = "Daily" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        HttpResponseMessage response = await _client.PatchAsync(
            $"/api/recurring-tasks/{created!.Id}/pause", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        RecurringTaskDto? paused = await response.Content.ReadFromJsonAsync<RecurringTaskDto>();
        paused!.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_ResumeRecurringTask_When_TaskIsPaused()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Resumable", pattern = "Daily" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        await _client.PatchAsync($"/api/recurring-tasks/{created!.Id}/pause", null);

        HttpResponseMessage response = await _client.PatchAsync(
            $"/api/recurring-tasks/{created.Id}/resume", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        RecurringTaskDto? resumed = await response.Content.ReadFromJsonAsync<RecurringTaskDto>();
        resumed!.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_ReturnConflict_When_PausingAlreadyPausedTask()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Already paused", pattern = "Daily" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        await _client.PatchAsync($"/api/recurring-tasks/{created!.Id}/pause", null);

        HttpResponseMessage response = await _client.PatchAsync(
            $"/api/recurring-tasks/{created.Id}/pause", null);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_GenerateInstance_When_TaskIsActive()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Generate me", pattern = "Daily" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        HttpResponseMessage response = await _client.PostAsync(
            $"/api/recurring-tasks/{created!.Id}/generate", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        TaskResponseDto? instance = await response.Content.ReadFromJsonAsync<TaskResponseDto>();
        instance!.Title.ShouldBe("Generate me");
        instance.Status.ShouldBe("Todo");
    }

    [Fact]
    public async Task Should_ReturnConflict_When_GeneratingFromPausedTask()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Paused gen", pattern = "Daily" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        await _client.PatchAsync($"/api/recurring-tasks/{created!.Id}/pause", null);

        HttpResponseMessage response = await _client.PostAsync(
            $"/api/recurring-tasks/{created.Id}/generate", null);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_ReturnNoContent_When_DeletingExistingRecurringTask()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Delete me", pattern = "Monthly" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        HttpResponseMessage response = await _client.DeleteAsync($"/api/recurring-tasks/{created!.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_DeletingNonExistentRecurringTask()
    {
        HttpResponseMessage response = await _client.DeleteAsync($"/api/recurring-tasks/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private sealed record RecurringTaskDto(Guid Id, string Title, string Pattern, bool IsActive);
}
