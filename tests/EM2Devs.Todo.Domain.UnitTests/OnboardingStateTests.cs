using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for the OnboardingState entity and progressive disclosure.
/// Maps to: docs/features/onboarding/progressive-disclosure.feature
/// </summary>
public sealed class OnboardingStateTests
{
    private static readonly PlayerProfileId _profileId = PlayerProfileId.New();
    private static readonly SessionId _sessionId = SessionId.New();

    // -----------------------------------------------------------------------
    // Scenario: Create an account with minimal friction
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartAtWelcomeStep_When_AccountCreated()
    {
        // Given / When — user signs up via social login
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);

        // Then — account created, taken to first-task prompt
        state.CurrentStep.ShouldBe(OnboardingStep.Welcome);
        state.ProfileId.ShouldBe(_profileId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TransitionToFirstTaskPrompt_When_OnboardingBegins()
    {
        // Given — freshly created account
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);

        // When — onboarding begins after social login
        state.BeginOnboarding();

        // Then — taken directly to "Create your first task" prompt
        state.CurrentStep.ShouldBe(OnboardingStep.FirstTask);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNoGamificationElements_When_AccountJustCreated()
    {
        // Given / When — new account
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);

        // Then — no gamification elements visible, clean simple TODO app
        state.UnlockedFeatures.ShouldBeEmpty();
        state.IsFeatureUnlocked(UnlockableFeature.BasicXp).ShouldBeFalse();
        state.IsFeatureUnlocked(UnlockableFeature.Quests).ShouldBeFalse();
        state.IsFeatureUnlocked(UnlockableFeature.SkillTrees).ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Scenario: Create first task during onboarding
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CompleteOnboarding_When_FirstTaskCreated()
    {
        // Given — just created account, at first-task prompt
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.BeginOnboarding();

        // When — user creates first task
        state.CompleteFirstTask();

        // Then — onboarding complete, taken to task inbox
        state.CurrentStep.ShouldBe(OnboardingStep.Complete);
        state.FirstTaskCreated.ShouldBeTrue();
        state.FirstTaskSkipped.ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Scenario: Skip the first-task prompt
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CompleteOnboarding_When_FirstTaskSkipped()
    {
        // Given — just created account, at first-task prompt
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.BeginOnboarding();

        // When — user dismisses the first-task prompt
        state.SkipFirstTask();

        // Then — taken to empty task inbox
        state.CurrentStep.ShouldBe(OnboardingStep.Complete);
        state.FirstTaskSkipped.ShouldBeTrue();
        state.FirstTaskCreated.ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Scenario: Day 1 experience is a clean TODO app
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowOnlyBasicFeatures_When_Day1()
    {
        // Given / When — day 1 features
        IReadOnlySet<string> day1Features = OnboardingState.GetDay1Features();

        // Then — only task inbox, today view, and upcoming view
        day1Features.ShouldContain("task inbox");
        day1Features.ShouldContain("today view");
        day1Features.ShouldContain("upcoming view");
        day1Features.Count.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HideGamificationFeatures_When_Day1()
    {
        // Given / When — day 1 hidden features
        IReadOnlySet<string> hidden = OnboardingState.GetDay1HiddenFeatures();

        // Then — gamification elements hidden
        hidden.ShouldContain("XP");
        hidden.ShouldContain("levels");
        hidden.ShouldContain("skill trees");
        hidden.ShouldContain("quests");
        hidden.ShouldContain("guilds");
        hidden.ShouldContain("leaderboards");
    }

    // -----------------------------------------------------------------------
    // Scenario Outline: Features unlock at specific engagement thresholds
    // -----------------------------------------------------------------------

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(5, 0, 1, 0, UnlockableFeature.Quests)]
    [InlineData(0, 10, 1, 0, UnlockableFeature.BasicXp)]
    [InlineData(0, 0, 3, 0, UnlockableFeature.SkillTrees)]
    [InlineData(0, 0, 5, 0, UnlockableFeature.Titles)]
    [InlineData(0, 0, 1, 3, UnlockableFeature.DailyBrief)]
    [InlineData(0, 0, 7, 0, UnlockableFeature.AccountabilityPartners)]
    public void Should_UnlockFeature_When_EngagementThresholdReached(
        int tasksCreated, int tasksCompleted, int level, int questsCompleted,
        UnlockableFeature expectedFeature)
    {
        // Given — authenticated user with engagement metrics
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);

        // When — system evaluates engagement
        IReadOnlyList<UnlockableFeature> newlyUnlocked = state.EvaluateEngagement(
            tasksCreated, tasksCompleted, level, questsCompleted);

        // Then — feature revealed with contextual explanation
        state.IsFeatureUnlocked(expectedFeature).ShouldBeTrue();
        newlyUnlocked.ShouldContain(expectedFeature);
    }

    // -----------------------------------------------------------------------
    // Scenario: Quest creation unlocked after creating multiple tasks
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnlockQuests_When_FiveTasksCreated()
    {
        // Given — user has created 5 tasks
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);

        // When — system evaluates engagement
        IReadOnlyList<UnlockableFeature> newlyUnlocked = state.EvaluateEngagement(
            tasksCreated: 5, tasksCompleted: 0, currentLevel: 1, questsCompleted: 0);

        // Then — quests unlocked
        newlyUnlocked.ShouldContain(UnlockableFeature.Quests);
        state.IsFeatureUnlocked(UnlockableFeature.Quests).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotUnlockQuests_When_BelowThreshold()
    {
        // Given — user has created only 4 tasks
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);

        // When
        state.EvaluateEngagement(tasksCreated: 4, tasksCompleted: 0, currentLevel: 1, questsCompleted: 0);

        // Then — quests not yet unlocked
        state.IsFeatureUnlocked(UnlockableFeature.Quests).ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Scenario: XP becomes visible after completing several tasks
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnlockXp_When_TenTasksCompleted()
    {
        // Given — user has completed 10 tasks
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);

        // When — system evaluates engagement
        IReadOnlyList<UnlockableFeature> newlyUnlocked = state.EvaluateEngagement(
            tasksCreated: 0, tasksCompleted: 10, currentLevel: 1, questsCompleted: 0);

        // Then — XP indicators appear, retroactive XP should be displayed
        newlyUnlocked.ShouldContain(UnlockableFeature.BasicXp);
        state.IsFeatureUnlocked(UnlockableFeature.BasicXp).ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Scenario: Features are never removed once revealed
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_KeepFeatureUnlocked_When_PreviouslyRevealed()
    {
        // Given — quests have been revealed
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.EvaluateEngagement(tasksCreated: 5, tasksCompleted: 0, currentLevel: 1, questsCompleted: 0);
        state.IsFeatureUnlocked(UnlockableFeature.Quests).ShouldBeTrue();

        // When — engagement re-evaluated (even with lower metrics, features are permanent)
        state.EvaluateEngagement(tasksCreated: 0, tasksCompleted: 0, currentLevel: 1, questsCompleted: 0);

        // Then — quest feature remains permanently available
        state.IsFeatureUnlocked(UnlockableFeature.Quests).ShouldBeTrue();
        state.IsFeaturePermanentlyUnlocked(UnlockableFeature.Quests).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NeverLoseFeatureAccess_When_MultipleEvaluations()
    {
        // Given — multiple features unlocked over time
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.EvaluateEngagement(tasksCreated: 5, tasksCompleted: 10, currentLevel: 3, questsCompleted: 0);

        // When — re-evaluate with zero metrics
        state.EvaluateEngagement(tasksCreated: 0, tasksCompleted: 0, currentLevel: 1, questsCompleted: 0);

        // Then — all previously unlocked features remain
        state.IsFeaturePermanentlyUnlocked(UnlockableFeature.Quests).ShouldBeTrue();
        state.IsFeaturePermanentlyUnlocked(UnlockableFeature.BasicXp).ShouldBeTrue();
        state.IsFeaturePermanentlyUnlocked(UnlockableFeature.SkillTrees).ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Scenario: User can manually explore features early
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowPreviewsOfLockedFeatures_When_ExploringDiscoverSection()
    {
        // Given — new user on day 2 with no features unlocked
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);

        // When — navigating to "Discover features" section
        IReadOnlyList<FeaturePreview> previews = state.GetAvailablePreviews();

        // Then — see preview of upcoming features with unlock requirements
        previews.Count.ShouldBeGreaterThan(0);
        previews.ShouldAllBe(p => !string.IsNullOrEmpty(p.Description));
        previews.ShouldAllBe(p => !string.IsNullOrEmpty(p.UnlockRequirement));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotForceUnlockFeatures_When_ViewingPreviews()
    {
        // Given — new user viewing previews
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);

        // When — previews are retrieved
        state.GetAvailablePreviews();

        // Then — no features actually unlocked
        state.UnlockedFeatures.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeUnlockedFeaturesFromPreviews_When_SomeAlreadyUnlocked()
    {
        // Given — user with quests already unlocked
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.EvaluateEngagement(tasksCreated: 5, tasksCompleted: 0, currentLevel: 1, questsCompleted: 0);

        // When — viewing previews
        IReadOnlyList<FeaturePreview> previews = state.GetAvailablePreviews();

        // Then — quests not in previews (already unlocked)
        previews.ShouldNotContain(p => p.Feature == UnlockableFeature.Quests);
    }

    // -----------------------------------------------------------------------
    // Scenario: Premium users see all features immediately
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnlockAllFeatures_When_PremiumUser()
    {
        // Given / When — premium user completes onboarding
        OnboardingState state = OnboardingState.CreatePremium(_profileId, _sessionId);

        // Then — all features immediately available and permanently unlocked
        foreach (UnlockableFeature feature in Enum.GetValues<UnlockableFeature>())
        {
            state.IsFeatureUnlocked(feature).ShouldBeTrue();
            state.IsFeaturePermanentlyUnlocked(feature).ShouldBeTrue();
        }
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StillSupportTutorials_When_PremiumUser()
    {
        // Given — premium user with all features
        OnboardingState state = OnboardingState.CreatePremium(_profileId, _sessionId);

        // When — requesting a contextual tutorial
        bool shown = state.RequestTutorial(TutorialTopic.QuestCreation);

        // Then — contextual tutorials still shown as features are used for first time
        shown.ShouldBeTrue();
        state.TutorialState.HasSeenTutorial(TutorialTopic.QuestCreation).ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Scenario: Retroactive XP reveal
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculateRetroactiveXp_When_GamificationActivated()
    {
        // Given — user has completed 45 tasks over 3 weeks (pre-gamification)
        int completedTasks = 45;

        // When — gamification layer activated, retroactive XP calculated
        ExperiencePoints retroXp = OnboardingState.CalculateRetroactiveXp(completedTasks);

        // Then — XP calculated from historical completions (base Normal XP per task)
        int expectedXp = completedTasks * ExperiencePoints.BaseForDifficulty(TaskDifficulty.Normal).Value;
        retroXp.Value.ShouldBe(expectedXp);
        retroXp.Value.ShouldBeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReachMeaningfulLevel_When_RetroactiveXpApplied()
    {
        // Given — retroactive XP from 45 completions
        ExperiencePoints retroXp = OnboardingState.CalculateRetroactiveXp(45);

        // When — applied to a new profile
        PlayerProfile profile = PlayerProfile.NewProfile(TestData.TestUserId);
        profile.AwardXp(retroXp);

        // Then — the player has already reached a level (not starting from scratch)
        profile.Level.Value.ShouldBeGreaterThan(1);
    }

    // -----------------------------------------------------------------------
    // Scenario: Retroactive skill tree unlocks
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculateRetroactiveSkillTreeProgress_When_GamificationActivated()
    {
        // Given — 20 tasks tagged "creative" during pre-gamification phase
        int taggedCompletions = 20;

        // When — gamification activated, progress calculated
        int progress = OnboardingState.CalculateRetroactiveSkillTreeProgress(taggedCompletions);

        // Then — progress reflects historical completions
        progress.ShouldBe(20);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ApplyRetroactiveSkillTreeProgress_When_Activated()
    {
        // Given — 20 "creative" completions, Creator skill tree discovered
        PlayerProfile profile = PlayerProfile.NewProfile(TestData.TestUserId);
        profile.DiscoverSkillTree(SkillTreeType.Creator);

        // When — retroactive progress applied
        int retroProgress = OnboardingState.CalculateRetroactiveSkillTreeProgress(20);
        for (int i = 0; i < retroProgress; i++)
        {
            profile.RecordSkillTreeProgress(SkillTreeType.Creator);
        }

        // Then — progress toward tier 2 reflects historical completions
        SkillTree creatorTree = profile.SkillTrees.First(t => t.Type == SkillTreeType.Creator);
        creatorTree.TasksCompletedInTier.ShouldBeGreaterThan(0);
    }

    // -----------------------------------------------------------------------
    // Scenario: Retroactive title eligibility
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_QualifyForRetroactiveTitle_When_HistoricalPatternMatches()
    {
        // Given — completed tasks before 9 AM consistently during pre-gamification
        int earlyMorningCompletions = 10;
        int totalCompletionDays = 10;

        // When — gamification activated, eligibility checked
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.MorningArchitect, earlyMorningCompletions, totalCompletionDays);

        // Then — title awarded based on historical data
        qualifies.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotQualifyForTitle_When_InsufficientHistory()
    {
        // Given — not enough early morning completions
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.MorningArchitect, earlyMorningCompletions: 2, totalCompletionDays: 2);

        // Then — does not qualify
        qualifies.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_TitleTypeHasNoRetroactiveRule()
    {
        // Given — a title type without retroactive eligibility rules
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.BossSlayer, earlyMorningCompletions: 100, totalCompletionDays: 100);

        // Then — does not qualify (no retroactive rule for BossSlayer)
        qualifies.ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Scenario: Quest tutorial on first quest creation
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowQuestTutorial_When_FirstQuestCreated()
    {
        // Given — quests revealed, first quest being created
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.EvaluateEngagement(tasksCreated: 5, tasksCompleted: 0, currentLevel: 1, questsCompleted: 0);

        // When — first quest created
        bool shown = state.RequestTutorial(TutorialTopic.QuestCreation);

        // Then — contextual tooltip shown, dismissible
        shown.ShouldBeTrue();
        state.TutorialState.HasSeenTutorial(TutorialTopic.QuestCreation).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotShowQuestTutorial_When_AlreadySeen()
    {
        // Given — quest tutorial already shown
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.RequestTutorial(TutorialTopic.QuestCreation);

        // When — creating another quest
        bool shown = state.RequestTutorial(TutorialTopic.QuestCreation);

        // Then — tutorial does not appear again
        shown.ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Scenario: Boss Task tutorial on first Boss Task encounter
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowBossTaskTutorial_When_FirstBossTaskViewed()
    {
        // Given — user has first Boss Task
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);

        // When — viewing Boss Task for the first time
        bool shown = state.RequestTutorial(TutorialTopic.BossTask);

        // Then — brief explanation shown
        shown.ShouldBeTrue();
        state.TutorialState.HasSeenTutorial(TutorialTopic.BossTask).ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Scenario: No tutorial bombardment
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowAtMostOneTutorial_When_MultipleNewlyUnlocked()
    {
        // Given — multiple features newly unlocked
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);

        // When — user logs in (first tutorial shown, second queued)
        bool firstShown = state.RequestTutorial(TutorialTopic.QuestCreation);
        bool secondShown = state.RequestTutorial(TutorialTopic.BossTask);

        // Then — at most 1 tutorial per session
        firstShown.ShouldBeTrue();
        secondShown.ShouldBeFalse();
        state.TutorialState.QueuedTutorials.ShouldContain(TutorialTopic.BossTask);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowQueuedTutorial_When_NextSessionStarts()
    {
        // Given — tutorial queued from previous session
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.RequestTutorial(TutorialTopic.QuestCreation);
        state.RequestTutorial(TutorialTopic.BossTask); // queued

        // When — new session starts
        SessionId newSession = SessionId.New();
        state.StartNewSession(newSession);
        bool shown = state.RequestTutorial(TutorialTopic.BossTask);

        // Then — queued tutorial shown in new session
        shown.ShouldBeTrue();
        state.TutorialState.HasSeenTutorial(TutorialTopic.BossTask).ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Scenario: Session is defined by a login or app launch
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateNewSession_When_LoginOrAppLaunch()
    {
        // Given — existing session
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        SessionId originalSession = state.TutorialState.CurrentSessionId;

        // When — new login or app launch
        SessionId newSession = SessionId.New();
        state.StartNewSession(newSession);

        // Then — this counts as a new session for tutorial purposes
        state.TutorialState.CurrentSessionId.ShouldBe(newSession);
        state.TutorialState.CurrentSessionId.ShouldNotBe(originalSession);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetTutorialCounter_When_NewSessionStarts()
    {
        // Given — session with tutorial already shown
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.RequestTutorial(TutorialTopic.QuestCreation);
        state.TutorialState.TutorialsShownThisSession.ShouldBe(1);

        // When — new session
        state.StartNewSession(SessionId.New());

        // Then — counter reset (returning from background would NOT reset)
        state.TutorialState.TutorialsShownThisSession.ShouldBe(0);
    }

    // -----------------------------------------------------------------------
    // Guard / edge case tests for mutation coverage
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_BeginOnboardingFromWrongStep()
    {
        // Given — already past Welcome step
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.BeginOnboarding();

        // When / Then
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => state.BeginOnboarding());
        ex.Message.ShouldContain("Welcome");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompleteFirstTaskFromWrongStep()
    {
        // Given — still at Welcome step
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);

        // When / Then
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => state.CompleteFirstTask());
        ex.Message.ShouldContain("FirstTask");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SkipFirstTaskFromWrongStep()
    {
        // Given — already completed onboarding
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.BeginOnboarding();
        state.CompleteFirstTask();

        // When / Then
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => state.SkipFirstTask());
        ex.Message.ShouldContain("FirstTask");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreateWithNullProfileId()
    {
        Should.Throw<ArgumentNullException>(
            () => OnboardingState.Create(null!, _sessionId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreateWithNullSessionId()
    {
        Should.Throw<ArgumentNullException>(
            () => OnboardingState.Create(_profileId, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreatePremiumWithNullProfileId()
    {
        Should.Throw<ArgumentNullException>(
            () => OnboardingState.CreatePremium(null!, _sessionId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreatePremiumWithNullSessionId()
    {
        // Null propagates to TutorialState constructor
        Should.Throw<ArgumentNullException>(
            () => OnboardingState.CreatePremium(_profileId, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_StartNewSessionWithNullId()
    {
        // Null propagates to TutorialState constructor via StartNewSession
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);

        Should.Throw<ArgumentNullException>(
            () => state.StartNewSession(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RetroactiveXpWithNegativeCount()
    {
        Should.Throw<Exceptions.DomainException>(
            () => OnboardingState.CalculateRetroactiveXp(-1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RetroactiveSkillTreeWithNegativeCount()
    {
        Should.Throw<Exceptions.DomainException>(
            () => OnboardingState.CalculateRetroactiveSkillTreeProgress(-1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroXp_When_RetroactiveXpWithZeroCompletions()
    {
        ExperiencePoints retroXp = OnboardingState.CalculateRetroactiveXp(0);
        retroXp.Value.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroProgress_When_RetroactiveSkillTreeWithZeroCompletions()
    {
        int progress = OnboardingState.CalculateRetroactiveSkillTreeProgress(0);
        progress.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotDuplicateUnlockedFeatures_When_EvaluatedMultipleTimes()
    {
        // Given — quests already unlocked
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.EvaluateEngagement(tasksCreated: 5, tasksCompleted: 0, currentLevel: 1, questsCompleted: 0);

        // When — evaluate again with same metrics
        IReadOnlyList<UnlockableFeature> newlyUnlocked = state.EvaluateEngagement(
            tasksCreated: 5, tasksCompleted: 0, currentLevel: 1, questsCompleted: 0);

        // Then — no newly unlocked features (already unlocked)
        newlyUnlocked.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrue_When_IsPremium()
    {
        OnboardingState state = OnboardingState.CreatePremium(_profileId, _sessionId);
        state.IsPremium.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_NotPremium()
    {
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.IsPremium.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_QualifyForEarlyBird_When_SufficientHistory()
    {
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.EarlyBird, earlyMorningCompletions: 5, totalCompletionDays: 5);
        qualifies.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotQualifyForEarlyBird_When_InsufficientDays()
    {
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.EarlyBird, earlyMorningCompletions: 2, totalCompletionDays: 2);
        qualifies.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveEmptyPreviewList_When_AllFeaturesUnlocked()
    {
        OnboardingState state = OnboardingState.CreatePremium(_profileId, _sessionId);
        IReadOnlyList<FeaturePreview> previews = state.GetAvailablePreviews();
        previews.ShouldBeEmpty();
    }

    // -----------------------------------------------------------------------
    // Boundary tests for retroactive title eligibility (kill mutation survivors)
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_QualifyForMorningArchitect_When_ExactlyAtThreshold()
    {
        // Given — exactly 5 days and 5 early morning completions
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.MorningArchitect, earlyMorningCompletions: 5, totalCompletionDays: 5);
        qualifies.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotQualifyForMorningArchitect_When_DaysJustBelowThreshold()
    {
        // Given — 4 days but 5 completions (days < 5)
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.MorningArchitect, earlyMorningCompletions: 5, totalCompletionDays: 4);
        qualifies.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotQualifyForMorningArchitect_When_CompletionsJustBelowThreshold()
    {
        // Given — 5 days but only 4 completions (completions < 5)
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.MorningArchitect, earlyMorningCompletions: 4, totalCompletionDays: 5);
        qualifies.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotQualifyForMorningArchitect_When_OnlyDaysMet()
    {
        // Given — enough days but zero completions (kills OR mutation)
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.MorningArchitect, earlyMorningCompletions: 0, totalCompletionDays: 10);
        qualifies.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotQualifyForMorningArchitect_When_OnlyCompletionsMet()
    {
        // Given — enough completions but zero days (kills OR mutation)
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.MorningArchitect, earlyMorningCompletions: 10, totalCompletionDays: 0);
        qualifies.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_QualifyForEarlyBird_When_ExactlyAtThreshold()
    {
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.EarlyBird, earlyMorningCompletions: 3, totalCompletionDays: 3);
        qualifies.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotQualifyForEarlyBird_When_DaysJustBelowThreshold()
    {
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.EarlyBird, earlyMorningCompletions: 3, totalCompletionDays: 2);
        qualifies.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotQualifyForEarlyBird_When_CompletionsJustBelowThreshold()
    {
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.EarlyBird, earlyMorningCompletions: 2, totalCompletionDays: 3);
        qualifies.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotQualifyForEarlyBird_When_OnlyDaysMet()
    {
        // Kills OR mutation
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.EarlyBird, earlyMorningCompletions: 0, totalCompletionDays: 10);
        qualifies.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotQualifyForEarlyBird_When_OnlyCompletionsMet()
    {
        // Kills OR mutation
        bool qualifies = OnboardingState.QualifiesForRetroactiveTitle(
            TitleType.EarlyBird, earlyMorningCompletions: 10, totalCompletionDays: 0);
        qualifies.ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Error message validation (kill string mutation survivors)
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContainMeaningfulMessage_When_RetroactiveXpNegative()
    {
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => OnboardingState.CalculateRetroactiveXp(-1));
        ex.Message.ShouldContain("Completed task count");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContainMeaningfulMessage_When_RetroactiveSkillTreeNegative()
    {
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => OnboardingState.CalculateRetroactiveSkillTreeProgress(-1));
        ex.Message.ShouldContain("negative");
    }

    // -----------------------------------------------------------------------
    // Test coverage for null guard in constructor via CreatePremium
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_VerifySessionIdStored_When_Created()
    {
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        state.TutorialState.CurrentSessionId.ShouldBe(_sessionId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_VerifyAllUnlockableFeatures_When_PremiumCreated()
    {
        OnboardingState state = OnboardingState.CreatePremium(_profileId, _sessionId);
        int totalFeatures = Enum.GetValues<UnlockableFeature>().Length;
        state.UnlockedFeatures.Count.ShouldBe(totalFeatures);
    }

    // -----------------------------------------------------------------------
    // Test for feature descriptions via previews (kill NoCoverage mutant on GetFeatureDescription)
    // -----------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnDescriptivePreviews_When_AllLockedFeaturesRequested()
    {
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        IReadOnlyList<FeaturePreview> previews = state.GetAvailablePreviews();

        // All 6 previewable features should have non-empty descriptions
        previews.Count.ShouldBe(6);
        foreach (FeaturePreview preview in previews)
        {
            preview.Description.Length.ShouldBeGreaterThan(0);
            preview.UnlockRequirement.Length.ShouldBeGreaterThan(0);
        }
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeCorrectFeatureDescriptions_When_Previewing()
    {
        OnboardingState state = OnboardingState.Create(_profileId, _sessionId);
        IReadOnlyList<FeaturePreview> previews = state.GetAvailablePreviews();

        FeaturePreview questPreview = previews.First(p => p.Feature == UnlockableFeature.Quests);
        questPreview.Description.ShouldContain("quests");

        FeaturePreview xpPreview = previews.First(p => p.Feature == UnlockableFeature.BasicXp);
        xpPreview.Description.ShouldContain("experience");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentOutOfRange_When_GetFeatureDescriptionForUnsupportedFeature()
    {
        // GetFeatureDescription's default case must throw for non-previewable features
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(
            () => OnboardingState.GetFeatureDescription(UnlockableFeature.Leaderboards));
        ex.Message.ShouldContain("No description defined");
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(UnlockableFeature.Quests)]
    [InlineData(UnlockableFeature.BasicXp)]
    [InlineData(UnlockableFeature.SkillTrees)]
    [InlineData(UnlockableFeature.Titles)]
    [InlineData(UnlockableFeature.DailyBrief)]
    [InlineData(UnlockableFeature.AccountabilityPartners)]
    public void Should_ReturnNonEmptyDescription_When_GetFeatureDescriptionForSupportedFeature(
        UnlockableFeature feature)
    {
        string description = OnboardingState.GetFeatureDescription(feature);
        description.Length.ShouldBeGreaterThan(0);
    }
}
