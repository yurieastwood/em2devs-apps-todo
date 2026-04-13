using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for Season value object.
/// Maps to: docs/features/progression/seasons.feature
/// Rule: "Seasons run quarterly and introduce themed content"
/// </summary>
public sealed class SeasonTests
{
    private static readonly DateOnly _seasonStart = new(2026, 1, 1);
    private static readonly DateOnly _seasonEnd = new(2026, 3, 31);
    private static readonly DateOnly _midSeason = new(2026, 2, 15);

    // --- Season creation ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateSeason_When_ValidParameters()
    {
        // Given / When
        var season = new Season("Season of the Architect", "Architecture", _seasonStart, _seasonEnd);

        // Then
        season.Name.ShouldBe("Season of the Architect");
        season.Theme.ShouldBe("Architecture");
        season.StartDate.ShouldBe(_seasonStart);
        season.EndDate.ShouldBe(_seasonEnd);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateSeasonWithCosmetics_When_CosmeticsProvided()
    {
        // Given
        var cosmetics = new List<CosmeticItem>
        {
            new("Crystal Compass", "Season of the Explorer", CosmeticRarity.Rare, 5)
        };

        // When
        var season = new Season("Season of the Explorer", "Exploration", _seasonStart, _seasonEnd, cosmetics);

        // Then
        season.AvailableCosmetics.Count.ShouldBe(1);
        season.AvailableCosmetics[0].Name.ShouldBe("Crystal Compass");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveEmptyCosmetics_When_CreatedWithoutCosmetics()
    {
        // Given / When
        var season = new Season("Test Season", "Theme", _seasonStart, _seasonEnd);

        // Then
        season.AvailableCosmetics.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeActive_When_TodayIsWithinDateRange()
    {
        // Given
        var season = new Season("Test Season", "Theme", _seasonStart, _seasonEnd);

        // When / Then
        season.IsActive(_midSeason).ShouldBeTrue();
        season.IsActive(_seasonStart).ShouldBeTrue();
        season.IsActive(_seasonEnd).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeActive_When_TodayIsOutsideDateRange()
    {
        // Given
        var season = new Season("Test Season", "Theme", _seasonStart, _seasonEnd);

        // When / Then
        season.IsActive(_seasonStart.AddDays(-1)).ShouldBeFalse();
        season.IsActive(_seasonEnd.AddDays(1)).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnDaysRemaining_When_SeasonIsActive()
    {
        // Given
        var season = new Season("Test Season", "Theme", _seasonStart, _seasonEnd);

        // When
        int remaining = season.DaysRemaining(_midSeason);

        // Then
        remaining.ShouldBe(_seasonEnd.DayNumber - _midSeason.DayNumber);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroDaysRemaining_When_SeasonIsNotActive()
    {
        // Given
        var season = new Season("Test Season", "Theme", _seasonStart, _seasonEnd);

        // When
        int remaining = season.DaysRemaining(_seasonEnd.AddDays(1));

        // Then
        remaining.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroDaysRemaining_When_BeforeSeasonStart()
    {
        // Given
        var season = new Season("Test Season", "Theme", _seasonStart, _seasonEnd);

        // When
        int remaining = season.DaysRemaining(_seasonStart.AddDays(-1));

        // Then
        remaining.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportHasEnded_When_TodayIsAfterEndDate()
    {
        // Given
        var season = new Season("Test Season", "Theme", _seasonStart, _seasonEnd);

        // When / Then
        season.HasEnded(_seasonEnd.AddDays(1)).ShouldBeTrue();
        season.HasEnded(_seasonEnd).ShouldBeFalse();
        season.HasEnded(_midSeason).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NameIsEmpty()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new Season("", "Theme", _seasonStart, _seasonEnd));
        ex.Message.ShouldContain("name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ThemeIsEmpty()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new Season("Name", "", _seasonStart, _seasonEnd));
        ex.Message.ShouldContain("theme cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EndDateBeforeStartDate()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new Season("Name", "Theme", _seasonEnd, _seasonStart));
        ex.Message.ShouldContain("end date must be after start date");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EndDateEqualsStartDate()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new Season("Name", "Theme", _seasonStart, _seasonStart));
        ex.Message.ShouldContain("end date must be after start date");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CosmeticsIsNull()
    {
        // Given / When / Then
        Should.Throw<ArgumentNullException>(
            () => new Season("Name", "Theme", _seasonStart, _seasonEnd, null!));
    }

    // --- Scenario: New season begins ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TransitionToNewSeason_When_CurrentSeasonEnds()
    {
        // Given
        var currentSeason = new Season("Season of the Architect", "Architecture", _seasonStart, _seasonEnd);
        var newSeasonEnd = new DateOnly(2026, 6, 30);
        var newCosmetics = new List<CosmeticItem>
        {
            new("Explorer Badge", "Season of the Explorer", CosmeticRarity.Rare, 3)
        };

        // When
        var newSeason = currentSeason.TransitionTo(
            "Season of the Explorer", "Exploration", newSeasonEnd, newCosmetics);

        // Then
        newSeason.Name.ShouldBe("Season of the Explorer");
        newSeason.Theme.ShouldBe("Exploration");
        newSeason.StartDate.ShouldBe(_seasonEnd.AddDays(1));
        newSeason.EndDate.ShouldBe(newSeasonEnd);
        newSeason.AvailableCosmetics.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ValidateTransition_When_NextSeasonStartsImmediatelyAfter()
    {
        // Given
        var currentSeason = new Season("Season 1", "Theme 1", _seasonStart, _seasonEnd);
        var nextSeason = new Season("Season 2", "Theme 2", _seasonEnd.AddDays(1), new DateOnly(2026, 6, 30));

        // When / Then
        currentSeason.CanTransitionTo(nextSeason).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RejectTransition_When_GapBetweenSeasons()
    {
        // Given
        var currentSeason = new Season("Season 1", "Theme 1", _seasonStart, _seasonEnd);
        var nextSeason = new Season("Season 2", "Theme 2", _seasonEnd.AddDays(2), new DateOnly(2026, 6, 30));

        // When / Then
        currentSeason.CanTransitionTo(nextSeason).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_TransitionToNullSeason()
    {
        // Given
        var season = new Season("Season 1", "Theme 1", _seasonStart, _seasonEnd);

        // When / Then
        Should.Throw<ArgumentNullException>(() => season.CanTransitionTo(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_TransitionToWithNullCosmetics()
    {
        // Given
        var season = new Season("Season 1", "Theme 1", _seasonStart, _seasonEnd);

        // When / Then
        Should.Throw<ArgumentNullException>(
            () => season.TransitionTo("Season 2", "Theme 2", new DateOnly(2026, 6, 30), null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PassCosmeticsToNewSeason_When_TransitionTo()
    {
        // Given
        var season = new Season("Season 1", "Theme 1", _seasonStart, _seasonEnd);
        var cosmetics = new List<CosmeticItem>
        {
            new("Badge", "Season 2", CosmeticRarity.Common, 1)
        };

        // When
        var newSeason = season.TransitionTo("Season 2", "Theme 2", new DateOnly(2026, 6, 30), cosmetics);

        // Then
        newSeason.AvailableCosmetics.Count.ShouldBe(1);
        newSeason.AvailableCosmetics[0].Name.ShouldBe("Badge");
    }

    // --- Scenario: View current season details ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowSeasonDetails_When_ViewingCurrentSeason()
    {
        // Given
        var cosmetics = new List<CosmeticItem>
        {
            new("Crystal Compass", "Season of the Explorer", CosmeticRarity.Rare, 5),
            new("Explorer Frame", "Season of the Explorer", CosmeticRarity.Epic, 8)
        };
        var season = new Season(
            "Season of the Explorer", "Exploration", _seasonStart, _seasonEnd, cosmetics);

        // When / Then — can see name, theme, days remaining, cosmetics
        season.Name.ShouldBe("Season of the Explorer");
        season.Theme.ShouldBe("Exploration");
        season.DaysRemaining(_midSeason).ShouldBeGreaterThan(0);
        season.AvailableCosmetics.Count.ShouldBe(2);
    }

    // --- Scenario: Seasonal cosmetic unavailable after season ends ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnLockedCosmetics_When_SeasonEndsAndCosmeticsNotEarned()
    {
        // Given
        var blueprintFrame = new CosmeticItem(
            "Blueprint Frame", "Season of the Architect", CosmeticRarity.Epic, 5);
        var architectBadge = new CosmeticItem(
            "Architect Badge", "Season of the Architect", CosmeticRarity.Common, 1);
        var season = new Season(
            "Season of the Architect", "Architecture", _seasonStart, _seasonEnd,
            new List<CosmeticItem> { blueprintFrame, architectBadge });

        // Earned only the badge
        var earned = new List<CosmeticItem> { architectBadge };

        // When
        var locked = season.GetLockedCosmetics(earned);

        // Then — Blueprint Frame should be locked
        locked.Count.ShouldBe(1);
        locked[0].Name.ShouldBe("Blueprint Frame");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnCorrectLockedCount_When_MultipleEarnedAndUnearnedCosmetics()
    {
        // Given — 3 cosmetics, 2 earned (tests that break in inner loop doesn't skip checks)
        var c1 = new CosmeticItem("Badge A", "Season 1", CosmeticRarity.Common, 1);
        var c2 = new CosmeticItem("Badge B", "Season 1", CosmeticRarity.Rare, 3);
        var c3 = new CosmeticItem("Badge C", "Season 1", CosmeticRarity.Epic, 5);
        var season = new Season("Season 1", "Theme", _seasonStart, _seasonEnd,
            new List<CosmeticItem> { c1, c2, c3 });

        // Earned c1 and c3
        var earned = new List<CosmeticItem> { c1, c3 };

        // When
        var locked = season.GetLockedCosmetics(earned);

        // Then — only c2 locked
        locked.Count.ShouldBe(1);
        locked[0].Name.ShouldBe("Badge B");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNoLockedCosmetics_When_AllEarned()
    {
        // Given
        var cosmetic = new CosmeticItem("Badge", "Season 1", CosmeticRarity.Common, 1);
        var season = new Season("Season 1", "Theme", _seasonStart, _seasonEnd,
            new List<CosmeticItem> { cosmetic });

        // When
        var locked = season.GetLockedCosmetics(new List<CosmeticItem> { cosmetic });

        // Then
        locked.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_GetLockedCosmeticsWithNull()
    {
        // Given
        var season = new Season("Season 1", "Theme", _seasonStart, _seasonEnd);

        // When / Then
        Should.Throw<ArgumentNullException>(() => season.GetLockedCosmetics(null!));
    }
}
