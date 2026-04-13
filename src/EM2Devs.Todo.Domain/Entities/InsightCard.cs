using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// A personalised productivity insight card generated from behavioural data.
/// Maps to: docs/features/reflection/insight-cards.feature
/// </summary>
public sealed class InsightCard
{
    public InsightCardId Id { get; }
    public InsightType Type { get; }
    public string Message { get; }
    public string SupportingData { get; }
    public InsightCardStatus Status { get; private set; }
    public DateOnly GeneratedAt { get; }
    public bool IsValidated { get; }

    private InsightCard(
        InsightCardId id,
        InsightType type,
        string message,
        string supportingData,
        InsightCardStatus status,
        DateOnly generatedAt,
        bool isValidated)
    {
        Id = id;
        Type = type;
        Message = message;
        SupportingData = supportingData;
        Status = status;
        GeneratedAt = generatedAt;
        IsValidated = isValidated;
    }

    /// <summary>
    /// Generates a new insight card after pattern detection and data validation.
    /// The insight must be validated against user data before delivery.
    /// </summary>
    public static InsightCard Generate(
        InsightType type,
        string message,
        string supportingData,
        DateOnly generatedAt,
        bool isValidated)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new DomainException("Insight card message cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(supportingData))
        {
            throw new DomainException("Insight card supporting data cannot be empty.");
        }

        if (!isValidated)
        {
            throw new DomainException("Insight must be validated against user data before delivery.");
        }

        return new InsightCard(
            InsightCardId.New(), type, message, supportingData,
            InsightCardStatus.Unread, generatedAt, isValidated);
    }

    /// <summary>
    /// Marks the insight card as read.
    /// </summary>
    public void MarkAsRead()
    {
        if (Status == InsightCardStatus.Dismissed)
        {
            throw new DomainException("Cannot mark a dismissed insight card as read.");
        }

        Status = InsightCardStatus.Read;
    }

    /// <summary>
    /// Saves the insight card for later reference.
    /// </summary>
    public void Save()
    {
        if (Status == InsightCardStatus.Dismissed)
        {
            throw new DomainException("Cannot save a dismissed insight card.");
        }

        Status = InsightCardStatus.Saved;
    }

    /// <summary>
    /// Dismisses the insight card, removing it from the active list.
    /// </summary>
    public void Dismiss()
    {
        Status = InsightCardStatus.Dismissed;
    }

    /// <summary>
    /// Evaluates whether the delivery policy allows this insight to be generated,
    /// considering data sufficiency, daily/weekly limits, and cooldown periods.
    /// Returns true if delivery is allowed.
    /// </summary>
    public static bool CanDeliver(
        InsightDeliveryPolicy policy,
        int daysOfHistory,
        int deliveredToday,
        int deliveredThisWeek,
        InsightType type,
        IReadOnlyList<(InsightType Type, DateOnly DeliveredAt)> recentDeliveries,
        DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(recentDeliveries);

        if (!policy.HasSufficientData(daysOfHistory))
        {
            return false;
        }

        if (policy.HasReachedDailyLimit(deliveredToday))
        {
            return false;
        }

        if (policy.HasReachedWeeklyLimit(deliveredThisWeek))
        {
            return false;
        }

        foreach (var delivery in recentDeliveries)
        {
            if (delivery.Type == type && policy.IsInCooldown(delivery.DeliveredAt, today))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Calculates a reduced frequency weight for an insight type based on dismissal count.
    /// Returns a value between 0.0 and 1.0 where lower values mean reduced frequency.
    /// </summary>
    public static double CalculateDismissalWeight(int dismissalCount)
    {
        if (dismissalCount < 1)
        {
            return 1.0;
        }

        if (dismissalCount > 2)
        {
            return 0.0;
        }

        return 1.0 - (dismissalCount / 3.0);
    }
}
