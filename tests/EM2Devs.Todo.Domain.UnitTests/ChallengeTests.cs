using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for Challenge, ChallengeParticipant, ChallengeResult value objects.
/// Maps to: docs/features/social/challenge-mode.feature
/// </summary>
public sealed class ChallengeTests
{
    private static readonly DateTimeOffset _saturday = new(2026, 4, 11, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _sunday = new(2026, 4, 12, 23, 59, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _now = new(2026, 4, 11, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _beforeStart = new(2026, 4, 10, 23, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _afterEnd = new(2026, 4, 13, 0, 0, 0, TimeSpan.Zero);
    private static readonly Guid _userId1 = Guid.NewGuid();
    private static readonly Guid _userId2 = Guid.NewGuid();
    private static readonly Guid _userId3 = Guid.NewGuid();
    private static readonly Guid _userId4 = Guid.NewGuid();
    private static readonly GuildId _guildId = GuildId.New();

    private static Challenge CreateActiveGlobalChallenge()
    {
        return Challenge.CreateGlobal(
            "Weekend Warrior: Complete the most tasks this weekend",
            _saturday,
            _sunday,
            "Complete the most tasks",
            "Seasonal cosmetic + bonus XP");
    }

    private static Challenge CreateActiveGuildChallenge(Guid creatorId)
    {
        return Challenge.CreateGuild(
            "Boss Rush: Clear all Boss Tasks before Friday",
            _saturday,
            _sunday,
            "Complete the most Boss Tasks",
            "Seasonal cosmetic + bonus XP",
            _guildId,
            creatorId);
    }

    // --- Scenario 1: View available challenges ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowChallengeDetails_When_ChallengeIsCreated()
    {
        // Given / When
        var challenge = CreateActiveGlobalChallenge();

        // Then
        challenge.Title.ShouldBe("Weekend Warrior: Complete the most tasks this weekend");
        challenge.Type.ShouldBe(ChallengeType.Global);
        challenge.StartTime.ShouldBe(_saturday);
        challenge.EndTime.ShouldBe(_sunday);
        challenge.Objective.ShouldBe("Complete the most tasks");
        challenge.Reward.ShouldBe("Seasonal cosmetic + bonus XP");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IdentifyActiveChallenge_When_WithinTimeWindow()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge();

        // When / Then
        challenge.IsActive(_now).ShouldBeTrue();
        challenge.IsActive(_beforeStart).ShouldBeFalse();
        challenge.IsActive(_afterEnd).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowGuildChallenge_When_UserBelongsToGuild()
    {
        // Given / When
        var challenge = CreateActiveGuildChallenge(_userId1);

        // Then
        challenge.Type.ShouldBe(ChallengeType.Guild);
        challenge.GuildId.ShouldBe(_guildId);
    }

    // --- Scenario 2: Join a global challenge ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RegisterAsParticipant_When_JoiningActiveChallenge()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge();

        // When
        var joined = challenge.Join(_userId1, _now);

        // Then
        joined.Participants.Count.ShouldBe(1);
        joined.Participants[0].UserId.ShouldBe(_userId1);
        joined.Participants[0].TasksCompleted.ShouldBe(0);
        joined.IsParticipating(_userId1).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_JoiningInactiveChallenge()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge();

        // When / Then
        var ex = Should.Throw<DomainException>(() => challenge.Join(_userId1, _beforeStart));
        ex.Message.ShouldContain("not active");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_JoiningTwice()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge().Join(_userId1, _now);

        // When / Then
        var ex = Should.Throw<DomainException>(() => challenge.Join(_userId1, _now));
        ex.Message.ShouldContain("already a participant");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_JoiningWithEmptyUserId()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge();

        // When / Then — ChallengeParticipant validates empty user IDs
        var ex = Should.Throw<DomainException>(() => challenge.Join(Guid.Empty, _now));
        ex.Message.ShouldContain("user ID cannot be empty");
    }

    // --- Scenario 3: Create a guild challenge ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateGuildChallenge_When_ValidParameters()
    {
        // Given / When
        var challenge = CreateActiveGuildChallenge(_userId1);

        // Then
        challenge.Title.ShouldBe("Boss Rush: Clear all Boss Tasks before Friday");
        challenge.Type.ShouldBe(ChallengeType.Guild);
        challenge.GuildId.ShouldBe(_guildId);
        challenge.CreatedByUserId.ShouldBe(_userId1);
        challenge.Objective.ShouldBe("Complete the most Boss Tasks");
        challenge.Reward.ShouldBe("Seasonal cosmetic + bonus XP");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GuildChallengeHasNoGuildId()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() =>
            new Challenge(ChallengeId.New(), "Test Challenge", ChallengeType.Guild,
                _saturday, _sunday, "Objective", "Reward",
                guildId: null, createdByUserId: _userId1));
        ex.Message.ShouldContain("guild ID");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GuildChallengeHasNoCreator()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() =>
            new Challenge(ChallengeId.New(), "Test Challenge", ChallengeType.Guild,
                _saturday, _sunday, "Objective", "Reward",
                guildId: _guildId, createdByUserId: null));
        ex.Message.ShouldContain("creator user ID");
    }

    // --- Scenario 4: Track challenge progress ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TrackTaskCount_When_TasksAreCompleted()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now);

        // When - complete 8 tasks
        var updated = challenge;
        for (int i = 0; i < 8; i++)
        {
            updated = updated.RecordTaskCompletion(_userId1, _now.AddMinutes(i + 1));
        }

        // Then
        updated.Participants[0].TasksCompleted.ShouldBe(8);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowCurrentRank_When_MultipleParticipants()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Join(_userId2, _now)
            .Join(_userId3, _now);

        // userId1 completes 5, userId2 completes 8, userId3 completes 3
        var updated = challenge;
        for (int i = 0; i < 8; i++)
        {
            updated = updated.RecordTaskCompletion(_userId2, _now.AddMinutes(i + 1));
        }

        for (int i = 0; i < 5; i++)
        {
            updated = updated.RecordTaskCompletion(_userId1, _now.AddMinutes(i + 10));
        }

        for (int i = 0; i < 3; i++)
        {
            updated = updated.RecordTaskCompletion(_userId3, _now.AddMinutes(i + 20));
        }

        // When / Then
        updated.GetParticipantRank(_userId2).ShouldBe(1);
        updated.GetParticipantRank(_userId1).ShouldBe(2);
        updated.GetParticipantRank(_userId3).ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowTopParticipants_When_Requested()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Join(_userId2, _now)
            .Join(_userId3, _now);

        var updated = challenge;
        for (int i = 0; i < 8; i++)
        {
            updated = updated.RecordTaskCompletion(_userId2, _now.AddMinutes(i + 1));
        }

        for (int i = 0; i < 5; i++)
        {
            updated = updated.RecordTaskCompletion(_userId1, _now.AddMinutes(i + 10));
        }

        for (int i = 0; i < 3; i++)
        {
            updated = updated.RecordTaskCompletion(_userId3, _now.AddMinutes(i + 20));
        }

        // When
        var top = updated.GetTopParticipants(5);

        // Then
        top.Count.ShouldBe(3);
        top[0].UserId.ShouldBe(_userId2);
        top[0].TasksCompleted.ShouldBe(8);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RecordingForNonParticipant()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge();

        // When / Then
        var ex = Should.Throw<DomainException>(() =>
            challenge.RecordTaskCompletion(_userId1, _now));
        ex.Message.ShouldContain("not a participant");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GettingRankForNonParticipant()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now);

        // When / Then
        var ex = Should.Throw<DomainException>(() =>
            challenge.GetParticipantRank(_userId2));
        ex.Message.ShouldContain("not an active participant");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TopCountLessThanOne()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge();

        // When / Then
        var ex = Should.Throw<DomainException>(() =>
            challenge.GetTopParticipants(0));
        ex.Message.ShouldContain("at least 1");
    }

    // --- Scenario 5: Challenge ends and results are announced ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AnnounceResults_When_ChallengeEnds()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Join(_userId2, _now)
            .Join(_userId3, _now);

        var updated = challenge;
        for (int i = 0; i < 12; i++)
        {
            updated = updated.RecordTaskCompletion(_userId1, _now.AddMinutes(i + 1));
        }

        for (int i = 0; i < 15; i++)
        {
            updated = updated.RecordTaskCompletion(_userId2, _now.AddMinutes(i + 20));
        }

        for (int i = 0; i < 14; i++)
        {
            updated = updated.RecordTaskCompletion(_userId3, _now.AddMinutes(i + 40));
        }

        // When
        var concluded = updated.Conclude();

        // Then
        concluded.Result.ShouldNotBeNull();
        concluded.Result!.Rankings.Count.ShouldBe(3);

        // userId2 ranked 1st (15 tasks), userId3 2nd (14), userId1 3rd (12)
        concluded.Result.Rankings[0].UserId.ShouldBe(_userId2);
        concluded.Result.Rankings[0].Rank.ShouldBe(1);
        concluded.Result.Rankings[0].ReceivesCosmetic.ShouldBeTrue();

        concluded.Result.Rankings[1].UserId.ShouldBe(_userId3);
        concluded.Result.Rankings[1].Rank.ShouldBe(2);
        concluded.Result.Rankings[1].ReceivesCosmetic.ShouldBeTrue();

        concluded.Result.Rankings[2].UserId.ShouldBe(_userId1);
        concluded.Result.Rankings[2].Rank.ShouldBe(3);
        concluded.Result.Rankings[2].TasksCompleted.ShouldBe(12);
        concluded.Result.Rankings[2].ReceivesCosmetic.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardParticipationXpToAll_When_ChallengeEnds()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Join(_userId2, _now);

        var updated = challenge;
        for (int i = 0; i < 3; i++)
        {
            updated = updated.RecordTaskCompletion(_userId1, _now.AddMinutes(i + 1));
        }

        for (int i = 0; i < 5; i++)
        {
            updated = updated.RecordTaskCompletion(_userId2, _now.AddMinutes(i + 10));
        }

        // When
        var concluded = updated.Conclude();

        // Then - all participants get participation XP
        foreach (ChallengeRanking r in concluded.Result!.Rankings)
        {
            r.XpReward.ShouldBeGreaterThanOrEqualTo(ChallengeResult.ParticipationXp);
        }
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardBonusXpToTop3_When_ChallengeEnds()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Join(_userId2, _now)
            .Join(_userId3, _now)
            .Join(_userId4, _now);

        var updated = challenge;
        for (int i = 0; i < 10; i++)
        {
            updated = updated.RecordTaskCompletion(_userId1, _now.AddMinutes(i + 1));
        }

        for (int i = 0; i < 8; i++)
        {
            updated = updated.RecordTaskCompletion(_userId2, _now.AddMinutes(i + 20));
        }

        for (int i = 0; i < 6; i++)
        {
            updated = updated.RecordTaskCompletion(_userId3, _now.AddMinutes(i + 40));
        }

        for (int i = 0; i < 4; i++)
        {
            updated = updated.RecordTaskCompletion(_userId4, _now.AddMinutes(i + 60));
        }

        // When
        var concluded = updated.Conclude();

        // Then
        int top3Xp = ChallengeResult.ParticipationXp + ChallengeResult.Top3BonusXp;
        concluded.Result!.Rankings[0].XpReward.ShouldBe(top3Xp);
        concluded.Result.Rankings[1].XpReward.ShouldBe(top3Xp);
        concluded.Result.Rankings[2].XpReward.ShouldBe(top3Xp);
        concluded.Result.Rankings[3].XpReward.ShouldBe(ChallengeResult.ParticipationXp);
        concluded.Result.Rankings[3].ReceivesCosmetic.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ConcludingAlreadyConcluded()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Conclude();

        // When / Then
        var ex = Should.Throw<DomainException>(() => challenge.Conclude());
        ex.Message.ShouldContain("already been concluded");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MarkConcludedChallengeAsInactive_When_Concluded()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Conclude();

        // When / Then — concluded challenges are no longer active
        challenge.IsActive(_now).ShouldBeFalse();
    }

    // --- Scenario 6: Challenge does not penalise non-participation ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotPenalise_When_UserDoesNotParticipate()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now);

        // When — userId2 never joins
        var concluded = challenge
            .RecordTaskCompletion(_userId1, _now.AddMinutes(1))
            .Conclude();

        // Then — userId2 is not in results, no penalty
        concluded.Result!.Rankings.ShouldNotContain(r => r.UserId == _userId2);
        concluded.IsParticipating(_userId2).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowViewingResults_When_NotParticipating()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .RecordTaskCompletion(_userId1, _now.AddMinutes(1))
            .Conclude();

        // When / Then — results are accessible to anyone (public data)
        challenge.Result.ShouldNotBeNull();
        challenge.Result!.Rankings.Count.ShouldBe(1);
        challenge.Result.IsFinalized.ShouldBeTrue();
    }

    // --- Scenario 10: Withdraw from a challenge after joining ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemoveFromLeaderboard_When_Withdrawing()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Join(_userId2, _now);

        var updated = challenge;
        for (int i = 0; i < 5; i++)
        {
            updated = updated.RecordTaskCompletion(_userId1, _now.AddMinutes(i + 1));
        }

        for (int i = 0; i < 3; i++)
        {
            updated = updated.RecordTaskCompletion(_userId2, _now.AddMinutes(i + 10));
        }

        // When
        var withdrawn = updated.Withdraw(_userId1);

        // Then
        withdrawn.IsParticipating(_userId1).ShouldBeFalse();
        withdrawn.Participants.First(p => p.UserId == _userId1).Withdrawn.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotReceiveRewards_When_Withdrawn()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Join(_userId2, _now);

        var updated = challenge;
        for (int i = 0; i < 10; i++)
        {
            updated = updated.RecordTaskCompletion(_userId1, _now.AddMinutes(i + 1));
        }

        for (int i = 0; i < 3; i++)
        {
            updated = updated.RecordTaskCompletion(_userId2, _now.AddMinutes(i + 20));
        }

        // When — userId1 withdraws, then challenge concludes
        var concluded = updated.Withdraw(_userId1).Conclude();

        // Then — withdrawn user is excluded from results
        concluded.Result!.Rankings.ShouldNotContain(r => r.UserId == _userId1);
        concluded.Result.Rankings.Count.ShouldBe(1);
        concluded.Result.Rankings[0].UserId.ShouldBe(_userId2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_WithdrawingNonParticipant()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge();

        // When / Then
        var ex = Should.Throw<DomainException>(() => challenge.Withdraw(_userId1));
        ex.Message.ShouldContain("not a participant");
    }

    // --- Scenario 11: Global challenges are system-generated ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateGlobalChallenge_When_SystemGenerated()
    {
        // Given / When
        var challenge = Challenge.CreateGlobal(
            "Weekend Warrior", _saturday, _sunday, "Complete tasks", "XP bonus");

        // Then
        challenge.Type.ShouldBe(ChallengeType.Global);
        challenge.CreatedByUserId.ShouldBeNull();
        challenge.GuildId.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GlobalChallengeHasCreator()
    {
        // Given / When / Then — global challenges cannot have a creator user ID
        var ex = Should.Throw<DomainException>(() =>
            new Challenge(ChallengeId.New(), "Test", ChallengeType.Global,
                _saturday, _sunday, "Objective", "Reward",
                createdByUserId: _userId1));
        ex.Message.ShouldContain("system-generated");
    }

    // --- Scenario 12: Guild challenges can be created by any guild member ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateGuildChallenge_When_AnyMemberCreates()
    {
        // Given — userId1 is a regular member, not leader
        // When
        var challenge = Challenge.CreateGuild(
            "Boss Rush", _saturday, _sunday, "Complete Boss Tasks", "XP bonus",
            _guildId, _userId1);

        // Then
        challenge.Type.ShouldBe(ChallengeType.Guild);
        challenge.CreatedByUserId.ShouldBe(_userId1);
        challenge.GuildId.ShouldBe(_guildId);
    }

    // --- Scenario 13: Tie resolution in challenge rankings ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RankByEarliestCompletion_When_TiedOnTaskCount()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Join(_userId2, _now);

        // Both complete 15 tasks, but userId1 reaches 15 first
        var updated = challenge;
        for (int i = 0; i < 15; i++)
        {
            updated = updated.RecordTaskCompletion(_userId1, _now.AddMinutes(i + 1));
        }

        for (int i = 0; i < 15; i++)
        {
            updated = updated.RecordTaskCompletion(_userId2, _now.AddMinutes(i + 20));
        }

        // When
        var concluded = updated.Conclude();

        // Then — userId1 ranks higher (reached 15 first)
        concluded.Result!.Rankings[0].UserId.ShouldBe(_userId1);
        concluded.Result.Rankings[0].Rank.ShouldBe(1);
        concluded.Result.Rankings[1].UserId.ShouldBe(_userId2);
        concluded.Result.Rankings[1].Rank.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardSameTierReward_When_Tied()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Join(_userId2, _now);

        var updated = challenge;
        for (int i = 0; i < 15; i++)
        {
            updated = updated.RecordTaskCompletion(_userId1, _now.AddMinutes(i + 1));
        }

        for (int i = 0; i < 15; i++)
        {
            updated = updated.RecordTaskCompletion(_userId2, _now.AddMinutes(i + 20));
        }

        // When
        var concluded = updated.Conclude();

        // Then — both tied participants get cosmetic (both in top 3)
        concluded.Result!.Rankings[0].ReceivesCosmetic.ShouldBeTrue();
        concluded.Result.Rankings[1].ReceivesCosmetic.ShouldBeTrue();
    }

    // --- Validation tests ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TitleIsEmpty()
    {
        var ex = Should.Throw<DomainException>(() =>
            Challenge.CreateGlobal("", _saturday, _sunday, "Objective", "Reward"));
        ex.Message.ShouldContain("title cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TitleExceedsMaxLength()
    {
        var ex = Should.Throw<DomainException>(() =>
            Challenge.CreateGlobal(new string('x', 201), _saturday, _sunday, "Objective", "Reward"));
        ex.Message.ShouldContain("cannot exceed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EndBeforeStart()
    {
        var ex = Should.Throw<DomainException>(() =>
            Challenge.CreateGlobal("Test", _sunday, _saturday, "Objective", "Reward"));
        ex.Message.ShouldContain("end time must be after start time");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ObjectiveIsEmpty()
    {
        var ex = Should.Throw<DomainException>(() =>
            Challenge.CreateGlobal("Test", _saturday, _sunday, "", "Reward"));
        ex.Message.ShouldContain("objective cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RewardIsEmpty()
    {
        var ex = Should.Throw<DomainException>(() =>
            Challenge.CreateGlobal("Test", _saturday, _sunday, "Objective", ""));
        ex.Message.ShouldContain("reward cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ChallengeIdIsNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            new Challenge(null!, "Test", ChallengeType.Global, _saturday, _sunday, "Objective", "Reward"));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HasEnded_When_AfterEndTime()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge();

        // When / Then
        challenge.HasEnded(_afterEnd).ShouldBeTrue();
        challenge.HasEnded(_now).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeActiveAtBoundary_When_ExactlyAtStartOrEnd()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge();

        // When / Then — active at exact start and end times
        challenge.IsActive(_saturday).ShouldBeTrue();
        challenge.IsActive(_sunday).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotHaveEnded_When_ExactlyAtEndTime()
    {
        // Given
        var challenge = CreateActiveGlobalChallenge();

        // When / Then
        challenge.HasEnded(_sunday).ShouldBeFalse();
    }

    // --- Mutation-killing boundary tests ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptTitle_When_Exactly200Characters()
    {
        // Kills mutant: title.Length > MaxTitleLength -> title.Length >= MaxTitleLength
        var challenge = Challenge.CreateGlobal(
            new string('x', 200), _saturday, _sunday, "Objective", "Reward");
        challenge.Title.Length.ShouldBe(200);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EndTimeEqualsStartTime()
    {
        // Kills mutant: endTime <= startTime -> endTime < startTime
        var ex = Should.Throw<DomainException>(() =>
            Challenge.CreateGlobal("Test", _saturday, _saturday, "Objective", "Reward"));
        ex.Message.ShouldContain("end time must be after start time");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GetTopParticipants_When_CountIsOne()
    {
        // Kills mutant: count < 1 -> count <= 1
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Join(_userId2, _now);

        var updated = challenge
            .RecordTaskCompletion(_userId1, _now.AddMinutes(1))
            .RecordTaskCompletion(_userId2, _now.AddMinutes(2));

        var top = updated.GetTopParticipants(1);
        top.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RankByEarliestCompletion_When_TiedInGetParticipantRank()
    {
        // Kills mutant: ThenBy -> ThenByDescending in GetParticipantRank
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Join(_userId2, _now);

        // Both complete 5 tasks; userId1 finishes faster
        var updated = challenge;
        for (int i = 0; i < 5; i++)
        {
            updated = updated.RecordTaskCompletion(_userId1, _now.AddMinutes(i + 1));
        }

        for (int i = 0; i < 5; i++)
        {
            updated = updated.RecordTaskCompletion(_userId2, _now.AddMinutes(i + 30));
        }

        // userId1 reached 5 first (at minute 5), userId2 at minute 34
        updated.GetParticipantRank(_userId1).ShouldBe(1);
        updated.GetParticipantRank(_userId2).ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OrderTopParticipantsByEarliestCompletion_When_Tied()
    {
        // Kills mutant: ThenBy -> ThenByDescending in GetTopParticipants
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now)
            .Join(_userId2, _now);

        var updated = challenge;
        for (int i = 0; i < 5; i++)
        {
            updated = updated.RecordTaskCompletion(_userId1, _now.AddMinutes(i + 1));
        }

        for (int i = 0; i < 5; i++)
        {
            updated = updated.RecordTaskCompletion(_userId2, _now.AddMinutes(i + 30));
        }

        var top = updated.GetTopParticipants(5);
        top[0].UserId.ShouldBe(_userId1);
        top[1].UserId.ShouldBe(_userId2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TrackParticipant_When_JoinedButNoTasksCompleted()
    {
        // Kills mutant: statement mutation on participant creation in Join
        var challenge = CreateActiveGlobalChallenge()
            .Join(_userId1, _now);

        challenge.Participants.Count.ShouldBe(1);
        challenge.Participants[0].UserId.ShouldBe(_userId1);
        challenge.Participants[0].TasksCompleted.ShouldBe(0);
        challenge.Participants[0].JoinedAt.ShouldBe(_now);
    }
}
