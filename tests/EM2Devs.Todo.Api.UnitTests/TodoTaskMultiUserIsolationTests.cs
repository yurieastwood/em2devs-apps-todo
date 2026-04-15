using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// Slice 1 multi-user data isolation: verify that TodoTasks created by one user
/// are not visible to another user and cannot be fetched by id across users.
/// </summary>
[Trait("Category", "Api")]
public sealed class TodoTaskMultiUserIsolationTests : IDisposable
{
    private static readonly Guid _userA = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid _userB = new("00000000-0000-0000-0000-000000000002");

    private readonly WebApplicationFactory<Program> _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_IsolateTaskListsPerUser_When_EachUserRequestsTheirTasks()
    {
        // Given — user A creates one task
        HttpClient clientA = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTestFixture.GetTokenFor(_userA));

        HttpResponseMessage createdA = await clientA.PostAsJsonAsync(
            "/api/tasks", new { title = "User A's task" });
        createdA.StatusCode.ShouldBe(HttpStatusCode.Created);

        // When — user B lists tasks
        HttpClient clientB = _factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTestFixture.GetTokenFor(_userB));

        HttpResponseMessage listBResponse = await clientB.GetAsync("/api/tasks");
        listBResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        List<TaskResponseDto>? tasksB = await listBResponse.Content.ReadFromJsonAsync<List<TaskResponseDto>>();

        // Then — user B sees no tasks
        tasksB.ShouldNotBeNull();
        tasksB.ShouldBeEmpty();

        // And — user A still sees their own task
        HttpResponseMessage listAResponse = await clientA.GetAsync("/api/tasks");
        listAResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        List<TaskResponseDto>? tasksA = await listAResponse.Content.ReadFromJsonAsync<List<TaskResponseDto>>();
        tasksA.ShouldNotBeNull();
        tasksA.Count.ShouldBe(1);
        tasksA[0].Title.ShouldBe("User A's task");
    }

    [Fact]
    public async Task Should_Return404_When_UserTriesToFetchAnotherUsersTaskById()
    {
        // Given — user A creates a task and captures its id
        HttpClient clientA = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTestFixture.GetTokenFor(_userA));
        HttpResponseMessage created = await clientA.PostAsJsonAsync(
            "/api/tasks", new { title = "A's private task" });
        TaskResponseDto? task = await created.Content.ReadFromJsonAsync<TaskResponseDto>();
        task.ShouldNotBeNull();

        // When — user B tries to fetch user A's task by id
        HttpClient clientB = _factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTestFixture.GetTokenFor(_userB));
        HttpResponseMessage response = await clientB.GetAsync($"/api/tasks/{task.Id}");

        // Then — the task is not found for user B
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
