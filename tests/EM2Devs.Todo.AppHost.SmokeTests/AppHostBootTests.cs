using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.AppHost.SmokeTests;

/// <summary>
/// Smoke tests that verify the Aspire AppHost assembles and boots correctly.
/// Catches assembly binding failures (e.g. TypeLoadException), missing
/// configuration, and resource wiring issues before they surface at deploy time.
/// </summary>
[Trait("Category", "Smoke")]
public sealed class AppHostBootTests
{
    [Fact]
    public async Task Should_BuildWithoutErrors_When_AppHostIsAssembled()
    {
        // When — build the distributed application
        // This catches TypeLoadException, assembly version mismatches,
        // and resource registration failures at test time.
        IDistributedApplicationTestingBuilder builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.EM2Devs_Todo_AppHost>();

        await using DistributedApplication app = await builder.BuildAsync();

        // Then — the app built successfully (no TypeLoadException or binding failures)
        app.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_RegisterExpectedResources_When_AppHostIsBuilt()
    {
        // Given
        IDistributedApplicationTestingBuilder builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.EM2Devs_Todo_AppHost>();

        // When
        await using DistributedApplication app = await builder.BuildAsync();

        // Then — verify expected resources are registered
        IResource? apiResource = builder.Resources.FirstOrDefault(r => r.Name == "api");
        IResource? postgresResource = builder.Resources.FirstOrDefault(r => r.Name == "postgres");
        IResource? redisResource = builder.Resources.FirstOrDefault(r => r.Name == "redis");
        IResource? tododbResource = builder.Resources.FirstOrDefault(r => r.Name == "tododb");

        apiResource.ShouldNotBeNull();
        postgresResource.ShouldNotBeNull();
        redisResource.ShouldNotBeNull();
        tododbResource.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_StartSuccessfully_When_AppHostIsLaunched()
    {
        // Given — build the distributed application
        IDistributedApplicationTestingBuilder builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.EM2Devs_Todo_AppHost>();

        await using DistributedApplication app = await builder.BuildAsync();

        // When — start the app (triggers lifecycle hooks including dashboard setup)
        // This catches configuration errors (missing env vars, dashboard config)
        // that only surface at startup, not at build time.
        await app.StartAsync();

        // Then — no exception means the host started successfully
        app.Services.ShouldNotBeNull();
    }
}
