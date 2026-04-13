using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Tracks when an upgrade prompt was shown for a specific premium feature.
/// Enforces the constraint that the same feature prompt should not appear more than once per week.
/// </summary>
public sealed record UpgradePrompt
{
    public static readonly TimeSpan MinimumInterval = TimeSpan.FromDays(7);

    public string FeatureName { get; }
    public DateTimeOffset LastShownAt { get; }

    public UpgradePrompt(string featureName, DateTimeOffset lastShownAt)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            throw new DomainException("Feature name cannot be empty.");
        }

        FeatureName = featureName;
        LastShownAt = lastShownAt;
    }

    /// <summary>
    /// Whether enough time has passed since the last prompt to show it again.
    /// </summary>
    public bool CanShowAgain(DateTimeOffset now)
    {
        return now - LastShownAt >= MinimumInterval;
    }

    /// <summary>
    /// Creates a new prompt record with the current timestamp.
    /// </summary>
    public UpgradePrompt RecordShown(DateTimeOffset now)
    {
        return new UpgradePrompt(FeatureName, now);
    }
}
