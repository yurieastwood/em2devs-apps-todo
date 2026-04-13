using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// Tracks the onboarding journey of a player, including step progression,
/// first task creation, feature unlocks, tutorial state, and retroactive reveals.
/// Maps to: docs/features/onboarding/progressive-disclosure.feature
/// </summary>
public sealed class OnboardingState
{
    public PlayerProfileId ProfileId { get; }
    public OnboardingStep CurrentStep { get; private set; }
    public bool FirstTaskCreated { get; private set; }
    public bool FirstTaskSkipped { get; private set; }
    public bool IsPremium { get; private set; }
    public TutorialState TutorialState { get; private set; }

    private readonly HashSet<UnlockableFeature> _unlockedFeatures = [];
    private readonly HashSet<UnlockableFeature> _permanentlyUnlockedFeatures = [];

    /// <summary>
    /// Features that have been revealed to the user through progressive disclosure.
    /// </summary>
    public IReadOnlySet<UnlockableFeature> UnlockedFeatures => _unlockedFeatures;

    private OnboardingState(
        PlayerProfileId profileId,
        OnboardingStep currentStep,
        bool isPremium,
        TutorialState tutorialState)
    {
        ArgumentNullException.ThrowIfNull(profileId);
        ProfileId = profileId;
        CurrentStep = currentStep;
        IsPremium = isPremium;
        TutorialState = tutorialState;
    }

    /// <summary>
    /// Creates a new onboarding state for a freshly signed-up user.
    /// The user starts at the Welcome step with no features unlocked.
    /// </summary>
    public static OnboardingState Create(PlayerProfileId profileId, SessionId sessionId)
    {
        return new OnboardingState(profileId, OnboardingStep.Welcome, isPremium: false, TutorialState.NewSession(sessionId));
    }

    /// <summary>
    /// Creates a new onboarding state for a premium user with all features immediately available.
    /// </summary>
    public static OnboardingState CreatePremium(PlayerProfileId profileId, SessionId sessionId)
    {
        var state = new OnboardingState(profileId, OnboardingStep.Welcome, isPremium: true, TutorialState.NewSession(sessionId));
        foreach (UnlockableFeature feature in Enum.GetValues<UnlockableFeature>())
        {
            state._unlockedFeatures.Add(feature);
            state._permanentlyUnlockedFeatures.Add(feature);
        }

        return state;
    }

    /// <summary>
    /// Transitions from Welcome to the FirstTask prompt.
    /// This happens immediately after social login account creation.
    /// </summary>
    public void BeginOnboarding()
    {
        if (CurrentStep != OnboardingStep.Welcome)
        {
            throw new Exceptions.DomainException(
                "Onboarding can only begin from the Welcome step.");
        }

        CurrentStep = OnboardingStep.FirstTask;
    }

    /// <summary>
    /// Records that the user created their first task during onboarding.
    /// Completes the onboarding flow.
    /// </summary>
    public void CompleteFirstTask()
    {
        if (CurrentStep != OnboardingStep.FirstTask)
        {
            throw new Exceptions.DomainException(
                "First task can only be completed during the FirstTask step.");
        }

        FirstTaskCreated = true;
        CurrentStep = OnboardingStep.Complete;
    }

    /// <summary>
    /// Records that the user skipped the first-task prompt.
    /// Completes the onboarding flow, taking the user to an empty inbox.
    /// </summary>
    public void SkipFirstTask()
    {
        if (CurrentStep != OnboardingStep.FirstTask)
        {
            throw new Exceptions.DomainException(
                "First task can only be skipped during the FirstTask step.");
        }

        FirstTaskSkipped = true;
        CurrentStep = OnboardingStep.Complete;
    }

    /// <summary>
    /// Evaluates engagement metrics and unlocks features that meet their thresholds.
    /// Unlocked features are permanent and can never be removed.
    /// </summary>
    public IReadOnlyList<UnlockableFeature> EvaluateEngagement(
        int tasksCreated,
        int tasksCompleted,
        int currentLevel,
        int questsCompleted)
    {
        var eligible = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated, tasksCompleted, currentLevel, questsCompleted);

        var newlyUnlocked = new List<UnlockableFeature>();
        foreach (var feature in eligible)
        {
            if (_unlockedFeatures.Add(feature))
            {
                _permanentlyUnlockedFeatures.Add(feature);
                newlyUnlocked.Add(feature);
            }
        }

        return newlyUnlocked.AsReadOnly();
    }

    /// <summary>
    /// Returns true if the specified feature has been revealed to the user.
    /// </summary>
    public bool IsFeatureUnlocked(UnlockableFeature feature) =>
        _unlockedFeatures.Contains(feature);

    /// <summary>
    /// Verifies that a previously unlocked feature remains permanently available.
    /// Features are never removed once revealed.
    /// </summary>
    public bool IsFeaturePermanentlyUnlocked(UnlockableFeature feature) =>
        _permanentlyUnlockedFeatures.Contains(feature);

    /// <summary>
    /// Returns the set of features visible on day 1 (only basic task management).
    /// No gamification elements should be shown.
    /// </summary>
    public static IReadOnlySet<string> GetDay1Features()
    {
        return new HashSet<string>(StringComparer.Ordinal) { "task inbox", "today view", "upcoming view" };
    }

    /// <summary>
    /// Returns the set of features that should be hidden on day 1.
    /// </summary>
    public static IReadOnlySet<string> GetDay1HiddenFeatures()
    {
        return new HashSet<string>(StringComparer.Ordinal)
        {
            "XP", "levels", "skill trees", "quests", "guilds", "leaderboards",
        };
    }

    /// <summary>
    /// Returns preview information for features that are not yet unlocked.
    /// Users can browse upcoming features without force-unlocking them.
    /// </summary>
    public IReadOnlyList<FeaturePreview> GetAvailablePreviews()
    {
        var previews = new List<FeaturePreview>();
        UnlockableFeature[] previewableFeatures =
        [
            UnlockableFeature.Quests,
            UnlockableFeature.BasicXp,
            UnlockableFeature.SkillTrees,
            UnlockableFeature.Titles,
            UnlockableFeature.DailyBrief,
            UnlockableFeature.AccountabilityPartners,
        ];

        foreach (var feature in previewableFeatures)
        {
            if (!_unlockedFeatures.Contains(feature))
            {
                string description = GetFeatureDescription(feature);
                string requirement = EngagementUnlockRegistry.GetThresholdDescription(feature);
                previews.Add(new FeaturePreview(feature, description, requirement));
            }
        }

        return previews.AsReadOnly();
    }

    /// <summary>
    /// Requests a contextual tutorial. Respects bombardment prevention rules.
    /// </summary>
    public bool RequestTutorial(TutorialTopic topic)
    {
        if (TutorialState.HasSeenTutorial(topic))
        {
            return false;
        }

        bool canShow = TutorialState.CanShowTutorial(topic);
        TutorialState = TutorialState.RequestTutorial(topic);
        return canShow;
    }

    /// <summary>
    /// Starts a new session (triggered by login or app launch).
    /// Resets the per-session tutorial counter and processes queued tutorials.
    /// </summary>
    public void StartNewSession(SessionId newSessionId)
    {
        TutorialState = TutorialState.StartNewSession(newSessionId);
    }

    /// <summary>
    /// Calculates retroactive XP from historical task completions.
    /// Used when the gamification layer activates for a user who has been
    /// using Waypoint during Phase 1 (pre-gamification).
    /// </summary>
    public static ExperiencePoints CalculateRetroactiveXp(int completedTaskCount)
    {
        if (completedTaskCount < 0)
        {
            throw new Exceptions.DomainException(
                "Completed task count cannot be negative.");
        }

        // Each completed task earns the base Normal difficulty XP retroactively
        int xpPerTask = ExperiencePoints.BaseForDifficulty(TaskDifficulty.Normal).Value;
        return new ExperiencePoints(completedTaskCount * xpPerTask);
    }

    /// <summary>
    /// Calculates retroactive skill tree progress from historical tagged completions.
    /// Returns the number of tasks that count toward the skill tree.
    /// </summary>
    public static int CalculateRetroactiveSkillTreeProgress(int taggedCompletions)
    {
        if (taggedCompletions < 0)
        {
            throw new Exceptions.DomainException(
                "Tagged completion count cannot be negative.");
        }

        return taggedCompletions;
    }

    /// <summary>
    /// Determines if a player qualifies for retroactive title eligibility
    /// based on historical completion patterns.
    /// </summary>
    public static bool QualifiesForRetroactiveTitle(
        TitleType titleType,
        int earlyMorningCompletions,
        int totalCompletionDays)
    {
        return titleType switch
        {
            TitleType.MorningArchitect => totalCompletionDays >= 5
                && earlyMorningCompletions >= 5,
            TitleType.EarlyBird => totalCompletionDays >= 3
                && earlyMorningCompletions >= 3,
            _ => false,
        };
    }

    internal static string GetFeatureDescription(UnlockableFeature feature)
    {
        return feature switch
        {
            UnlockableFeature.Quests => "Group related tasks into quests for focused progress",
            UnlockableFeature.BasicXp => "Earn experience points for completing tasks",
            UnlockableFeature.SkillTrees => "Develop specialized skills through themed task completion",
            UnlockableFeature.Titles => "Earn titles that reflect your productivity patterns",
            UnlockableFeature.DailyBrief => "Complete multi-task storylines across longer periods",
            UnlockableFeature.AccountabilityPartners => "Partner with others for mutual motivation",
            _ => throw new ArgumentOutOfRangeException(
                nameof(feature), feature, "No description defined for this feature."),
        };
    }
}
