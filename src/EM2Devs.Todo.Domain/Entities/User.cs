using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// Authenticated user aggregate (Phase 0 multi-user JWT auth migration).
/// Holds identity credentials and display profile. Separate from <see cref="PlayerProfile"/>,
/// which is the gamification projection keyed by <see cref="UserId"/>.
/// </summary>
public sealed class User
{
    /// <summary>Maximum length of an RFC 5321 email address.</summary>
    public const int MaxEmailLength = 254;

    /// <summary>Minimum length of a display name.</summary>
    public const int MinDisplayNameLength = 1;

    /// <summary>Maximum length of a display name.</summary>
    public const int MaxDisplayNameLength = 100;

    public UserId Id { get; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string DisplayName { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? DeactivatedAt { get; private set; }

    /// <summary>True when the account is in its post-deletion holding period.</summary>
    public bool IsDeactivated => DeactivatedAt.HasValue;

    private User(UserId id, string email, string passwordHash, string displayName, DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        DisplayName = displayName;
        CreatedAt = createdAt;
    }

    // Stryker disable all : EF Core materialization constructor — not a behavioural surface.
#pragma warning disable CS8618 // Non-nullable members set by EF via reflection.
    private User(UserId id, DateTimeOffset createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }
#pragma warning restore CS8618
    // Stryker restore all

    /// <summary>
    /// Factory: validates inputs and constructs a new <see cref="User"/>.
    /// </summary>
    public static User Create(
        string email,
        string passwordHash,
        string displayName,
        DateTimeOffset createdAt,
        UserId? id = null)
    {
        string validatedEmail = ValidateEmail(email);
        string validatedHash = ValidatePasswordHash(passwordHash);
        string validatedName = ValidateDisplayName(displayName);

        return new User(id ?? UserId.New(), validatedEmail, validatedHash, validatedName, createdAt);
    }

    /// <summary>
    /// Replaces the stored password hash. Caller is responsible for hashing plaintext.
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = ValidatePasswordHash(newPasswordHash);
    }

    /// <summary>
    /// Updates the user's display name. Validates length and non-empty.
    /// </summary>
    public void UpdateDisplayName(string newDisplayName)
    {
        DisplayName = ValidateDisplayName(newDisplayName);
    }

    /// <summary>
    /// Marks the account as deactivated at the given instant. The user row is preserved
    /// so the email/displayName cannot be immediately reclaimed; cleanup happens after
    /// the 30-day holding period (see <see cref="HoldingPeriodElapsed"/>).
    /// </summary>
    public void Deactivate(DateTimeOffset at)
    {
        if (at == default)
        {
            throw new DomainException("Deactivation timestamp cannot be default.");
        }

        if (DeactivatedAt.HasValue)
        {
            throw new DomainException("Account is already deactivated.");
        }

        DeactivatedAt = at;
    }

    /// <summary>Length of the post-deletion holding period before the email is released.</summary>
    public static readonly TimeSpan HoldingPeriod = TimeSpan.FromDays(30);

    /// <summary>
    /// Returns true when this account was deactivated more than the holding period ago,
    /// meaning the email/displayName can be reclaimed by a new registration.
    /// </summary>
    public bool HoldingPeriodElapsed(DateTimeOffset now)
    {
        return DeactivatedAt.HasValue && now - DeactivatedAt.Value >= HoldingPeriod;
    }

    /// <summary>
    /// Cancels a pending deactivation and returns the account to active state.
    /// Throws when the account is not currently deactivated (paired invariant with
    /// <see cref="Deactivate"/>).
    /// </summary>
    public void Reactivate()
    {
        if (!DeactivatedAt.HasValue)
        {
            throw new DomainException("Account is not deactivated.");
        }

        DeactivatedAt = null;
    }

    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Email cannot be empty.");
        }

        if (email.Length > MaxEmailLength)
        {
            throw new DomainException($"Email cannot exceed {MaxEmailLength} characters.");
        }

        if (!email.Contains('@', StringComparison.Ordinal))
        {
            throw new DomainException("Email must contain '@'.");
        }

        return email;
    }

    private static string ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash cannot be empty.");
        }

        return passwordHash;
    }

    private static string ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainException("Display name cannot be empty.");
        }

        if (displayName.Length > MaxDisplayNameLength)
        {
            throw new DomainException($"Display name cannot exceed {MaxDisplayNameLength} characters.");
        }

        return displayName;
    }
}
