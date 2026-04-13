namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Tracks the current step in the onboarding flow.
/// Maps to: docs/features/onboarding/progressive-disclosure.feature
/// </summary>
public enum OnboardingStep
{
    Welcome,
    FirstTask,
    Complete,
}
