namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a preview of a locked feature, showing what it does and
/// what engagement threshold unlocks it.
/// Maps to: docs/features/onboarding/progressive-disclosure.feature
/// — "User can manually explore features early"
/// </summary>
public sealed record FeaturePreview
{
    public UnlockableFeature Feature { get; }
    public string Description { get; }
    public string UnlockRequirement { get; }

    public FeaturePreview(UnlockableFeature feature, string description, string unlockRequirement)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new Exceptions.DomainException("Feature preview description cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(unlockRequirement))
        {
            throw new Exceptions.DomainException("Feature preview unlock requirement cannot be empty.");
        }

        Feature = feature;
        Description = description;
        UnlockRequirement = unlockRequirement;
    }
}
