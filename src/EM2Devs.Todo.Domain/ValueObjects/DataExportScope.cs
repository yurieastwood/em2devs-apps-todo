namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Scope of data to include in an export.
/// Maps to: docs/features/data/local-first-data.feature — "Export all data as JSON" / "Export tasks as CSV"
/// </summary>
public enum DataExportScope
{
    All,
    TasksOnly,
}
