using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// Verifies that the global OpenAPI request-body validation filter (ADR-030)
/// enforces JSON Schema constraints declared in the contract that ASP.NET model
/// binding doesn't enforce on primitives (`minimum`, `enum`, item-type, ...).
/// Targets POST /api/data/import as the most constraint-dense endpoint.
/// </summary>
[Trait("Category", "Api")]
public sealed class OpenApiRequestBodyValidationFilterTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public OpenApiRequestBodyValidationFilterTests()
    {
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", AuthTestFixture.GetTokenFor(Guid.NewGuid()));
    }

    public void Dispose() => _factory.Dispose();

    private const string ValidEnvelope = """
        {
          "meta": {"exportedAt": "2026-05-14T00:00:00Z", "format": "json", "scope": "all", "recordCount": 0},
          "tasks": [], "quests": [], "epics": [], "sagas": [],
          "xpHistory": [], "level": {}, "skillTreeProgress": [], "titlesEarned": [],
          "weeklyReviews": [], "timelineEvents": [], "insightCards": [],
          "settings": {"dataPrivacy": null, "notifications": null, "sync": null, "leaderboard": null}
        }
        """;

    private async Task<HttpResponseMessage> ImportAsync(string body)
    {
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        return await _client.PostAsync("/api/data/import", content);
    }

    [Fact]
    public async Task Should_Reject_When_MetaRecordCountIsNegative()
    {
        string body = ValidEnvelope.Replace("\"recordCount\": 0", "\"recordCount\": -1");
        HttpResponseMessage response = await ImportAsync(body);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        JsonElement json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("title").GetString()!.ShouldContain("OpenAPI");
        json.GetProperty("errors").TryGetProperty("meta.recordCount", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Reject_When_MetaScopeIsNotInEnum()
    {
        string body = ValidEnvelope.Replace("\"scope\": \"all\"", "\"scope\": \"AAA\"");
        HttpResponseMessage response = await ImportAsync(body);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Reject_When_SagasItemIsNotAnObject()
    {
        string body = ValidEnvelope.Replace("\"sagas\": []", "\"sagas\": [[null, null]]");
        HttpResponseMessage response = await ImportAsync(body);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Reject_When_LevelCurrentIsBelowMinimum()
    {
        string body = ValidEnvelope.Replace("\"level\": {}", "\"level\": {\"current\": 0}");
        HttpResponseMessage response = await ImportAsync(body);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Accept_When_EnvelopeIsSchemaCompliant()
    {
        HttpResponseMessage response = await ImportAsync(ValidEnvelope);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Skip_When_RequestHasNoJsonBody()
    {
        // GET /api/tasks has no body — filter must let it through unmodified.
        HttpResponseMessage response = await _client.GetAsync("/api/tasks");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
