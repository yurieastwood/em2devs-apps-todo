using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// Represents a user's subscription to a Waypoint plan.
/// Manages tier, status, lifecycle transitions (upgrade, downgrade, expiry, renewal).
/// </summary>
public sealed class Subscription
{
    public SubscriptionId Id { get; }
    public Guid UserId { get; }
    public SubscriptionTier Tier { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public bool AutoRenew { get; private set; }

    private Subscription(SubscriptionId id, Guid userId, SubscriptionTier tier,
        SubscriptionStatus status, DateTimeOffset startedAt, DateTimeOffset? expiresAt, bool autoRenew)
    {
        Id = id;
        UserId = userId;
        Tier = tier;
        Status = status;
        StartedAt = startedAt;
        ExpiresAt = expiresAt;
        AutoRenew = autoRenew;
    }

    /// <summary>
    /// Creates a new free-tier subscription for a user.
    /// Free subscriptions have no expiry date.
    /// </summary>
    public static Subscription CreateFree(Guid userId, DateTimeOffset now)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User ID cannot be empty.");
        }

        return new Subscription(SubscriptionId.New(), userId, SubscriptionTier.Free,
            SubscriptionStatus.Active, now, expiresAt: null, autoRenew: false);
    }

    /// <summary>
    /// Upgrades this subscription to the Pro tier.
    /// </summary>
    public void UpgradeToPro(DateTimeOffset now, DateTimeOffset expiresAt)
    {
        if (Tier == SubscriptionTier.Pro || Tier == SubscriptionTier.Team)
        {
            throw new DomainException($"Cannot upgrade to Pro from {Tier} tier.");
        }

        Tier = SubscriptionTier.Pro;
        Status = SubscriptionStatus.Active;
        ExpiresAt = expiresAt;
        AutoRenew = true;
    }

    /// <summary>
    /// Upgrades this subscription to the Team tier.
    /// </summary>
    public void UpgradeToTeam(DateTimeOffset now, DateTimeOffset expiresAt)
    {
        if (Tier == SubscriptionTier.Team)
        {
            throw new DomainException("Already on Team tier.");
        }

        Tier = SubscriptionTier.Team;
        Status = SubscriptionStatus.Active;
        ExpiresAt = expiresAt;
        AutoRenew = true;
    }

    /// <summary>
    /// Expires the subscription, reverting the user to free-tier.
    /// Premium data remains read-only.
    /// </summary>
    public void Expire()
    {
        if (Status == SubscriptionStatus.Expired)
        {
            throw new DomainException("Subscription is already expired.");
        }

        if (Tier == SubscriptionTier.Free)
        {
            throw new DomainException("Free-tier subscriptions do not expire.");
        }

        Status = SubscriptionStatus.Expired;
    }

    /// <summary>
    /// Cancels the subscription. For team tiers, enters a grace period.
    /// </summary>
    public void Cancel()
    {
        if (Status == SubscriptionStatus.Cancelled)
        {
            throw new DomainException("Subscription is already cancelled.");
        }

        if (Status == SubscriptionStatus.Expired)
        {
            throw new DomainException("Cannot cancel an expired subscription.");
        }

        if (Tier == SubscriptionTier.Free)
        {
            throw new DomainException("Free-tier subscriptions cannot be cancelled.");
        }

        Status = SubscriptionStatus.Cancelled;
        AutoRenew = false;
    }

    /// <summary>
    /// Enters a grace period (e.g., when a team subscription is cancelled but still has time left).
    /// </summary>
    public void EnterGracePeriod(DateTimeOffset gracePeriodEnd)
    {
        if (Status == SubscriptionStatus.GracePeriod)
        {
            throw new DomainException("Already in grace period.");
        }

        if (Tier == SubscriptionTier.Free)
        {
            throw new DomainException("Free-tier subscriptions do not have a grace period.");
        }

        Status = SubscriptionStatus.GracePeriod;
        ExpiresAt = gracePeriodEnd;
        AutoRenew = false;
    }

    /// <summary>
    /// Downgrades the subscription to a specified tier.
    /// </summary>
    public void DowngradeTo(SubscriptionTier targetTier)
    {
        if (targetTier >= Tier)
        {
            throw new DomainException($"Cannot downgrade from {Tier} to {targetTier}.");
        }

        if (targetTier == SubscriptionTier.Free)
        {
            Tier = SubscriptionTier.Free;
            Status = SubscriptionStatus.Active;
            ExpiresAt = null;
            AutoRenew = false;
        }
        else
        {
            Tier = targetTier;
        }
    }

    /// <summary>
    /// Renews the subscription with a new expiry date.
    /// </summary>
    public void Renew(DateTimeOffset newExpiresAt)
    {
        if (Tier == SubscriptionTier.Free)
        {
            throw new DomainException("Free-tier subscriptions do not need renewal.");
        }

        Status = SubscriptionStatus.Active;
        ExpiresAt = newExpiresAt;
        AutoRenew = true;
    }

    /// <summary>
    /// Whether this subscription grants premium features.
    /// </summary>
    public bool IsPremium => Tier == SubscriptionTier.Pro || Tier == SubscriptionTier.Team;

    /// <summary>
    /// Whether this subscription is currently active (not expired or cancelled without grace).
    /// </summary>
    public bool IsActive => Status == SubscriptionStatus.Active || Status == SubscriptionStatus.GracePeriod;
}
