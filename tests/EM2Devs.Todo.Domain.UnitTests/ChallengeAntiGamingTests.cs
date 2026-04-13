using Shouldly;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for ChallengeAntiGaming domain service.
/// Maps to: docs/features/social/challenge-mode.feature
/// Rule: "Challenges use anti-gaming measures to ensure fair competition"
/// </summary>
public sealed class ChallengeAntiGamingTests
{
    private static readonly DateTimeOffset _baseTime = new(2026, 4, 11, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _challengeStart = new(2026, 4, 11, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _challengeEnd = new(2026, 4, 12, 23, 59, 0, TimeSpan.Zero);

    // --- Scenario 7: Trivial task spam during a challenge ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RejectTrivialTask_When_DifficultyBelowThreshold()
    {
        // Given — a trivial task
        var title = new TaskTitle("Complete this trivial task");
        var createdAt = _baseTime;
        var completedAt = _baseTime.AddMinutes(10);

        // When
        bool eligible = ChallengeAntiGaming.IsEligible(title, TaskDifficulty.Trivial, createdAt, completedAt);

        // Then
        eligible.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RejectEasyTask_When_DifficultyBelowThreshold()
    {
        // Given
        var title = new TaskTitle("Complete this easy task here");
        var createdAt = _baseTime;
        var completedAt = _baseTime.AddMinutes(10);

        // When
        bool eligible = ChallengeAntiGaming.IsEligible(title, TaskDifficulty.Easy, createdAt, completedAt);

        // Then
        eligible.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptNormalTask_When_AllCriteriaMet()
    {
        // Given
        var title = new TaskTitle("Complete this normal difficulty task");
        var createdAt = _baseTime;
        var completedAt = _baseTime.AddMinutes(10);

        // When
        bool eligible = ChallengeAntiGaming.IsEligible(title, TaskDifficulty.Normal, createdAt, completedAt);

        // Then
        eligible.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptHardTask_When_AllCriteriaMet()
    {
        // Given
        var title = new TaskTitle("Complete this hard difficulty task");
        var createdAt = _baseTime;
        var completedAt = _baseTime.AddMinutes(10);

        // When
        bool eligible = ChallengeAntiGaming.IsEligible(title, TaskDifficulty.Hard, createdAt, completedAt);

        // Then
        eligible.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptEpicTask_When_AllCriteriaMet()
    {
        // Given
        var title = new TaskTitle("Complete this epic difficulty task");
        var createdAt = _baseTime;
        var completedAt = _baseTime.AddMinutes(10);

        // When
        bool eligible = ChallengeAntiGaming.IsEligible(title, TaskDifficulty.Epic, createdAt, completedAt);

        // Then
        eligible.ShouldBeTrue();
    }

    // --- Scenario 9: Minimum difficulty threshold for challenge tasks ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RejectTask_When_OpenForLessThan5Minutes()
    {
        // Given — task completed after only 3 minutes
        var title = new TaskTitle("This is a legitimate task title");
        var createdAt = _baseTime;
        var completedAt = _baseTime.AddMinutes(3);

        // When
        bool eligible = ChallengeAntiGaming.IsEligible(title, TaskDifficulty.Normal, createdAt, completedAt);

        // Then
        eligible.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptTask_When_OpenForExactly5Minutes()
    {
        // Given
        var title = new TaskTitle("This is a legitimate task title");
        var createdAt = _baseTime;
        var completedAt = _baseTime.AddMinutes(5);

        // When
        bool eligible = ChallengeAntiGaming.IsEligible(title, TaskDifficulty.Normal, createdAt, completedAt);

        // Then
        eligible.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RejectTask_When_TitleShorterThan10Characters()
    {
        // Given — title has only 9 characters
        var title = new TaskTitle("Short tsk");
        var createdAt = _baseTime;
        var completedAt = _baseTime.AddMinutes(10);

        // When
        bool eligible = ChallengeAntiGaming.IsEligible(title, TaskDifficulty.Normal, createdAt, completedAt);

        // Then
        eligible.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptTask_When_TitleExactly10Characters()
    {
        // Given — title has exactly 10 characters
        var title = new TaskTitle("1234567890");
        var createdAt = _baseTime;
        var completedAt = _baseTime.AddMinutes(10);

        // When
        bool eligible = ChallengeAntiGaming.IsEligible(title, TaskDifficulty.Normal, createdAt, completedAt);

        // Then
        eligible.ShouldBeTrue();
    }

    // --- Scenario 8: Tasks completed during challenge window count regardless of creation date ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CountTask_When_CompletedDuringChallengeWindow()
    {
        // Given — task created on Friday, completed on Saturday (during challenge)
        var completedAt = _challengeStart.AddHours(3);

        // When
        bool inWindow = ChallengeAntiGaming.IsWithinChallengeWindow(completedAt, _challengeStart, _challengeEnd);

        // Then
        inWindow.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCountTask_When_CompletedBeforeChallengeStart()
    {
        // Given — task completed before Saturday
        var completedAt = _challengeStart.AddHours(-1);

        // When
        bool inWindow = ChallengeAntiGaming.IsWithinChallengeWindow(completedAt, _challengeStart, _challengeEnd);

        // Then
        inWindow.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCountTask_When_CompletedAfterChallengeEnd()
    {
        // Given — task completed after Sunday 23:59
        var completedAt = _challengeEnd.AddMinutes(1);

        // When
        bool inWindow = ChallengeAntiGaming.IsWithinChallengeWindow(completedAt, _challengeStart, _challengeEnd);

        // Then
        inWindow.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CountTask_When_CompletedExactlyAtChallengeStart()
    {
        // Given / When
        bool inWindow = ChallengeAntiGaming.IsWithinChallengeWindow(_challengeStart, _challengeStart, _challengeEnd);

        // Then
        inWindow.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CountTask_When_CompletedExactlyAtChallengeEnd()
    {
        // Given / When
        bool inWindow = ChallengeAntiGaming.IsWithinChallengeWindow(_challengeEnd, _challengeStart, _challengeEnd);

        // Then
        inWindow.ShouldBeTrue();
    }

    // --- Individual method tests ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrue_When_DifficultyIsNormalOrAbove()
    {
        ChallengeAntiGaming.MeetsDifficultyThreshold(TaskDifficulty.Normal).ShouldBeTrue();
        ChallengeAntiGaming.MeetsDifficultyThreshold(TaskDifficulty.Hard).ShouldBeTrue();
        ChallengeAntiGaming.MeetsDifficultyThreshold(TaskDifficulty.Epic).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_DifficultyIsBelowNormal()
    {
        ChallengeAntiGaming.MeetsDifficultyThreshold(TaskDifficulty.Trivial).ShouldBeFalse();
        ChallengeAntiGaming.MeetsDifficultyThreshold(TaskDifficulty.Easy).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrue_When_TitleMeetsLength()
    {
        ChallengeAntiGaming.MeetsTitleLengthRequirement(new TaskTitle("1234567890")).ShouldBeTrue();
        ChallengeAntiGaming.MeetsTitleLengthRequirement(new TaskTitle("Long enough title")).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_TitleTooShort()
    {
        ChallengeAntiGaming.MeetsTitleLengthRequirement(new TaskTitle("Short")).ShouldBeFalse();
        ChallengeAntiGaming.MeetsTitleLengthRequirement(new TaskTitle("123456789")).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrue_When_OpenTimeIsSufficient()
    {
        ChallengeAntiGaming.MeetsMinimumOpenTime(_baseTime, _baseTime.AddMinutes(5)).ShouldBeTrue();
        ChallengeAntiGaming.MeetsMinimumOpenTime(_baseTime, _baseTime.AddMinutes(60)).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_OpenTimeIsInsufficient()
    {
        ChallengeAntiGaming.MeetsMinimumOpenTime(_baseTime, _baseTime.AddMinutes(4)).ShouldBeFalse();
        ChallengeAntiGaming.MeetsMinimumOpenTime(_baseTime, _baseTime.AddMinutes(0)).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_TitleIsNull_InIsEligible()
    {
        Should.Throw<ArgumentNullException>(() =>
            ChallengeAntiGaming.IsEligible(null!, TaskDifficulty.Normal, _baseTime, _baseTime.AddMinutes(10)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_TitleIsNull_InMeetsTitleLength()
    {
        Should.Throw<ArgumentNullException>(() =>
            ChallengeAntiGaming.MeetsTitleLengthRequirement(null!));
    }
}
