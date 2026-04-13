using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object representing data privacy and ownership settings.
/// Enforces local-first defaults: sync disabled, telemetry disabled, no export schedule.
/// Maps to: docs/features/data/local-first-data.feature — "No data sent to servers without explicit opt-in"
/// </summary>
public sealed record DataPrivacySettings
{
    public bool SyncEnabled { get; }
    public bool TelemetryEnabled { get; }
    public ExportSchedule? ExportSchedule { get; }

    public DataPrivacySettings(bool syncEnabled, bool telemetryEnabled, ExportSchedule? exportSchedule)
    {
        ExportSchedule = exportSchedule;
        SyncEnabled = syncEnabled;
        TelemetryEnabled = telemetryEnabled;
    }

    /// <summary>
    /// Creates default privacy settings — all sharing/sync disabled (local-first).
    /// </summary>
    public static DataPrivacySettings CreateDefault()
    {
        return new DataPrivacySettings(false, false, null);
    }

    /// <summary>
    /// Enables cloud sync (requires premium). Returns new instance.
    /// </summary>
    public DataPrivacySettings EnableSync()
    {
        return new DataPrivacySettings(true, TelemetryEnabled, ExportSchedule);
    }

    /// <summary>
    /// Disables cloud sync. Returns new instance.
    /// </summary>
    public DataPrivacySettings DisableSync()
    {
        return new DataPrivacySettings(false, TelemetryEnabled, ExportSchedule);
    }

    /// <summary>
    /// Enables telemetry. Returns new instance.
    /// </summary>
    public DataPrivacySettings EnableTelemetry()
    {
        return new DataPrivacySettings(SyncEnabled, true, ExportSchedule);
    }

    /// <summary>
    /// Disables telemetry. Returns new instance.
    /// </summary>
    public DataPrivacySettings DisableTelemetry()
    {
        return new DataPrivacySettings(SyncEnabled, false, ExportSchedule);
    }

    /// <summary>
    /// Configures a scheduled export. Returns new instance.
    /// </summary>
    public DataPrivacySettings WithExportSchedule(ExportSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        return new DataPrivacySettings(SyncEnabled, TelemetryEnabled, schedule);
    }

    /// <summary>
    /// Removes the scheduled export. Returns new instance.
    /// </summary>
    public DataPrivacySettings WithoutExportSchedule()
    {
        return new DataPrivacySettings(SyncEnabled, TelemetryEnabled, null);
    }

    /// <summary>
    /// Whether any data is being shared externally (sync or telemetry).
    /// </summary>
    public bool IsFullyLocal => !SyncEnabled && !TelemetryEnabled;

    /// <summary>
    /// Whether a scheduled export is configured.
    /// </summary>
    public bool HasExportSchedule => ExportSchedule is not null;
}
