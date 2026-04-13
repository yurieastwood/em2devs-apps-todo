using Shouldly;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for TutorialState value object.
/// Maps to: docs/features/onboarding/progressive-disclosure.feature — Tutorial rule
/// </summary>
public sealed class TutorialStateTests
{
    private static readonly SessionId _sessionId = SessionId.New();

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartWithCleanState_When_NewSessionCreated()
    {
        // Given / When
        TutorialState state = TutorialState.NewSession(_sessionId);

        // Then
        state.CurrentSessionId.ShouldBe(_sessionId);
        state.TutorialsShownThisSession.ShouldBe(0);
        state.SeenTutorials.ShouldBeEmpty();
        state.QueuedTutorials.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MarkTutorialAsSeen_When_Requested()
    {
        // Given
        TutorialState state = TutorialState.NewSession(_sessionId);

        // When
        TutorialState updated = state.RequestTutorial(TutorialTopic.QuestCreation);

        // Then
        updated.HasSeenTutorial(TutorialTopic.QuestCreation).ShouldBeTrue();
        updated.TutorialsShownThisSession.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_QueueTutorial_When_BombardmentLimitReached()
    {
        // Given — one tutorial already shown
        TutorialState state = TutorialState.NewSession(_sessionId)
            .RequestTutorial(TutorialTopic.QuestCreation);

        // When — another tutorial requested in same session
        TutorialState updated = state.RequestTutorial(TutorialTopic.BossTask);

        // Then — second tutorial queued, not shown
        updated.HasSeenTutorial(TutorialTopic.BossTask).ShouldBeFalse();
        updated.QueuedTutorials.ShouldContain(TutorialTopic.BossTask);
        updated.TutorialsShownThisSession.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NoOp_When_TutorialAlreadySeen()
    {
        // Given — tutorial already seen
        TutorialState state = TutorialState.NewSession(_sessionId)
            .RequestTutorial(TutorialTopic.QuestCreation);

        // When — same tutorial requested again
        TutorialState updated = state.RequestTutorial(TutorialTopic.QuestCreation);

        // Then — no change
        updated.TutorialsShownThisSession.ShouldBe(1);
        updated.SeenTutorials.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetSessionCounter_When_NewSessionStarted()
    {
        // Given — session with tutorial shown
        TutorialState state = TutorialState.NewSession(_sessionId)
            .RequestTutorial(TutorialTopic.QuestCreation);

        // When — new session
        SessionId newSession = SessionId.New();
        TutorialState updated = state.StartNewSession(newSession);

        // Then — counter reset, seen tutorials preserved
        updated.TutorialsShownThisSession.ShouldBe(0);
        updated.CurrentSessionId.ShouldBe(newSession);
        updated.HasSeenTutorial(TutorialTopic.QuestCreation).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowNextQueued_When_NewSessionAndQueuedExists()
    {
        // Given — tutorial queued
        TutorialState state = TutorialState.NewSession(_sessionId)
            .RequestTutorial(TutorialTopic.QuestCreation)
            .RequestTutorial(TutorialTopic.BossTask); // queued

        // When — new session, show next queued
        TutorialState newSessionState = state.StartNewSession(SessionId.New());
        TutorialState updated = newSessionState.ShowNextQueued();

        // Then — queued tutorial now shown
        updated.HasSeenTutorial(TutorialTopic.BossTask).ShouldBeTrue();
        updated.QueuedTutorials.ShouldNotContain(TutorialTopic.BossTask);
        updated.TutorialsShownThisSession.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NoOp_When_ShowNextQueuedWithEmptyQueue()
    {
        TutorialState state = TutorialState.NewSession(_sessionId);
        TutorialState updated = state.ShowNextQueued();

        updated.TutorialsShownThisSession.ShouldBe(0);
        updated.SeenTutorials.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NoOp_When_ShowNextQueuedButLimitReached()
    {
        // Given — session limit already reached with queued items
        TutorialState state = TutorialState.NewSession(_sessionId)
            .RequestTutorial(TutorialTopic.QuestCreation)
            .RequestTutorial(TutorialTopic.BossTask); // queued

        // When — try to show next queued without new session
        TutorialState updated = state.ShowNextQueued();

        // Then — no change because limit already reached
        updated.HasSeenTutorial(TutorialTopic.BossTask).ShouldBeFalse();
        updated.TutorialsShownThisSession.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrue_When_CanShowTutorialAndWithinLimit()
    {
        TutorialState state = TutorialState.NewSession(_sessionId);
        state.CanShowTutorial(TutorialTopic.QuestCreation).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_CanShowTutorialButAlreadySeen()
    {
        TutorialState state = TutorialState.NewSession(_sessionId)
            .RequestTutorial(TutorialTopic.QuestCreation);
        state.CanShowTutorial(TutorialTopic.QuestCreation).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_CanShowTutorialButLimitReached()
    {
        TutorialState state = TutorialState.NewSession(_sessionId)
            .RequestTutorial(TutorialTopic.QuestCreation);
        state.CanShowTutorial(TutorialTopic.BossTask).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_NewSessionWithNullId()
    {
        Should.Throw<ArgumentNullException>(() => TutorialState.NewSession(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_StartNewSessionWithNullId()
    {
        TutorialState state = TutorialState.NewSession(_sessionId);
        Should.Throw<ArgumentNullException>(() => state.StartNewSession(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveQueuedTutorials_When_StartingNewSession()
    {
        // Given — multiple tutorials queued
        TutorialState state = TutorialState.NewSession(_sessionId)
            .RequestTutorial(TutorialTopic.QuestCreation)
            .RequestTutorial(TutorialTopic.BossTask)
            .RequestTutorial(TutorialTopic.XpSystem);

        // When — new session
        TutorialState updated = state.StartNewSession(SessionId.New());

        // Then — queued tutorials preserved
        updated.QueuedTutorials.ShouldContain(TutorialTopic.BossTask);
        updated.QueuedTutorials.ShouldContain(TutorialTopic.XpSystem);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotDuplicateQueuedTutorial_When_RequestedTwice()
    {
        // Given — tutorial already queued
        TutorialState state = TutorialState.NewSession(_sessionId)
            .RequestTutorial(TutorialTopic.QuestCreation)
            .RequestTutorial(TutorialTopic.BossTask);

        // When — same tutorial requested again (still queued)
        TutorialState updated = state.RequestTutorial(TutorialTopic.BossTask);

        // Then — no duplicate in queue
        updated.QueuedTutorials.Count(t => t == TutorialTopic.BossTask).ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotQueueAlreadySeenTutorial_When_Requested()
    {
        // Given — QuestCreation already seen
        TutorialState state = TutorialState.NewSession(_sessionId)
            .RequestTutorial(TutorialTopic.QuestCreation);

        // When — request same tutorial again (limit reached but tutorial already seen)
        TutorialState updated = state.RequestTutorial(TutorialTopic.QuestCreation);

        // Then — should NOT be queued (it was already seen), queued list stays empty
        updated.QueuedTutorials.ShouldBeEmpty();
        updated.TutorialsShownThisSession.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemoveFromQueue_When_TutorialShownInNewSession()
    {
        // Given — BossTask queued from previous session
        TutorialState state = TutorialState.NewSession(_sessionId)
            .RequestTutorial(TutorialTopic.QuestCreation)
            .RequestTutorial(TutorialTopic.BossTask); // queued

        state.QueuedTutorials.ShouldContain(TutorialTopic.BossTask);

        // When — new session starts, show the queued BossTask via RequestTutorial
        TutorialState newSession = state.StartNewSession(SessionId.New());
        TutorialState updated = newSession.RequestTutorial(TutorialTopic.BossTask);

        // Then — BossTask removed from queue after being shown
        updated.QueuedTutorials.ShouldNotContain(TutorialTopic.BossTask);
        updated.HasSeenTutorial(TutorialTopic.BossTask).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExposeMaxTutorialsPerSession_When_Checked()
    {
        TutorialState.MaxTutorialsPerSession.ShouldBe(1);
    }
}
