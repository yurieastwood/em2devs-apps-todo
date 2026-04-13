using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for WrappedSlide value object.
/// Maps to: docs/features/reflection/annual-wrapped.feature
/// </summary>
public sealed class WrappedSlideTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateSlide_When_ValidParametersProvided()
    {
        var slide = new WrappedSlide("Total tasks", "156 tasks", "counter");

        slide.Title.ShouldBe("Total tasks");
        slide.Metric.ShouldBe("156 tasks");
        slide.VisualizationType.ShouldBe("counter");
        slide.IsShareable.ShouldBeTrue();
        slide.IsExcludedFromShare.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenTitleEmpty_When_CreatingSlide()
    {
        Should.Throw<DomainException>(() => new WrappedSlide("", "data", "counter"))
            .Message.ShouldContain("title");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenTitleWhitespace_When_CreatingSlide()
    {
        Should.Throw<DomainException>(() => new WrappedSlide("  ", "data", "counter"))
            .Message.ShouldContain("title");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenTitleNull_When_CreatingSlide()
    {
        Should.Throw<DomainException>(() => new WrappedSlide(null!, "data", "counter"))
            .Message.ShouldContain("title");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenMetricEmpty_When_CreatingSlide()
    {
        Should.Throw<DomainException>(() => new WrappedSlide("Title", "", "counter"))
            .Message.ShouldContain("metric");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenMetricWhitespace_When_CreatingSlide()
    {
        Should.Throw<DomainException>(() => new WrappedSlide("Title", "  ", "counter"))
            .Message.ShouldContain("metric");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenMetricNull_When_CreatingSlide()
    {
        Should.Throw<DomainException>(() => new WrappedSlide("Title", null!, "counter"))
            .Message.ShouldContain("metric");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenVisualizationTypeEmpty_When_CreatingSlide()
    {
        Should.Throw<DomainException>(() => new WrappedSlide("Title", "data", ""))
            .Message.ShouldContain("visualization type");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenVisualizationTypeWhitespace_When_CreatingSlide()
    {
        Should.Throw<DomainException>(() => new WrappedSlide("Title", "data", "  "))
            .Message.ShouldContain("visualization type");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenVisualizationTypeNull_When_CreatingSlide()
    {
        Should.Throw<DomainException>(() => new WrappedSlide("Title", "data", null!))
            .Message.ShouldContain("visualization type");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeFromShare_When_UserExcludes()
    {
        var slide = new WrappedSlide("Title", "data", "counter");
        var excluded = slide.ExcludeFromShare();

        excluded.IsExcludedFromShare.ShouldBeTrue();
        excluded.Title.ShouldBe("Title");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeInShare_When_UserReIncludes()
    {
        var slide = new WrappedSlide("Title", "data", "counter");
        var excluded = slide.ExcludeFromShare();
        var included = excluded.IncludeInShare();

        included.IsExcludedFromShare.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EnableSharing_When_CallingEnableSharing()
    {
        var slide = new WrappedSlide("Title", "data", "counter");
        var excluded = slide.ExcludeFromShare();
        var enabled = excluded.EnableSharing();

        enabled.IsShareable.ShouldBeTrue();
        enabled.IsExcludedFromShare.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateEncouragingSlide_When_UsingFactory()
    {
        var slide = WrappedSlide.CreateEncouraging(
            "Quests completed",
            "No quests yet — your first quest awaits!",
            "counter");

        slide.Title.ShouldBe("Quests completed");
        slide.Metric.ShouldContain("No quests yet");
        slide.VisualizationType.ShouldBe("counter");
        slide.IsShareable.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeImmutable_When_ExcludingFromShare()
    {
        var original = new WrappedSlide("Title", "data", "counter");
        var excluded = original.ExcludeFromShare();

        original.IsExcludedFromShare.ShouldBeFalse();
        excluded.IsExcludedFromShare.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveAllProperties_When_ExcludingFromShare()
    {
        var original = new WrappedSlide("My Title", "My Metric", "chart");
        var excluded = original.ExcludeFromShare();

        excluded.Title.ShouldBe("My Title");
        excluded.Metric.ShouldBe("My Metric");
        excluded.VisualizationType.ShouldBe("chart");
        excluded.IsShareable.ShouldBeTrue();
    }
}
