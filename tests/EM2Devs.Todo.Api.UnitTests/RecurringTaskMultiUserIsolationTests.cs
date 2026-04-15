using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// Slice 2 multi-user data isolation: verify that RecurringTasks created by one user
/// are not visible to another user.
/// </summary>
[Trait("Category", "Api")]
public sealed class RecurringTaskMultiUserIsolationTests : IDisposable
{
    private static readonly Guid _userA = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid _userB = new("00000000-0000-0000-0000-000000000002");

    private readonly WebApplicationFactory<Program> _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_IsolateRecurringTaskListsPerUser_When_EachUserRequestsTheirTemplates()
    {
        // Given — user A creates a recurring task
        HttpClient clientA = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTestFixture.GetTokenFor(_userA));

        HttpResponseMessage createdA = await clientA.PostAsJsonAsync(
            "/api/recurring-tasks", new { title = "A's daily standup", pattern = "Daily" });
        createdA.StatusCode.ShouldBe(HttpStatusCode.Created);

        // When — user B lists recurring tasks
        HttpClient clientB = _factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTestFixture.GetTokenFor(_userB));

        HttpResponseMessage listBResponse = await clientB.GetAsync("/api/recurring-tasks");
        listBResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        List<RecurringTaskResponseDto>? listB = await listBResponse.Content
            .ReadFromJsonAsync<List<RecurringTaskResponseDto>>();

        // Then — user B sees nothing
        listB.ShouldNotBeNull();
        listB.ShouldBeEmpty();

        // And — user A still sees their own template
        HttpResponseMessage listAResponse = await clientA.GetAsync("/api/recurring-tasks");
        listAResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        List<RecurringTaskResponseDto>? listA = await listAResponse.Content
            .ReadFromJsonAsync<List<RecurringTaskResponseDto>>();
        listA.ShouldNotBeNull();
        listA.Count.ShouldBe(1);
        listA[0].Title.ShouldBe("A's daily standup");
    }

    [Fact]
    public async Task Should_Return404_When_UserTriesToFetchAnotherUsersRecurringTaskById()
    {
        // Given — user A creates a recurring task
        HttpClient clientA = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTestFixture.GetTokenFor(_userA));
        HttpResponseMessage created = await clientA.PostAsJsonAsync(
            "/api/recurring-tasks", new { title = "A's private recurring", pattern = "Weekly" });
        RecurringTaskResponseDto? recurring = await created.Content
            .ReadFromJsonAsync<RecurringTaskResponseDto>();
        recurring.ShouldNotBeNull();

        // When — user B tries to fetch user A's template
        HttpClient clientB = _factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTestFixture.GetTokenFor(_userB));
        HttpResponseMessage response = await clientB.GetAsync($"/api/recurring-tasks/{recurring.Id}");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}

internal sealed record RecurringTaskResponseDto(Guid Id, string Title, string Pattern, bool IsActive);
