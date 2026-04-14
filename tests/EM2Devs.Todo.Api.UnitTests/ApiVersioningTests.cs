using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

[Trait("Category", "Api")]
public sealed class ApiVersioningTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiVersioningTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient().Authenticated();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnSameResults_When_UsingVersionedAndUnversionedTasksRoute()
    {
        // Given
        HttpResponseMessage seedResponse = await _client.PostAsJsonAsync("/api/tasks", new { title = "Versioning test" });
        seedResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // When
        HttpResponseMessage unversioned = await _client.GetAsync("/api/tasks");
        HttpResponseMessage versioned = await _client.GetAsync("/api/v1/tasks");

        // Then
        unversioned.StatusCode.ShouldBe(HttpStatusCode.OK);
        versioned.StatusCode.ShouldBe(HttpStatusCode.OK);

        string unversionedBody = await unversioned.Content.ReadAsStringAsync();
        string versionedBody = await versioned.Content.ReadAsStringAsync();
        versionedBody.ShouldBe(unversionedBody);
    }

    [Fact]
    public async Task Should_ReturnSameResults_When_UsingVersionedAndUnversionedProfileRoute()
    {
        // When
        HttpResponseMessage unversioned = await _client.GetAsync("/api/profile");
        HttpResponseMessage versioned = await _client.GetAsync("/api/v1/profile");

        // Then
        unversioned.StatusCode.ShouldBe(HttpStatusCode.OK);
        versioned.StatusCode.ShouldBe(HttpStatusCode.OK);

        string unversionedBody = await unversioned.Content.ReadAsStringAsync();
        string versionedBody = await versioned.Content.ReadAsStringAsync();
        versionedBody.ShouldBe(unversionedBody);
    }

    [Fact]
    public async Task Should_ReturnApiVersionHeader_When_RequestIsMade()
    {
        // When
        HttpResponseMessage response = await _client.GetAsync("/api/v1/tasks");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.Contains("api-supported-versions").ShouldBeTrue();
        response.Headers.GetValues("api-supported-versions")
            .ShouldContain("1.0");
    }

    [Fact]
    public async Task Should_CreateTaskViaVersionedRoute_When_ValidTitleProvided()
    {
        // When
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/v1/tasks", new { title = "Versioned create" });

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        VersionedTaskResponse? task = await response.Content.ReadFromJsonAsync<VersionedTaskResponse>();
        task!.Title.ShouldBe("Versioned create");
    }

    private sealed record VersionedTaskResponse(Guid Id, string Title, string Status);
}
