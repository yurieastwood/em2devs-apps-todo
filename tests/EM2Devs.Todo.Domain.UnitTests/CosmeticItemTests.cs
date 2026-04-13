using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for CosmeticItem value object.
/// Maps to: docs/features/progression/seasons.feature
/// Rule: "Seasonal cosmetics are limited to the season and cannot be earned later"
/// </summary>
public sealed class CosmeticItemTests
{
    private static readonly DateOnly _seasonStart = new(2026, 1, 1);
    private static readonly DateOnly _seasonEnd = new(2026, 3, 31);
    private static readonly DateOnly _midSeason = new(2026, 2, 15);

    // --- Creation ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateCosmeticItem_When_ValidParameters()
    {
        // Given / When
        var item = new CosmeticItem("Crystal Compass", "Season of the Explorer", CosmeticRarity.Rare, 5);

        // Then
        item.Name.ShouldBe("Crystal Compass");
        item.SeasonName.ShouldBe("Season of the Explorer");
        item.Rarity.ShouldBe(CosmeticRarity.Rare);
        item.RequiredStage.ShouldBe(5);
        item.IsSeasonExclusive.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SetSeasonExclusive_When_Specified()
    {
        // Given / When
        var item = new CosmeticItem("Generic Badge", "Season 1", CosmeticRarity.Common, 1, false);

        // Then
        item.IsSeasonExclusive.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NameIsEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new CosmeticItem("", "Season 1", CosmeticRarity.Common, 1));
        ex.Message.ShouldContain("name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NameIsWhitespace()
    {
        var ex = Should.Throw<DomainException>(
            () => new CosmeticItem("  ", "Season 1", CosmeticRarity.Common, 1));
        ex.Message.ShouldContain("name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SeasonNameIsEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new CosmeticItem("Badge", "", CosmeticRarity.Common, 1));
        ex.Message.ShouldContain("season name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RequiredStageIsZero()
    {
        var ex = Should.Throw<DomainException>(
            () => new CosmeticItem("Badge", "Season 1", CosmeticRarity.Common, 0));
        ex.Message.ShouldContain("Required stage must be between 1 and");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RequiredStageExceedsMax()
    {
        var ex = Should.Throw<DomainException>(
            () => new CosmeticItem("Badge", "Season 1", CosmeticRarity.Common, SeasonalQuestLine.MaxStages + 1));
        ex.Message.ShouldContain("Required stage must be between 1 and");
    }

    // --- Scenario: Earn a seasonal cosmetic ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeEarnable_When_SeasonIsActiveAndMatches()
    {
        // Given
        var season = new Season("Season of the Explorer", "Exploration", _seasonStart, _seasonEnd);
        var item = new CosmeticItem("Crystal Compass", "Season of the Explorer", CosmeticRarity.Rare, 5);

        // When / Then
        item.IsEarnable(season, _midSeason).ShouldBeTrue();
    }

    // --- Scenario: Seasonal cosmetic unavailable after season ends ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEarnable_When_SeasonHasEnded()
    {
        // Given
        var season = new Season("Season of the Architect", "Architecture", _seasonStart, _seasonEnd);
        var item = new CosmeticItem("Blueprint Frame", "Season of the Architect", CosmeticRarity.Epic, 5);

        // When / Then
        item.IsEarnable(season, _seasonEnd.AddDays(1)).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEarnable_When_SeasonNameDoesNotMatch()
    {
        // Given
        var season = new Season("Season of the Explorer", "Exploration", _seasonStart, _seasonEnd);
        var item = new CosmeticItem("Blueprint Frame", "Season of the Architect", CosmeticRarity.Epic, 5);

        // When / Then
        item.IsEarnable(season, _midSeason).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEarnable_When_BeforeSeasonStarts()
    {
        // Given
        var season = new Season("Season of the Explorer", "Exploration", _seasonStart, _seasonEnd);
        var item = new CosmeticItem("Crystal Compass", "Season of the Explorer", CosmeticRarity.Rare, 5);

        // When / Then
        item.IsEarnable(season, _seasonStart.AddDays(-1)).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_SeasonIsNull()
    {
        // Given
        var item = new CosmeticItem("Badge", "Season 1", CosmeticRarity.Common, 1);

        // When / Then
        Should.Throw<ArgumentNullException>(() => item.IsEarnable(null!, _midSeason));
    }
}
