using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Queries;

[Trait("Category", "Application")]
public sealed class GetPlayerProfileQueryTests
{
    private readonly IPlayerProfileRepository _repository = Substitute.For<IPlayerProfileRepository>();
    private readonly GetPlayerProfileQueryHandler _handler;

    public GetPlayerProfileQueryTests()
    {
        _handler = new GetPlayerProfileQueryHandler(_repository);
    }

    [Fact]
    public async Task Should_ReturnProjectedProfile_When_ProfileHasEarnedTitleAndActiveTitle()
    {
        // Given
        PlayerProfile profile = PlayerProfile.NewProfile(TestData.TestUserId);
        profile.AwardTitle(new Title(TitleType.EarlyBird, new DateOnly(2026, 1, 15)));
        profile.AwardTitle(new Title(TitleType.NightOwl, new DateOnly(2026, 2, 1)));
        profile.SelectActiveTitle(TitleType.EarlyBird);

        _repository.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(PlayerProfileProjection.Project(profile, lastBreakdown: null));

        // When
        Result<PlayerProfileReadModel> result = await _handler.Handle(new GetPlayerProfileQuery(), default);

        // Then
        result.IsSuccess.ShouldBeTrue();
        PlayerProfileReadModel read = result.Match(p => p, _ => throw new Xunit.Sdk.XunitException("expected success"));
        read.Titles.ShouldNotBeNull();
        read.Titles!.Earned.Count.ShouldBe(2);
        read.Titles.Earned.ShouldContain(t => t.Type == "EarlyBird" && t.DisplayName == "Early Bird");
        read.Titles.Active.ShouldBe("EarlyBird");
        read.Titles.Progress.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_SurfaceSkillTreeProgressAndPerks_When_TreeAdvancedToTierTwo()
    {
        // Given
        PlayerProfile profile = PlayerProfile.NewProfile(TestData.TestUserId);
        profile.DiscoverSkillTree(SkillTreeType.Scholar);
        // Advance Scholar to tier 2 by completing the required 30 tasks.
        for (int i = 0; i < 30; i++)
        {
            profile.RecordSkillTreeProgress(SkillTreeType.Scholar);
        }

        _repository.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(PlayerProfileProjection.Project(profile, lastBreakdown: null));

        // When
        Result<PlayerProfileReadModel> result = await _handler.Handle(new GetPlayerProfileQuery(), default);

        // Then
        PlayerProfileReadModel read = result.Match(p => p, _ => throw new Xunit.Sdk.XunitException("expected success"));
        read.SkillTrees.ShouldNotBeNull();
        SkillTreeReadModel scholar = read.SkillTrees!.Single(t => t.Type == "Scholar");
        scholar.Tier.ShouldBe(2);
        scholar.TasksCompletedInTier.ShouldBe(0);
        scholar.UnlockHint.ShouldBeNull();
        scholar.Perks.Count.ShouldBe(2); // tier 1 Tips + tier 2 Workflow
        scholar.Perks.ShouldContain(p => p.Tier == 1 && p.PerkType == "Tips");
        scholar.Perks.ShouldContain(p => p.Tier == 2 && p.PerkType == "Workflow");

        // Locked trees carry an unlock hint and no tier/perks.
        SkillTreeReadModel creator = read.SkillTrees.Single(t => t.Type == "Creator");
        creator.Tier.ShouldBeNull();
        creator.UnlockHint.ShouldNotBeNullOrEmpty();
        creator.Perks.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_LimitXpHistoryToLast20Entries_When_ProfileHasManyEntries()
    {
        // Given
        PlayerProfile profile = PlayerProfile.NewProfile(TestData.TestUserId);
        var day = new DateOnly(2026, 1, 1);
        for (int i = 1; i <= 25; i++)
        {
            profile.RecordXpEarning(day.AddDays(i), new ExperiencePoints(10), $"task-{i}");
        }

        _repository.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(PlayerProfileProjection.Project(profile, lastBreakdown: null));

        // When
        Result<PlayerProfileReadModel> result = await _handler.Handle(new GetPlayerProfileQuery(), default);

        // Then
        PlayerProfileReadModel read = result.Match(p => p, _ => throw new Xunit.Sdk.XunitException("expected success"));
        read.XpHistory.ShouldNotBeNull();
        read.XpHistory!.Count.ShouldBe(20);
        // Last entry should be the 25th task with cumulative total 250.
        read.XpHistory[^1].Source.ShouldBe("task-25");
        read.XpHistory[^1].CumulativeTotal.ShouldBe(250);
        // First retained entry should be task-6 (entries 6..25 = 20 items).
        read.XpHistory[0].Source.ShouldBe("task-6");
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_RequestIsNull()
    {
        await Should.ThrowAsync<ArgumentNullException>(
            async () => await _handler.Handle(null!, default));
    }
}
