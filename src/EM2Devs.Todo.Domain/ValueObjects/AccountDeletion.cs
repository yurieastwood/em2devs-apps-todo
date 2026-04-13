using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object representing an account deletion request with a 30-day holding period.
/// During the holding period, the account can be recovered.
/// Maps to: docs/features/data/local-first-data.feature — "Delete account entirely" / "Recover account during the 30-day holding period"
/// </summary>
public sealed record AccountDeletion
{
    /// <summary>
    /// The holding period duration before permanent deletion.
    /// </summary>
    public static readonly TimeSpan HoldingPeriod = TimeSpan.FromDays(30);

    public DateTimeOffset RequestedAt { get; }
    public DateTimeOffset ScheduledPurgeAt { get; }
    public bool Recovered { get; }

    public AccountDeletion(DateTimeOffset requestedAt, DateTimeOffset scheduledPurgeAt, bool recovered)
    {
        if (requestedAt == default)
        {
            throw new DomainException("Deletion request timestamp cannot be default.");
        }

        if (scheduledPurgeAt == default)
        {
            throw new DomainException("Scheduled purge timestamp cannot be default.");
        }

        if (scheduledPurgeAt <= requestedAt)
        {
            throw new DomainException("Scheduled purge must be after the deletion request.");
        }

        RequestedAt = requestedAt;
        ScheduledPurgeAt = scheduledPurgeAt;
        Recovered = recovered;
    }

    /// <summary>
    /// Creates a new account deletion request with a 30-day holding period.
    /// </summary>
    public static AccountDeletion Request(DateTimeOffset requestedAt)
    {
        return new AccountDeletion(requestedAt, requestedAt.Add(HoldingPeriod), false);
    }

    /// <summary>
    /// Recovers the account during the holding period.
    /// </summary>
    public AccountDeletion Recover(DateTimeOffset now)
    {
        if (Recovered)
        {
            throw new DomainException("Account has already been recovered.");
        }

        if (now >= ScheduledPurgeAt)
        {
            throw new DomainException("Cannot recover account after the holding period has expired.");
        }

        return new AccountDeletion(RequestedAt, ScheduledPurgeAt, true);
    }

    /// <summary>
    /// Whether the holding period has expired and the account should be permanently deleted.
    /// </summary>
    public bool IsPurgeOverdue(DateTimeOffset now)
    {
        return !Recovered && now >= ScheduledPurgeAt;
    }

    /// <summary>
    /// Whether the account is still within the recoverable holding period.
    /// </summary>
    public bool IsRecoverable(DateTimeOffset now)
    {
        return !Recovered && now < ScheduledPurgeAt;
    }
}
