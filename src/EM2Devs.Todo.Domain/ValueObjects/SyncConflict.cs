using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object representing a sync conflict between local and remote data versions.
/// Records both versions, the resolution applied, and when it was resolved.
/// Maps to: docs/features/data/local-first-data.feature — "Sync conflict resolution"
/// </summary>
public sealed record SyncConflict
{
    public string LocalVersion { get; }
    public string RemoteVersion { get; }
    public ConflictResolutionStrategy Resolution { get; }
    public DateTimeOffset ResolvedAt { get; }

    public SyncConflict(string localVersion, string remoteVersion, ConflictResolutionStrategy resolution, DateTimeOffset resolvedAt)
    {
        if (string.IsNullOrWhiteSpace(localVersion))
        {
            throw new DomainException("Local version cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(remoteVersion))
        {
            throw new DomainException("Remote version cannot be empty.");
        }

        if (!Enum.IsDefined(resolution))
        {
            throw new DomainException("Invalid conflict resolution strategy.");
        }

        if (resolvedAt == default)
        {
            throw new DomainException("Conflict resolution timestamp cannot be default.");
        }

        LocalVersion = localVersion;
        RemoteVersion = remoteVersion;
        Resolution = resolution;
        ResolvedAt = resolvedAt;
    }

    /// <summary>
    /// Creates a conflict resolved by last-write-wins strategy.
    /// </summary>
    public static SyncConflict ResolveLastWriteWins(string localVersion, string remoteVersion, DateTimeOffset resolvedAt)
    {
        return new SyncConflict(localVersion, remoteVersion, ConflictResolutionStrategy.LastWriteWins, resolvedAt);
    }

    /// <summary>
    /// Whether both versions are identical (false conflict).
    /// </summary>
    public bool IsIdentical => LocalVersion == RemoteVersion;
}
