namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Strongly-typed session identifier for tutorial bombardment prevention.
/// A new session is created on login or app launch, not on resume from background.
/// Maps to: docs/features/onboarding/progressive-disclosure.feature
/// </summary>
public sealed record SessionId(Guid Value)
{
    public static SessionId New() => new(Guid.NewGuid());
}
