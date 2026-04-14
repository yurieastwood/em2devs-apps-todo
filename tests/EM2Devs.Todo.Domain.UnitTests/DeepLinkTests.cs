using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for DeepLink: tap a notification to navigate to the relevant item.
/// </summary>
public sealed class DeepLinkTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateLink_With_TrimmedValues()
    {
        DeepLink link = DeepLink.Create("  task  ", "  abc-123  ");
        link.EntityType.ShouldBe("task");
        link.EntityId.ShouldBe("abc-123");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnPath_When_ConvertedToPath()
    {
        DeepLink link = DeepLink.Create("task", "abc-123");
        link.ToPath().ShouldBe("/task/abc-123");
    }

    [Theory]
    [InlineData("", "id")]
    [InlineData("  ", "id")]
    [InlineData("task", "")]
    [InlineData("task", "   ")]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ArgumentsMissing(string entityType, string entityId)
    {
        Should.Throw<DomainException>(() => DeepLink.Create(entityType, entityId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_EntityTypeNull()
    {
        DomainException ex = Should.Throw<DomainException>(() => DeepLink.Create(null!, "id"));
        ex.Message.ShouldContain("entity type");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_EntityIdNull()
    {
        DomainException ex = Should.Throw<DomainException>(() => DeepLink.Create("task", null!));
        ex.Message.ShouldContain("entity id");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeEqual_When_SameEntityTypeAndId()
    {
        DeepLink a = DeepLink.Create("task", "123");
        DeepLink b = DeepLink.Create("task", "123");
        a.ShouldBe(b);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqual_When_DifferentEntityIds()
    {
        DeepLink a = DeepLink.Create("task", "123");
        DeepLink b = DeepLink.Create("task", "456");
        a.ShouldNotBe(b);
    }
}
