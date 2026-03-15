using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for Title, TitleInventory value objects.
/// Maps to: docs/features/progression/titles-and-ranks.feature
/// Rule: "Titles are earned through sustained behaviour"
/// Rule: "Titles are permanently earned and never revoked"
/// </summary>
public sealed class TitleTests
{
    private static readonly DateOnly _today = new(2026, 3, 15);

    // --- Title ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateTitle_When_ValidTypeAndDate()
    {
        // Given / When
        var title = new Title(TitleType.EarlyBird, _today);

        // Then
        title.Type.ShouldBe(TitleType.EarlyBird);
        title.EarnedOn.ShouldBe(_today);
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(TitleType.EarlyBird, "Early Bird")]
    [InlineData(TitleType.MorningArchitect, "Morning Architect")]
    [InlineData(TitleType.NightOwl, "Night Owl")]
    [InlineData(TitleType.MarathonBuilder, "Marathon Builder")]
    [InlineData(TitleType.BossSlayer, "Boss Slayer")]
    [InlineData(TitleType.StreakMaster, "Streak Master")]
    [InlineData(TitleType.QuestCloser, "Quest Closer")]
    [InlineData(TitleType.ConsistentPlanner, "Consistent Planner")]
    [InlineData(TitleType.TeamAnchor, "Team Anchor")]
    public void Should_ReturnDisplayName_When_TitleTypeIsValid(
        TitleType type, string expectedName)
    {
        // Given / When
        string name = Title.DisplayName(type);

        // Then
        name.ShouldBe(expectedName);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentOutOfRange_When_DisplayNameTypeIsInvalid()
    {
        // Given / When / Then
        var ex = Should.Throw<ArgumentOutOfRangeException>(
            () => Title.DisplayName((TitleType)999));
        ex.Message.ShouldContain("Unknown title type");
    }

    // --- TitleInventory ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartEmpty_When_NewCollectionCreated()
    {
        // Given / When
        var collection = TitleInventory.Empty();

        // Then
        collection.EarnedTitles.ShouldBeEmpty();
        collection.ActiveTitle.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddTitle_When_Awarded()
    {
        // Given
        var collection = TitleInventory.Empty();
        var title = new Title(TitleType.BossSlayer, _today);

        // When
        var result = collection.AwardTitle(title);

        // Then
        result.EarnedTitles.Count.ShouldBe(1);
        result.HasTitle(TitleType.BossSlayer).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotDuplicate_When_AwardingSameTitleTwice()
    {
        // Given
        var collection = TitleInventory.Empty();
        var title = new Title(TitleType.EarlyBird, _today);

        // When
        var result = collection.AwardTitle(title).AwardTitle(title);

        // Then
        result.EarnedTitles.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RetainAllTitles_When_MultipleAwarded()
    {
        // Given
        var collection = TitleInventory.Empty();

        // When — award multiple titles
        var result = collection
            .AwardTitle(new Title(TitleType.EarlyBird, _today))
            .AwardTitle(new Title(TitleType.BossSlayer, _today))
            .AwardTitle(new Title(TitleType.StreakMaster, _today));

        // Then — all retained (titles are permanent)
        result.EarnedTitles.Count.ShouldBe(3);
        result.HasTitle(TitleType.EarlyBird).ShouldBeTrue();
        result.HasTitle(TitleType.BossSlayer).ShouldBeTrue();
        result.HasTitle(TitleType.StreakMaster).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SelectActiveTitle_When_TitleIsEarned()
    {
        // Given
        var collection = TitleInventory.Empty()
            .AwardTitle(new Title(TitleType.EarlyBird, _today))
            .AwardTitle(new Title(TitleType.BossSlayer, _today));

        // When
        var result = collection.SelectActiveTitle(TitleType.BossSlayer);

        // Then
        result.ActiveTitle.ShouldBe(TitleType.BossSlayer);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SelectingUnearnedTitle()
    {
        // Given
        var collection = TitleInventory.Empty()
            .AwardTitle(new Title(TitleType.EarlyBird, _today));

        // When / Then
        var ex = Should.Throw<DomainException>(
            () => collection.SelectActiveTitle(TitleType.NightOwl));
        ex.Message.ShouldContain("has not been earned");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RetainTitle_When_BehaviourChanges()
    {
        // Given — earned Early Bird through consistent morning completions
        var collection = TitleInventory.Empty()
            .AwardTitle(new Title(TitleType.EarlyBird, _today.AddDays(-30)));

        // When — behaviour changes (no morning tasks for weeks)
        // Titles are permanent — no method to revoke

        // Then — title still earned and selectable
        collection.HasTitle(TitleType.EarlyBird).ShouldBeTrue();
        var selected = collection.SelectActiveTitle(TitleType.EarlyBird);
        selected.ActiveTitle.ShouldBe(TitleType.EarlyBird);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveActiveTitle_When_NewTitleAwarded()
    {
        // Given — active title is Boss Slayer
        var collection = TitleInventory.Empty()
            .AwardTitle(new Title(TitleType.BossSlayer, _today))
            .SelectActiveTitle(TitleType.BossSlayer);

        // When — earn a new title
        var result = collection.AwardTitle(new Title(TitleType.NightOwl, _today));

        // Then — active title unchanged
        result.ActiveTitle.ShouldBe(TitleType.BossSlayer);
        result.EarnedTitles.Count.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ActiveTitleNotInEarnedList()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new TitleInventory([], TitleType.EarlyBird));
        ex.Message.ShouldContain("must be one of the earned titles");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AwardingNullTitle()
    {
        // Given
        var collection = TitleInventory.Empty();

        // When / Then
        Should.Throw<ArgumentNullException>(() => collection.AwardTitle(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreatingWithNullTitles()
    {
        // Given / When / Then
        Should.Throw<ArgumentNullException>(() => new TitleInventory(null!, null));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_CheckingUnearnedTitle()
    {
        // Given
        var collection = TitleInventory.Empty();

        // When / Then
        collection.HasTitle(TitleType.MarathonBuilder).ShouldBeFalse();
    }
}
