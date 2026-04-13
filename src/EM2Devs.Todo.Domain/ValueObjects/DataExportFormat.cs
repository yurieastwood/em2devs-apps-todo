namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Supported data export formats.
/// Maps to: docs/features/data/local-first-data.feature — "Export all data as JSON" / "Export tasks as CSV"
/// </summary>
public enum DataExportFormat
{
    Json,
    Csv,
}
