using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class RecordActualTimeTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RecordActualTimeTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    private sealed record TaskSummary(Guid Id);
    private sealed record TaskDetail(
        Guid Id, string Title, string Status, int? EstimatedMinutes,
        int? ActualMinutes, int? VariancePercent);

    private async Task<Guid> CreateDoneTaskWithEstimate(int estimatedMinutes)
    {
        HttpResponseMessage create = await _client.PostAsJsonAsync("/api/tasks", new { title = "Timed task" });
        TaskSummary? created = await create.Content.ReadFromJsonAsync<TaskSummary>();
        await _client.PatchAsJsonAsync($"/api/tasks/{created!.Id}", new { estimatedMinutes });
        await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new { status = "InProgress" });
        await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new { status = "Done" });
        return created.Id;
    }

    [Fact]
    public async Task Should_RecordActualTime_When_TaskDoneWithEstimate()
    {
        Guid id = await CreateDoneTaskWithEstimate(30);

        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{id}/actual-time",
            new { actualMinutes = 45 });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        TaskDetail? task = await response.Content.ReadFromJsonAsync<TaskDetail>();
        task!.ActualMinutes.ShouldBe(45);
        task.VariancePercent.ShouldBe(50);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TaskMissing()
    {
        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{Guid.NewGuid()}/actual-time",
            new { actualMinutes = 10 });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TaskNotDone()
    {
        HttpResponseMessage create = await _client.PostAsJsonAsync("/api/tasks", new { title = "Not done" });
        TaskSummary? created = await create.Content.ReadFromJsonAsync<TaskSummary>();
        await _client.PatchAsJsonAsync($"/api/tasks/{created!.Id}", new { estimatedMinutes = 30 });

        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{created.Id}/actual-time",
            new { actualMinutes = 20 });

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TaskHasNoEstimate()
    {
        HttpResponseMessage create = await _client.PostAsJsonAsync("/api/tasks", new { title = "No estimate" });
        TaskSummary? created = await create.Content.ReadFromJsonAsync<TaskSummary>();
        await _client.PatchAsJsonAsync($"/api/tasks/{created!.Id}/status", new { status = "InProgress" });
        await _client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status", new { status = "Done" });

        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{created.Id}/actual-time",
            new { actualMinutes = 20 });

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_ActualMinutesOutOfRange()
    {
        Guid id = await CreateDoneTaskWithEstimate(30);

        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{id}/actual-time",
            new { actualMinutes = 0 });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NoToken()
    {
        using HttpClient unauth = _factory.CreateClient();
        HttpResponseMessage response = await unauth.PatchAsJsonAsync(
            $"/api/tasks/{Guid.NewGuid()}/actual-time",
            new { actualMinutes = 10 });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_ReturnDifficultySuggestion_When_ActualTimeFarFromEstimate()
    {
        Guid id = await CreateDoneTaskWithEstimate(60);

        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/tasks/{id}/actual-time",
            new { actualMinutes = 10 });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("difficultySuggestion");
        json.ShouldContain("difficultySuggestionReason");

        var task = await response.Content.ReadFromJsonAsync<TaskWithSuggestion>();
        task!.DifficultySuggestion.ShouldNotBeNull();
        task.DifficultySuggestionReason.ShouldNotBeNull();
    }

    private sealed record TaskWithSuggestion(
        Guid Id, string? DifficultySuggestion, string? DifficultySuggestionReason);
}
