namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Strategy for resolving sync conflicts between devices.
/// Maps to: docs/features/data/local-first-data.feature — "Sync conflict resolution"
/// </summary>
public enum ConflictResolutionStrategy
{
    LastWriteWins,
}
