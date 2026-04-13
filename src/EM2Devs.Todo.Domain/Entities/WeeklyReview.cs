using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// Entity representing a weekly review ritual.
/// Contains summary metrics, reflection notes, and status tracking.
/// Supports draft persistence and completion with XP rewards.
/// </summary>
public sealed class WeeklyReview
{
    public const int ReviewXpReward = 50;
    public const int EstimatedMinutes = 5;
    public const int FollowUpReminderHours = 24;
    public const int ConsistentPlannerThreshold = 8;

    private readonly Dictionary<string, string> _reflectionNotes = new();

    public WeeklyReviewId Id { get; }
    public DateOnly WeekStart { get; }
    public WeeklyReviewSummary? Summary { get; private set; }
    public IReadOnlyDictionary<string, string> ReflectionNotes => _reflectionNotes;
    public WeeklyReviewStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public bool IsPremium { get; }

    // Premium-only fields
    public IReadOnlyList<WeeklyReviewSummary>? ComparisonWeeks { get; private set; }
    public string? MostProductiveDayAndTime { get; private set; }
    public IReadOnlyList<string>? AvoidedTasks { get; private set; }
    public int? EstimationAccuracyPercent { get; private set; }
    public IReadOnlyList<string>? QuestProgressUpdates { get; private set; }
    public IReadOnlyList<string>? TrendInsights { get; private set; }

    // Notification tracking
    public bool IsPromptDismissed { get; private set; }
    public DateTimeOffset? DismissedAt { get; private set; }
    public bool FollowUpReminderSent { get; private set; }

    private WeeklyReview(
        WeeklyReviewId id,
        DateOnly weekStart,
        bool isPremium,
        DateTimeOffset createdAt)
    {
        Id = id;
        WeekStart = weekStart;
        IsPremium = isPremium;
        CreatedAt = createdAt;
        Status = WeeklyReviewStatus.Draft;
    }

    /// <summary>
    /// Starts a new weekly review. The review begins as a Draft.
    /// </summary>
    public static WeeklyReview Start(DateOnly weekStart, bool isPremium = false, DateTimeOffset? createdAt = null)
    {
        return new WeeklyReview(
            WeeklyReviewId.New(),
            weekStart,
            isPremium,
            createdAt ?? DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Sets the summary metrics for this review.
    /// </summary>
    public void SetSummary(WeeklyReviewSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        Summary = summary;
    }

    /// <summary>
    /// Records a reflection note for the given prompt question.
    /// </summary>
    public void AddReflection(string prompt, string text)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new DomainException("Reflection prompt cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("Reflection text cannot be empty.");
        }

        if (Status == WeeklyReviewStatus.Complete)
        {
            throw new DomainException("Cannot add reflections to a completed review.");
        }

        _reflectionNotes[prompt] = text;
    }

    /// <summary>
    /// Completes the review, awarding XP. Returns the XP earned.
    /// </summary>
    public ExperiencePoints Complete()
    {
        if (Status == WeeklyReviewStatus.Complete)
        {
            throw new DomainException("Review is already complete.");
        }

        if (Summary is null)
        {
            throw new DomainException("Cannot complete a review without a summary.");
        }

        Status = WeeklyReviewStatus.Complete;
        CompletedAt = DateTimeOffset.UtcNow;

        return new ExperiencePoints(ReviewXpReward);
    }

    /// <summary>
    /// Saves the current state as a draft (for mid-review logout).
    /// </summary>
    public void SaveAsDraft()
    {
        if (Status == WeeklyReviewStatus.Complete)
        {
            throw new DomainException("Cannot save a completed review as draft.");
        }

        // Status is already Draft — this is a no-op save point
        // but we keep the method explicit for domain semantics
    }

    /// <summary>
    /// Sets premium analytics data for the review.
    /// </summary>
    public void SetPremiumData(
        IReadOnlyList<WeeklyReviewSummary> comparisonWeeks,
        string mostProductiveDayAndTime,
        IReadOnlyList<string> avoidedTasks,
        int estimationAccuracyPercent,
        IReadOnlyList<string> questProgressUpdates)
    {
        if (!IsPremium)
        {
            throw new DomainException("Premium data is only available for premium users.");
        }

        ArgumentNullException.ThrowIfNull(comparisonWeeks);

        if (string.IsNullOrWhiteSpace(mostProductiveDayAndTime))
        {
            throw new DomainException("Most productive day and time cannot be empty.");
        }

        ArgumentNullException.ThrowIfNull(avoidedTasks);
        ArgumentNullException.ThrowIfNull(questProgressUpdates);

        if (estimationAccuracyPercent < 0 || estimationAccuracyPercent > 100)
        {
            throw new DomainException("Estimation accuracy must be between 0 and 100.");
        }

        ComparisonWeeks = comparisonWeeks;
        MostProductiveDayAndTime = mostProductiveDayAndTime;
        AvoidedTasks = avoidedTasks;
        EstimationAccuracyPercent = estimationAccuracyPercent;
        QuestProgressUpdates = questProgressUpdates;
    }

    /// <summary>
    /// Sets trend insights for the review (premium, requires 8+ historical reviews).
    /// </summary>
    public void SetTrendInsights(IReadOnlyList<string> insights)
    {
        if (!IsPremium)
        {
            throw new DomainException("Trend insights are only available for premium users.");
        }

        ArgumentNullException.ThrowIfNull(insights);

        if (insights.Count == 0)
        {
            throw new DomainException("Trend insights cannot be empty.");
        }

        TrendInsights = insights;
    }

    /// <summary>
    /// Marks the review prompt as dismissed.
    /// </summary>
    public void DismissPrompt(DateTimeOffset dismissedAt)
    {
        IsPromptDismissed = true;
        DismissedAt = dismissedAt;
    }

    /// <summary>
    /// Marks the follow-up reminder as sent.
    /// </summary>
    public void MarkFollowUpReminderSent()
    {
        if (!IsPromptDismissed)
        {
            throw new DomainException("Cannot send follow-up reminder when prompt was not dismissed.");
        }

        FollowUpReminderSent = true;
    }

    /// <summary>
    /// Returns true if a follow-up reminder should be sent based on the current time.
    /// </summary>
    public bool ShouldSendFollowUpReminder(DateTimeOffset currentTime)
    {
        if (!IsPromptDismissed || FollowUpReminderSent || DismissedAt is null)
        {
            return false;
        }

        return currentTime >= DismissedAt.Value.AddHours(FollowUpReminderHours);
    }

    /// <summary>
    /// Creates a notification for the weekly review prompt.
    /// </summary>
    public static Notification CreatePromptNotification()
    {
        return Notification.Create(
            NotificationType.WeeklyReviewPrompt,
            $"Time for your weekly review! Estimated time: {EstimatedMinutes} minutes.");
    }

    /// <summary>
    /// Determines whether completing reviews has earned the Consistent Planner title progress.
    /// </summary>
    public static bool QualifiesForConsistentPlannerProgress(int completedReviewCount)
    {
        return completedReviewCount >= ConsistentPlannerThreshold;
    }
}
