using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object representing cross-device sync configuration.
/// Sync is opt-in (disabled by default) and requires premium subscription.
/// Maps to: docs/features/data/local-first-data.feature — "Enable cross-device sync" / "Disable sync and delete cloud data"
/// </summary>
public sealed record SyncSettings
{
    public bool Enabled { get; }
    public DateTimeOffset? LastSyncAt { get; }
    public ConflictResolutionStrategy ConflictResolution { get; }

    public SyncSettings(bool enabled, DateTimeOffset? lastSyncAt, ConflictResolutionStrategy conflictResolution)
    {
        if (!Enum.IsDefined(conflictResolution))
        {
            throw new DomainException("Invalid conflict resolution strategy.");
        }

        if (!enabled && lastSyncAt.HasValue)
        {
            throw new DomainException("Cannot have a last sync timestamp when sync is disabled.");
        }

        Enabled = enabled;
        LastSyncAt = lastSyncAt;
        ConflictResolution = conflictResolution;
    }

    /// <summary>
    /// Creates default sync settings (disabled, last-write-wins).
    /// </summary>
    public static SyncSettings CreateDefault()
    {
        return new SyncSettings(false, null, ConflictResolutionStrategy.LastWriteWins);
    }

    /// <summary>
    /// Enables sync with the specified conflict resolution strategy.
    /// </summary>
    public SyncSettings Enable()
    {
        return new SyncSettings(true, null, ConflictResolution);
    }

    /// <summary>
    /// Disables sync, clearing the last sync timestamp.
    /// </summary>
    public SyncSettings Disable()
    {
        return new SyncSettings(false, null, ConflictResolution);
    }

    /// <summary>
    /// Records a successful sync at the given timestamp.
    /// </summary>
    public SyncSettings RecordSync(DateTimeOffset syncedAt)
    {
        if (!Enabled)
        {
            throw new DomainException("Cannot record sync when sync is disabled.");
        }

        if (syncedAt == default)
        {
            throw new DomainException("Sync timestamp cannot be default.");
        }

        return new SyncSettings(true, syncedAt, ConflictResolution);
    }
}
