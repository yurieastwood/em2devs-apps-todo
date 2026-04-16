using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// Verifies the GET /api/profile/estimation-bias endpoint auth-gates correctly
/// and returns a well-shaped <c>EstimationBiasResponse</c> for a freshly-authenticated
/// user (no history → NotEnoughData + neutral 1.0 bias).
/// </summary>
[Trait("Category", "Api")]
public sealed class EstimationBiasEndpointTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _authedClient;
    private readonly HttpClient _anonymousClient;

    public EstimationBiasEndpointTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _authedClient = _factory.CreateClient().Authenticated();
        _anonymousClient = _factory.CreateClient();
    }

    public void Dispose()
    {
        _authedClient.Dispose();
        _anonymousClient.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Should_ReturnDashboard_When_AuthenticatedUserRequestsAccuracy()
    {
        HttpResponseMessage response = await _authedClient.GetAsync("/api/profile/estimation-accuracy");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string json = await response.Content.ReadAsStringAsync();
        json.ShouldContain("\"perCategory\"");
        json.ShouldContain("\"accuracyTrend\"");
    }

    [Fact]
    public async Task Should_Return401_When_Unauthenticated()
    {
        HttpResponseMessage response = await _anonymousClient.GetAsync("/api/profile/estimation-bias");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Return200WithNotEnoughData_When_UserHasNoHistory()
    {
        HttpResponseMessage response = await _authedClient.GetAsync("/api/profile/estimation-bias");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        EstimationBiasResponse? body = await response.Content.ReadFromJsonAsync<EstimationBiasResponse>();
        body.ShouldNotBeNull();
        body.CalibrationState.ShouldBe("NotEnoughData");
        body.BiasFactor.ShouldBe(1.0);
        body.SampleSize.ShouldBe(0);
    }

    private sealed record EstimationBiasResponse(
        double BiasFactor,
        int SampleSize,
        string CalibrationState);
}
