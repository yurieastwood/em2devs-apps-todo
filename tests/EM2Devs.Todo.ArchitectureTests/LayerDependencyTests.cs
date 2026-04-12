using Shouldly;
using NetArchTest.Rules;
using Xunit;

namespace EM2Devs.Todo.ArchitectureTests;

/// <summary>
/// Gate 3: Architecture Fitness Tests
/// Enforces the dependency rules defined in ADR-022.
/// These tests run locally and in CI. The agent cannot violate
/// layer boundaries without failing this gate.
/// </summary>
public sealed class LayerDependencyTests
{
    private const string DomainNamespace = "EM2Devs.Todo.Domain";
    private const string ApplicationNamespace = "EM2Devs.Todo.Application";
    private const string InfrastructureNamespace = "EM2Devs.Todo.Infrastructure";
    private const string ApiNamespace = "EM2Devs.Todo.Api";

    [Fact]
    [Trait("Category", "Architecture")]
    public void Domain_Should_NotDependOn_Application()
    {
        var result = Types.InAssembly(typeof(Domain.ValueObjects.TaskId).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Domain must not reference Application (ADR-022). Violating types: {FormatFailingTypes(result)}");
    }

    [Fact]
    [Trait("Category", "Architecture")]
    public void Domain_Should_NotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Domain.ValueObjects.TaskId).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Domain must not reference Infrastructure (ADR-022). Violating types: {FormatFailingTypes(result)}");
    }

    [Fact]
    [Trait("Category", "Architecture")]
    public void Domain_Should_NotDependOn_Api()
    {
        var result = Types.InAssembly(typeof(Domain.ValueObjects.TaskId).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Domain must not reference Api (ADR-022). Violating types: {FormatFailingTypes(result)}");
    }

    [Fact]
    [Trait("Category", "Architecture")]
    public void Application_Should_NotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Application.Ports.ITaskRepository).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Application must not reference Infrastructure (ADR-022). Violating types: {FormatFailingTypes(result)}");
    }

    [Fact]
    [Trait("Category", "Architecture")]
    public void Application_Should_NotDependOn_Api()
    {
        var result = Types.InAssembly(typeof(Application.Ports.ITaskRepository).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Application must not reference Api (ADR-022). Violating types: {FormatFailingTypes(result)}");
    }

    [Fact]
    [Trait("Category", "Architecture")]
    public void Infrastructure_Should_NotDependOn_Api()
    {
        var result = Types.InAssembly(typeof(Infrastructure.Persistence.InMemoryTaskRepository).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Infrastructure must not reference Api (ADR-022). Violating types: {FormatFailingTypes(result)}");
    }

    private static string FormatFailingTypes(TestResult result)
    {
        if (result.FailingTypes is null || !result.FailingTypes.Any())
        {
            return "none";
        }

        return string.Join(", ", result.FailingTypes.Select(t => t.FullName));
    }
}
