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
        _client = _factory.CreateClient().Authenticated();
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

    [Fact]
    public async Task Should_ReturnInstances_When_ListingRecurringTaskInstances()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Daily standup", pattern = "Daily" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        await _client.PostAsync($"/api/recurring-tasks/{created!.Id}/generate?scheduledDate=2026-04-01", null);
        await _client.PostAsync($"/api/recurring-tasks/{created.Id}/generate?scheduledDate=2026-04-02", null);

        HttpResponseMessage response = await _client.GetAsync(
            $"/api/recurring-tasks/{created.Id}/instances");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        List<TaskInstanceDto>? instances = await response.Content.ReadFromJsonAsync<List<TaskInstanceDto>>();
        instances.ShouldNotBeNull();
        instances.Count.ShouldBe(2);
        instances.ShouldAllBe(i => i.SourceRecurringTaskId == created.Id);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ListingInstancesForNonExistentRecurringTask()
    {
        HttpResponseMessage response = await _client.GetAsync(
            $"/api/recurring-tasks/{Guid.NewGuid()}/instances");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_IncludeScheduledDate_When_GeneratingInstance()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Scheduled gen", pattern = "Daily" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        HttpResponseMessage response = await _client.PostAsync(
            $"/api/recurring-tasks/{created!.Id}/generate?scheduledDate=2026-04-01", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        TaskInstanceDto? instance = await response.Content.ReadFromJsonAsync<TaskInstanceDto>();
        instance!.ScheduledDate.ShouldBe("2026-04-01");
        instance.SourceRecurringTaskId.ShouldBe(created.Id);
    }

    [Fact]
    public async Task Should_ReturnOk_When_UpdatingRecurringTask()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Old title", pattern = "Daily" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/recurring-tasks/{created!.Id}",
            new { title = "New title", pattern = "Weekly", applyToFutureInstances = false });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        RecurringTaskDto? updated = await response.Content.ReadFromJsonAsync<RecurringTaskDto>();
        updated!.Title.ShouldBe("New title");
        updated.Pattern.ShouldBe("Weekly");
    }

    [Fact]
    public async Task Should_UpdateFutureInstances_When_ApplyToFutureInstancesIsTrue()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Old name", pattern = "Daily" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        // Generate a future instance (fixed date well in the future)
        string futureDate = "2099-01-02";
        await _client.PostAsync($"/api/recurring-tasks/{created!.Id}/generate?scheduledDate={futureDate}", null);

        // Generate a past instance — should NOT be updated
        string pastDate = "2020-01-01";
        await _client.PostAsync($"/api/recurring-tasks/{created.Id}/generate?scheduledDate={pastDate}", null);

        // Update with apply to future instances
        await _client.PutAsJsonAsync($"/api/recurring-tasks/{created.Id}",
            new { title = "New name", applyToFutureInstances = true });

        // Verify instances
        HttpResponseMessage instancesResponse = await _client.GetAsync(
            $"/api/recurring-tasks/{created.Id}/instances");
        List<TaskInstanceDto>? instances = await instancesResponse.Content
            .ReadFromJsonAsync<List<TaskInstanceDto>>();

        instances.ShouldNotBeNull();
        instances.Count.ShouldBe(2);

        TaskInstanceDto futureInstance = instances.First(i => i.ScheduledDate == futureDate);
        futureInstance.Title.ShouldBe("New name");

        TaskInstanceDto pastInstance = instances.First(i => i.ScheduledDate == pastDate);
        pastInstance.Title.ShouldBe("Old name");
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_UpdatingNonExistentRecurringTask()
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/recurring-tasks/{Guid.NewGuid()}",
            new { title = "Doesn't exist" });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnOk_When_SkippingOpenInstance()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Skippable", pattern = "Daily" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        HttpResponseMessage genResponse = await _client.PostAsync(
            $"/api/recurring-tasks/{created!.Id}/generate?scheduledDate=2026-04-01", null);
        TaskInstanceDto? instance = await genResponse.Content.ReadFromJsonAsync<TaskInstanceDto>();

        HttpResponseMessage response = await _client.PatchAsync(
            $"/api/recurring-tasks/{created.Id}/instances/{instance!.Id}/skip", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        TaskInstanceDto? skipped = await response.Content.ReadFromJsonAsync<TaskInstanceDto>();
        skipped!.Status.ShouldBe("Skipped");
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_SkippingInstanceFromWrongRecurringTask()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Owner A", pattern = "Daily" });
        RecurringTaskDto? ownerA = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        HttpResponseMessage genResponse = await _client.PostAsync(
            $"/api/recurring-tasks/{ownerA!.Id}/generate?scheduledDate=2026-04-01", null);
        TaskInstanceDto? instance = await genResponse.Content.ReadFromJsonAsync<TaskInstanceDto>();

        // Try to skip using a different recurring task ID
        HttpResponseMessage response = await _client.PatchAsync(
            $"/api/recurring-tasks/{Guid.NewGuid()}/instances/{instance!.Id}/skip", null);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_SkippingCompletedInstance()
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Complete then skip", pattern = "Daily" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        HttpResponseMessage genResponse = await _client.PostAsync(
            $"/api/recurring-tasks/{created!.Id}/generate?scheduledDate=2026-04-01", null);
        TaskInstanceDto? instance = await genResponse.Content.ReadFromJsonAsync<TaskInstanceDto>();

        await _client.PatchAsJsonAsync($"/api/tasks/{instance!.Id}/status",
            new { status = "InProgress" });
        await _client.PatchAsJsonAsync($"/api/tasks/{instance.Id}/status",
            new { status = "Done" });

        HttpResponseMessage response = await _client.PatchAsync(
            $"/api/recurring-tasks/{created.Id}/instances/{instance.Id}/skip", null);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("not-a-date")]
    [InlineData("2026-13-99")]
    [InlineData("")]
    public async Task Should_ReturnBadRequest_When_ScheduledDateIsInvalid(string invalidDate)
    {
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks",
            new { title = "Bad date", pattern = "Daily" });
        RecurringTaskDto? created = await createResponse.Content.ReadFromJsonAsync<RecurringTaskDto>();

        HttpResponseMessage response = await _client.PostAsync(
            $"/api/recurring-tasks/{created!.Id}/generate?scheduledDate={invalidDate}", null);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        string body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("Invalid scheduledDate format. Expected: yyyy-MM-dd");
    }

    private sealed record RecurringTaskDto(Guid Id, string Title, string Pattern, bool IsActive);
    private sealed record TaskInstanceDto(
        Guid Id, string Title, string? Description, string Status, string Difficulty,
        DateTimeOffset? DueDate, DateTimeOffset? CompletedAt, string? ScheduledDate,
        Guid? SourceRecurringTaskId);
}
