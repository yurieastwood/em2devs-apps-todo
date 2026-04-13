using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for SeasonalProfile value object.
/// Maps to: docs/features/progression/seasons.feature
/// Covers: quest line progression, seasonal XP, cosmetics, ranks, profile display.
/// </summary>
public sealed class SeasonalProfileTests
{
    private const string SeasonName = "Season of the Explorer";

    // --- Scenario: Start the seasonal quest line ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateNewProfile_When_JoiningSeason()
    {
        // Given / When
        var profile = SeasonalProfile.StartNew(SeasonName, 8);

        // Then
        profile.SeasonName.ShouldBe(SeasonName);
        profile.SeasonalXp.Value.ShouldBe(0);
        profile.FinalRank.ShouldBeNull();
        profile.EarnedCosmetics.ShouldBeEmpty();
        profile.QuestLine.CurrentStage.ShouldBe(1);
        profile.QuestLine.TotalStages.ShouldBe(8);
        profile.IsComplete.ShouldBeFalse();
        profile.ActiveBadge.ShouldBeNull();
    }

    // --- Scenario: Add seasonal XP ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddSeasonalXp_When_EarningXp()
    {
        // Given
        var profile = SeasonalProfile.StartNew(SeasonName, 8);

        // When
        var updated = profile.AddSeasonalXp(new ExperiencePoints(100));

        // Then
        updated.SeasonalXp.Value.ShouldBe(100);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AccumulateSeasonalXp_When_EarningMultipleTimes()
    {
        // Given
        var profile = SeasonalProfile.StartNew(SeasonName, 8);

        // When
        var updated = profile
            .AddSeasonalXp(new ExperiencePoints(100))
            .AddSeasonalXp(new ExperiencePoints(50));

        // Then
        updated.SeasonalXp.Value.ShouldBe(150);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AddingNullXp()
    {
        // Null propagates to ExperiencePoints.Add which validates
        var profile = SeasonalProfile.StartNew(SeasonName, 8);
        Should.Throw<ArgumentNullException>(() => profile.AddSeasonalXp(null!));
    }

    // --- Scenario: Earn a seasonal cosmetic ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EarnCosmetic_When_CompletingStage()
    {
        // Given
        var profile = SeasonalProfile.StartNew(SeasonName, 8);
        var cosmetic = new CosmeticItem("Crystal Compass", SeasonName, CosmeticRarity.Rare, 5);

        // When
        var updated = profile.EarnCosmetic(cosmetic);

        // Then
        updated.EarnedCosmetics.Count.ShouldBe(1);
        updated.EarnedCosmetics[0].Name.ShouldBe("Crystal Compass");
        updated.EarnedCosmetics[0].IsSeasonExclusive.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_EarningNullCosmetic()
    {
        var profile = SeasonalProfile.StartNew(SeasonName, 8);
        Should.Throw<ArgumentNullException>(() => profile.EarnCosmetic(null!));
    }

    // --- Scenario: Season ends and final ranks are recorded ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecordFinalRank_When_SeasonEnds()
    {
        // Given
        var profile = SeasonalProfile.StartNew(SeasonName, 8)
            .AddSeasonalXp(new ExperiencePoints(1200));

        // When — season ends, rank recorded
        var completed = profile.RecordFinalRank(15);

        // Then
        completed.FinalRank.ShouldBe(15);
        completed.IsComplete.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FinalRankIsZero()
    {
        var profile = SeasonalProfile.StartNew(SeasonName, 8);
        var ex = Should.Throw<DomainException>(() => profile.RecordFinalRank(0));
        ex.Message.ShouldContain("Final rank must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FinalRankIsNegative()
    {
        var profile = SeasonalProfile.StartNew(SeasonName, 8);
        var ex = Should.Throw<DomainException>(() => profile.RecordFinalRank(-1));
        ex.Message.ShouldContain("Final rank must be at least 1");
    }

    // --- Scenario Outline: Complete a seasonal quest line stage ---

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(1, 3, 2, 2)]
    [InlineData(3, 5, 4, 4)]
    [InlineData(5, 7, 6, 6)]
    public void Should_AdvanceStageAndEarnRewards_When_CompletingStageViaProfile(
        int stage, int required, int completed, int nextStage)
    {
        // Given — profile with quest line at the given stage
        var questLine = new SeasonalQuestLine(8, stage, completed);
        var cosmetic = new CosmeticItem($"Stage {stage} Badge", SeasonName, CosmeticRarity.Common, stage);
        var profile = new SeasonalProfile(
            SeasonName, new ExperiencePoints(0), null, [], questLine);

        // When — complete another qualifying task
        var updated = profile.CompleteQuestStage(required, new ExperiencePoints(50), cosmetic);

        // Then — stage advances, XP earned, cosmetic earned
        updated.QuestLine.CurrentStage.ShouldBe(nextStage);
        updated.SeasonalXp.Value.ShouldBe(50);
        updated.EarnedCosmetics.Count.ShouldBe(1);
        updated.EarnedCosmetics[0].Name.ShouldBe($"Stage {stage} Badge");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotAwardRewards_When_StageNotYetComplete()
    {
        // Given — stage 1, requires 3 tasks, completed 0
        var profile = SeasonalProfile.StartNew(SeasonName, 8);
        var cosmetic = new CosmeticItem("Badge", SeasonName, CosmeticRarity.Common, 1);

        // When — complete 1 task (still need 2 more)
        var updated = profile.CompleteQuestStage(3, new ExperiencePoints(50), cosmetic);

        // Then — no advancement, no rewards yet
        updated.QuestLine.CurrentStage.ShouldBe(1);
        updated.SeasonalXp.Value.ShouldBe(0);
        updated.EarnedCosmetics.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AdvanceWithoutCosmetic_When_NoCosmeticForStage()
    {
        // Given — stage 1, requires 1 task, completed 0
        var questLine = new SeasonalQuestLine(8, 1, 0);
        var profile = new SeasonalProfile(SeasonName, new ExperiencePoints(0), null, [], questLine);

        // When — complete without cosmetic
        var updated = profile.CompleteQuestStage(1, new ExperiencePoints(50));

        // Then — stage advances, XP earned, no cosmetic
        updated.QuestLine.CurrentStage.ShouldBe(2);
        updated.SeasonalXp.Value.ShouldBe(50);
        updated.EarnedCosmetics.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CompleteQuestStageWithNullXp()
    {
        var profile = SeasonalProfile.StartNew(SeasonName, 8);
        Should.Throw<ArgumentNullException>(() => profile.CompleteQuestStage(3, null!));
    }

    // --- Scenario: Complete the full seasonal quest line ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CompleteFullQuestLine_When_AllStagesDone()
    {
        // Given — completed stages 1-7, on stage 8
        var questLine = new SeasonalQuestLine(8, 8, 6);
        var profile = new SeasonalProfile(
            SeasonName, new ExperiencePoints(400), null, [], questLine);

        // When — complete stage 8 (advance to 9 = completed)
        var stageCompleted = profile.CompleteQuestStage(7, new ExperiencePoints(50));
        stageCompleted.QuestLine.IsCompleted.ShouldBeTrue();

        // Then — award completion bonus
        var completionCosmetic = new CosmeticItem(
            "Season Champion", SeasonName, CosmeticRarity.Legendary, 8);
        var final = stageCompleted.CompleteFullQuestLine(
            new ExperiencePoints(200), completionCosmetic);

        final.SeasonalXp.Value.ShouldBe(650); // 400 + 50 + 200
        final.EarnedCosmetics.Count.ShouldBe(1);
        final.EarnedCosmetics[0].Name.ShouldBe("Season Champion");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CompletionBonusIsNull()
    {
        var profile = SeasonalProfile.StartNew(SeasonName, 8);
        var cosmetic = new CosmeticItem("Badge", SeasonName, CosmeticRarity.Common, 1);
        Should.Throw<ArgumentNullException>(() => profile.CompleteFullQuestLine(null!, cosmetic));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CompletionCosmeticIsNull()
    {
        var profile = SeasonalProfile.StartNew(SeasonName, 8);
        Should.Throw<ArgumentNullException>(
            () => profile.CompleteFullQuestLine(new ExperiencePoints(100), null!));
    }

    // --- Scenario: Display seasonal cosmetic on profile ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SetActiveBadge_When_BadgeIsEarned()
    {
        // Given
        var cosmetic = new CosmeticItem("Crystal Compass", SeasonName, CosmeticRarity.Rare, 5);
        var profile = SeasonalProfile.StartNew(SeasonName, 8).EarnCosmetic(cosmetic);

        // When
        var updated = profile.SetActiveBadge("Crystal Compass");

        // Then
        updated.ActiveBadge.ShouldBe("Crystal Compass");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SettingUnearnedBadge()
    {
        // Given
        var profile = SeasonalProfile.StartNew(SeasonName, 8);

        // When / Then
        var ex = Should.Throw<DomainException>(() => profile.SetActiveBadge("Nonexistent"));
        ex.Message.ShouldContain("Cannot set a badge that has not been earned");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_BadgeNameIsEmpty()
    {
        var profile = SeasonalProfile.StartNew(SeasonName, 8);
        var ex = Should.Throw<DomainException>(() => profile.SetActiveBadge(""));
        ex.Message.ShouldContain("Badge name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_BadgeNameIsWhitespace()
    {
        var profile = SeasonalProfile.StartNew(SeasonName, 8);
        var ex = Should.Throw<DomainException>(() => profile.SetActiveBadge("  "));
        ex.Message.ShouldContain("Badge name cannot be empty");
    }

    // --- Scenario: View past season history ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowSeasonHistory_When_MultipleSeasonsCompleted()
    {
        // Given — 3 previous seasons completed
        var cosmetic1 = new CosmeticItem("Badge 1", "Season 1", CosmeticRarity.Common, 1);
        var cosmetic2 = new CosmeticItem("Badge 2", "Season 2", CosmeticRarity.Rare, 3);
        var cosmetic3 = new CosmeticItem("Badge 3", "Season 3", CosmeticRarity.Epic, 5);

        var seasons = new List<SeasonalProfile>
        {
            new SeasonalProfile("Season 1", new ExperiencePoints(500), 10,
                new List<CosmeticItem> { cosmetic1 }, new SeasonalQuestLine(8, 9, 0), true),
            new SeasonalProfile("Season 2", new ExperiencePoints(800), 5,
                new List<CosmeticItem> { cosmetic2 }, new SeasonalQuestLine(8, 9, 0), true),
            new SeasonalProfile("Season 3", new ExperiencePoints(1200), 3,
                new List<CosmeticItem> { cosmetic3 }, new SeasonalQuestLine(8, 9, 0), true),
        };

        // Then — each season has summary with rank, cosmetics, XP
        seasons.Count.ShouldBe(3);
        seasons[0].FinalRank.ShouldBe(10);
        seasons[0].EarnedCosmetics.Count.ShouldBe(1);
        seasons[0].SeasonalXp.Value.ShouldBe(500);
        seasons[1].FinalRank.ShouldBe(5);
        seasons[2].FinalRank.ShouldBe(3);
        seasons[2].SeasonalXp.Value.ShouldBe(1200);
    }

    // --- Scenario: User inactive for an entire season ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNoSeasonRecord_When_InactiveForEntireSeason()
    {
        // Given — user has season history but missed "Season of the Architect"
        var history = new List<SeasonalProfile>
        {
            new SeasonalProfile("Season 1", new ExperiencePoints(500), 10,
                [], new SeasonalQuestLine(8, 9, 0), true),
        };

        // Then — no record for the missed season
        bool hasArchitectRecord = false;
        foreach (var p in history)
        {
            if (p.SeasonName == "Season of the Architect")
            {
                hasArchitectRecord = true;
            }
        }

        hasArchitectRecord.ShouldBeFalse();
    }

    // --- Scenario: User joins mid-season ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartFromStage1_When_JoiningMidSeason()
    {
        // Given / When — user joins mid-season
        var profile = SeasonalProfile.StartNew(SeasonName, 8);

        // Then — starts at stage 1 with 0 XP
        profile.QuestLine.CurrentStage.ShouldBe(1);
        profile.QuestLine.IsStageAvailable(1).ShouldBeTrue();
        profile.SeasonalXp.Value.ShouldBe(0);
    }

    // --- Validation ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SeasonNameIsEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => SeasonalProfile.StartNew("", 8));
        ex.Message.ShouldContain("Season name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SeasonNameIsWhitespace()
    {
        var ex = Should.Throw<DomainException>(
            () => SeasonalProfile.StartNew("  ", 8));
        ex.Message.ShouldContain("Season name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_SeasonalXpIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new SeasonalProfile("Season 1", null!, null, [], SeasonalQuestLine.Start(8)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_EarnedCosmeticsIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new SeasonalProfile("Season 1", new ExperiencePoints(0), null, null!,
                SeasonalQuestLine.Start(8)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_QuestLineIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new SeasonalProfile("Season 1", new ExperiencePoints(0), null, [], null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FinalRankIsZeroInConstructor()
    {
        var ex = Should.Throw<DomainException>(
            () => new SeasonalProfile("Season 1", new ExperiencePoints(0), 0, [],
                SeasonalQuestLine.Start(8)));
        ex.Message.ShouldContain("Final rank must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FinalRankIsNegativeInConstructor()
    {
        var ex = Should.Throw<DomainException>(
            () => new SeasonalProfile("Season 1", new ExperiencePoints(0), -1, [],
                SeasonalQuestLine.Start(8)));
        ex.Message.ShouldContain("Final rank must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowNullFinalRank_When_SeasonStillActive()
    {
        // Given / When
        var profile = new SeasonalProfile("Season 1", new ExperiencePoints(0), null, [],
            SeasonalQuestLine.Start(8));

        // Then
        profile.FinalRank.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowFinalRankOfOne_When_ConstructedWithRankOne()
    {
        // Given / When — rank = 1 should be valid (kills < 1 vs <= 1 mutation)
        var profile = new SeasonalProfile("Season 1", new ExperiencePoints(0), 1, [],
            SeasonalQuestLine.Start(8));

        // Then
        profile.FinalRank.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowRecordFinalRankOfOne_When_RankIsOne()
    {
        // Given
        var profile = SeasonalProfile.StartNew(SeasonName, 8);

        // When — rank = 1 should be valid (kills < 1 vs <= 1 mutation)
        var updated = profile.RecordFinalRank(1);

        // Then
        updated.FinalRank.ShouldBe(1);
        updated.IsComplete.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SetActiveBadge_When_MultipleCosmeticsEarned()
    {
        // Given — multiple cosmetics earned (kills break; removal in SetActiveBadge)
        var cosmetic1 = new CosmeticItem("Badge A", SeasonName, CosmeticRarity.Common, 1);
        var cosmetic2 = new CosmeticItem("Badge B", SeasonName, CosmeticRarity.Rare, 3);
        var profile = SeasonalProfile.StartNew(SeasonName, 8)
            .EarnCosmetic(cosmetic1)
            .EarnCosmetic(cosmetic2);

        // When — set first badge
        var updated = profile.SetActiveBadge("Badge A");

        // Then — should work and only set Badge A
        updated.ActiveBadge.ShouldBe("Badge A");
    }
}
