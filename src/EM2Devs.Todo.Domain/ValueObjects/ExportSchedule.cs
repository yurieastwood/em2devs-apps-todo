using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object representing a scheduled automatic export configuration.
/// Premium feature for recurring data backups.
/// Maps to: docs/features/data/local-first-data.feature — "Scheduled automatic export"
/// </summary>
public sealed record ExportSchedule
{
    /// <summary>
    /// Maximum number of recent backups to retain.
    /// </summary>
    public const int MaxRetainedBackups = 4;

    public DayOfWeek DayOfWeek { get; }
    public string LocalDirectory { get; }
    public int RetainedBackups { get; }

    public ExportSchedule(DayOfWeek dayOfWeek, string localDirectory, int retainedBackups)
    {
        if (!Enum.IsDefined(dayOfWeek))
        {
            throw new DomainException("Invalid day of week for export schedule.");
        }

        if (string.IsNullOrWhiteSpace(localDirectory))
        {
            throw new DomainException("Local directory for export cannot be empty.");
        }

        if (retainedBackups < 1)
        {
            throw new DomainException("Must retain at least 1 backup.");
        }

        if (retainedBackups > MaxRetainedBackups)
        {
            throw new DomainException($"Cannot retain more than {MaxRetainedBackups} backups.");
        }

        DayOfWeek = dayOfWeek;
        LocalDirectory = localDirectory;
        RetainedBackups = retainedBackups;
    }

    /// <summary>
    /// Creates a weekly export schedule with default retention (4 backups).
    /// </summary>
    public static ExportSchedule Weekly(DayOfWeek dayOfWeek, string localDirectory)
    {
        return new ExportSchedule(dayOfWeek, localDirectory, MaxRetainedBackups);
    }
}
