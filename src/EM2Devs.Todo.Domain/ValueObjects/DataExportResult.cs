using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object representing the result of a data export operation.
/// Contains the exported content, format metadata, and record count.
/// Maps to: docs/features/data/local-first-data.feature — "Export all data as JSON" / "Export tasks as CSV"
/// </summary>
public sealed record DataExportResult
{
    public string Content { get; }
    public DataExportFormat Format { get; }
    public DateTimeOffset ExportedAt { get; }
    public int RecordCount { get; }

    public DataExportResult(string content, DataExportFormat format, DateTimeOffset exportedAt, int recordCount)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (!Enum.IsDefined(format))
        {
            throw new DomainException("Invalid export format.");
        }

        if (exportedAt == default)
        {
            throw new DomainException("Export timestamp cannot be default.");
        }

        if (recordCount < 0)
        {
            throw new DomainException("Record count cannot be negative.");
        }

        Content = content;
        Format = format;
        ExportedAt = exportedAt;
        RecordCount = recordCount;
    }

    /// <summary>
    /// Whether this export contains any records.
    /// </summary>
    public bool HasRecords => RecordCount > 0;
}
