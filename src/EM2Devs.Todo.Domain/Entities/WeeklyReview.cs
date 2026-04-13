using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// Represents a weekly review session. Contains summary metrics for the week,
/// user reflections, and review status (Draft or Complete).
/// Completing a review awards XP and contributes to the review streak.
/// Premium users get additional data including productivity charts,
/// estimation accuracy, and pattern analysis.
/// </summary>
public sealed class WeeklyReview
{
    public const int CompletionXp = 25;
    public const int EstimatedMinutes = 5;
    public const string WentWellPrompt = "What went well this week?";
    public const string CouldGoBetterPrompt = "What could go better next week?";

    public WeeklyReviewId Id { get; }
    public DateOnly WeekStart { get; }
    public WeeklyReviewSummary Summary { get; }
    public WeeklyReviewStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public bool IsPremium { get; }

    private readonly Dictionary<string, string> _reflections = [];

    public IReadOnlyDictionary<string, string> Reflections => _reflections;

    /// <summary>
    /// Premium-only fields for advanced review.
    /// </summary>
    public IReadOnlyList<WeeklyReviewSummary>? PastWeeksSummaries { get; }
    public string? MostProductiveDay { get; }
    public string? MostProductiveTimeWindow { get; }
    public IReadOnlyList<string>? AvoidedTasks { get; }
    public int? EstimationAccuracyPercent { get; }
    public IReadOnlyList<string>? QuestProgressUpdates { get; }
    public IReadOnlyList<string>? TrendInsights { get; }

    private WeeklyReview(
        WeeklyReviewId id,
        DateOnly weekStart,
        WeeklyReviewSummary summary,
        DateTimeOffset createdAt,
        bool isPremium,
        IReadOnlyList<WeeklyReviewSummary>? pastWeeksSummaries,
        string? mostProductiveDay,
        string? mostProductiveTimeWindow,
        IReadOnlyList<string>? avoidedTasks,
        int? estimationAccuracyPercent,
        IReadOnlyList<string>? questProgressUpdates,
        IReadOnlyList<string>? trendInsights)
    {
        Id = id;
        WeekStart = weekStart;
        Summary = summary;
        Status = WeeklyReviewStatus.Draft;
        CreatedAt = createdAt;
        IsPremium = isPremium;
        PastWeeksSummaries = pastWeeksSummaries;
        MostProductiveDay = mostProductiveDay;
        MostProductiveTimeWindow = mostProductiveTimeWindow;
        AvoidedTasks = avoidedTasks;
        EstimationAccuracyPercent = estimationAccuracyPercent;
        QuestProgressUpdates = questProgressUpdates;
        TrendInsights = trendInsights;
    }

    /// <summary>
    /// Creates a basic (free-tier) weekly review with summary metrics.
    /// </summary>
    public static WeeklyReview Create(
        DateOnly weekStart,
        WeeklyReviewSummary summary,
        DateTimeOffset? createdAt = null)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return new WeeklyReview(
            WeeklyReviewId.New(),
            weekStart,
            summary,
            createdAt ?? DateTimeOffset.UtcNow,
            isPremium: false,
            pastWeeksSummaries: null,
            mostProductiveDay: null,
            mostProductiveTimeWindow: null,
            avoidedTasks: null,
            estimationAccuracyPercent: null,
            questProgressUpdates: null,
            trendInsights: null);
    }

    /// <summary>
    /// Creates a premium weekly review with advanced analytics data.
    /// </summary>
    public static WeeklyReview CreatePremium(
        DateOnly weekStart,
        WeeklyReviewSummary summary,
        IReadOnlyList<WeeklyReviewSummary> pastWeeksSummaries,
        string mostProductiveDay,
        string mostProductiveTimeWindow,
        IReadOnlyList<string> avoidedTasks,
        int estimationAccuracyPercent,
        IReadOnlyList<string> questProgressUpdates,
        IReadOnlyList<string>? trendInsights = null,
        DateTimeOffset? createdAt = null)
    {
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentNullException.ThrowIfNull(pastWeeksSummaries);
        ArgumentNullException.ThrowIfNull(avoidedTasks);
        ArgumentNullException.ThrowIfNull(questProgressUpdates);

        if (string.IsNullOrWhiteSpace(mostProductiveDay))
        {
            throw new DomainException("Most productive day cannot be empty for premium reviews.");
        }

        if (string.IsNullOrWhiteSpace(mostProductiveTimeWindow))
        {
            throw new DomainException("Most productive time window cannot be empty for premium reviews.");
        }

        if (estimationAccuracyPercent < 0 || estimationAccuracyPercent > 100)
        {
            throw new DomainException("Estimation accuracy must be between 0 and 100.");
        }

        return new WeeklyReview(
            WeeklyReviewId.New(),
            weekStart,
            summary,
            createdAt ?? DateTimeOffset.UtcNow,
            isPremium: true,
            pastWeeksSummaries,
            mostProductiveDay,
            mostProductiveTimeWindow,
            avoidedTasks,
            estimationAccuracyPercent,
            questProgressUpdates,
            trendInsights);
    }

    /// <summary>
    /// Adds or updates a reflection response for a given prompt.
    /// </summary>
    public void AddReflection(string prompt, string response)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new DomainException("Reflection prompt cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(response))
        {
            throw new DomainException("Reflection response cannot be empty.");
        }

        if (Status == WeeklyReviewStatus.Complete)
        {
            throw new DomainException("Cannot add reflections to a completed review.");
        }

        _reflections[prompt] = response;
    }

    /// <summary>
    /// Completes the review. Requires at least the two standard reflections
    /// for basic reviews.
    /// </summary>
    public ExperiencePoints Complete()
    {
        if (Status == WeeklyReviewStatus.Complete)
        {
            throw new DomainException("Review is already complete.");
        }

        if (!IsPremium && _reflections.Count < 2)
        {
            throw new DomainException("Basic review requires at least two reflections to complete.");
        }

        Status = WeeklyReviewStatus.Complete;
        CompletedAt = DateTimeOffset.UtcNow;
        return new ExperiencePoints(CompletionXp);
    }

    /// <summary>
    /// Saves the current state as a draft. Already in draft status by default,
    /// but this method signals an explicit save (e.g., before logout).
    /// Returns true if the review is still a draft and can be resumed.
    /// </summary>
    public bool SaveAsDraft()
    {
        return Status == WeeklyReviewStatus.Draft;
    }

    /// <summary>
    /// Returns true if this review has draft status and can be resumed.
    /// </summary>
    public bool CanResume => Status == WeeklyReviewStatus.Draft;
}
