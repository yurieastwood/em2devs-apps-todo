using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object representing a request to export user data.
/// Captures the format, scope, and timestamp of the request.
/// Maps to: docs/features/data/local-first-data.feature — "Export all data as JSON" / "Export tasks as CSV"
/// </summary>
public sealed record DataExportRequest
{
    public DataExportFormat Format { get; }
    public DataExportScope Scope { get; }
    public DateTimeOffset RequestedAt { get; }

    public DataExportRequest(DataExportFormat format, DataExportScope scope, DateTimeOffset requestedAt)
    {
        if (!Enum.IsDefined(format))
        {
            throw new DomainException("Invalid export format.");
        }

        if (!Enum.IsDefined(scope))
        {
            throw new DomainException("Invalid export scope.");
        }

        if (requestedAt == default)
        {
            throw new DomainException("Export request timestamp cannot be default.");
        }

        Format = format;
        Scope = scope;
        RequestedAt = requestedAt;
    }

    /// <summary>
    /// Creates a JSON export request for all data.
    /// </summary>
    public static DataExportRequest AllAsJson(DateTimeOffset requestedAt)
    {
        return new DataExportRequest(DataExportFormat.Json, DataExportScope.All, requestedAt);
    }

    /// <summary>
    /// Creates a CSV export request for tasks only.
    /// </summary>
    public static DataExportRequest TasksAsCsv(DateTimeOffset requestedAt)
    {
        return new DataExportRequest(DataExportFormat.Csv, DataExportScope.TasksOnly, requestedAt);
    }
}
