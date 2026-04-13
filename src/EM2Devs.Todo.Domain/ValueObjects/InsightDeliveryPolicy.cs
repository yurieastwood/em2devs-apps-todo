using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Controls the cadence, cooldown, and data thresholds for insight card delivery.
/// Maps to: docs/features/reflection/insight-cards.feature
/// Rule: "Insights are generated from behavioural data and delivered as discoverable cards"
/// </summary>
public sealed record InsightDeliveryPolicy
{
    /// <summary>
    /// Maximum number of insight cards delivered per day.
    /// </summary>
    public int MaxPerDay { get; }

    /// <summary>
    /// Minimum number of insight cards delivered per week.
    /// </summary>
    public int MinPerWeek { get; }

    /// <summary>
    /// Maximum number of insight cards delivered per week.
    /// </summary>
    public int MaxPerWeek { get; }

    /// <summary>
    /// Number of days the same insight type cannot repeat (cooldown period).
    /// </summary>
    public int CooldownDays { get; }

    /// <summary>
    /// Minimum number of days of task history required before insights can be generated.
    /// </summary>
    public int MinimumDataDays { get; }

    public InsightDeliveryPolicy(int maxPerDay, int minPerWeek, int maxPerWeek, int cooldownDays, int minimumDataDays)
    {
        if (maxPerDay <= 0)
        {
            throw new DomainException("Max per day must be positive.");
        }

        if (minPerWeek <= 0)
        {
            throw new DomainException("Min per week must be positive.");
        }

        if (maxPerWeek < minPerWeek)
        {
            throw new DomainException("Max per week cannot be less than min per week.");
        }

        if (cooldownDays <= 0)
        {
            throw new DomainException("Cooldown days must be positive.");
        }

        if (minimumDataDays <= 0)
        {
            throw new DomainException("Minimum data days must be positive.");
        }

        MaxPerDay = maxPerDay;
        MinPerWeek = minPerWeek;
        MaxPerWeek = maxPerWeek;
        CooldownDays = cooldownDays;
        MinimumDataDays = minimumDataDays;
    }

    /// <summary>
    /// The default delivery policy: max 1/day, 2-3/week, 90-day cooldown per type, 30-day minimum data.
    /// </summary>
    public static InsightDeliveryPolicy Default => new(1, 2, 3, 90, 30);

    /// <summary>
    /// Returns true if the user has enough historical data for insight generation.
    /// </summary>
    public bool HasSufficientData(int daysOfHistory) => daysOfHistory >= MinimumDataDays;

    /// <summary>
    /// Returns true if the insight type is within its cooldown period.
    /// </summary>
    public bool IsInCooldown(DateOnly lastDelivered, DateOnly today)
    {
        int daysSinceLast = today.DayNumber - lastDelivered.DayNumber;
        return daysSinceLast < CooldownDays;
    }

    /// <summary>
    /// Returns true if the daily delivery limit has been reached.
    /// </summary>
    public bool HasReachedDailyLimit(int deliveredToday) => deliveredToday >= MaxPerDay;

    /// <summary>
    /// Returns true if the weekly delivery limit has been reached.
    /// </summary>
    public bool HasReachedWeeklyLimit(int deliveredThisWeek) => deliveredThisWeek >= MaxPerWeek;
}
