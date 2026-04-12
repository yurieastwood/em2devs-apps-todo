using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Value object representing user notification preferences.
/// Includes category toggles, quiet hours configuration, and timezone.
/// </summary>
public sealed record NotificationSettings
{
    private static readonly IReadOnlyDictionary<NotificationCategory, bool> _defaultToggles =
        new Dictionary<NotificationCategory, bool>
        {
            { NotificationCategory.TaskReminders, true },
            { NotificationCategory.AchievementAlerts, true },
            { NotificationCategory.DailyBriefReady, true },
            { NotificationCategory.WeeklyReviewPrompt, true },
            { NotificationCategory.GuildActivity, true },
            { NotificationCategory.PartnerMessages, true },
            { NotificationCategory.InsightCards, true },
            { NotificationCategory.CapacityWarnings, true },
            { NotificationCategory.UpgradePrompts, false },
        };

    private readonly IReadOnlyDictionary<NotificationCategory, bool> _categoryToggles;

    public TimeOnly? QuietHoursStart { get; }
    public TimeOnly? QuietHoursEnd { get; }
    public string TimeZoneId { get; }

    private NotificationSettings(
        IReadOnlyDictionary<NotificationCategory, bool> categoryToggles,
        TimeOnly? quietHoursStart,
        TimeOnly? quietHoursEnd,
        string timeZoneId)
    {
        _categoryToggles = categoryToggles;
        QuietHoursStart = quietHoursStart;
        QuietHoursEnd = quietHoursEnd;
        TimeZoneId = timeZoneId;
    }

    /// <summary>
    /// Creates default notification settings with all categories at their default values
    /// and no quiet hours configured.
    /// </summary>
    public static NotificationSettings CreateDefault()
    {
        return new NotificationSettings(
            new Dictionary<NotificationCategory, bool>(_defaultToggles),
            null,
            null,
            "UTC");
    }

    /// <summary>
    /// Creates notification settings with the specified category toggles.
    /// Any categories not specified use their default values.
    /// </summary>
    public static NotificationSettings Create(
        IReadOnlyDictionary<NotificationCategory, bool> categoryToggles,
        TimeOnly? quietHoursStart = null,
        TimeOnly? quietHoursEnd = null,
        string? timeZoneId = null)
    {
        ArgumentNullException.ThrowIfNull(categoryToggles);

        string effectiveTimeZoneId = timeZoneId ?? "UTC";

        ValidateTimeZone(effectiveTimeZoneId);
        ValidateQuietHours(quietHoursStart, quietHoursEnd);

        Dictionary<NotificationCategory, bool> merged = new Dictionary<NotificationCategory, bool>(_defaultToggles);
        foreach (KeyValuePair<NotificationCategory, bool> kvp in categoryToggles)
        {
            merged[kvp.Key] = kvp.Value;
        }

        return new NotificationSettings(merged, quietHoursStart, quietHoursEnd, effectiveTimeZoneId);
    }

    /// <summary>
    /// Checks whether the given notification category is enabled.
    /// </summary>
    public bool IsCategoryEnabled(NotificationCategory category)
    {
        return _categoryToggles.TryGetValue(category, out bool enabled) && enabled;
    }

    /// <summary>
    /// Returns a new NotificationSettings with the specified category toggled.
    /// </summary>
    public NotificationSettings WithCategoryToggle(NotificationCategory category, bool enabled)
    {
        Dictionary<NotificationCategory, bool> updated = new Dictionary<NotificationCategory, bool>(_categoryToggles)
        {
            [category] = enabled
        };

        return new NotificationSettings(updated, QuietHoursStart, QuietHoursEnd, TimeZoneId);
    }

    /// <summary>
    /// Returns a new NotificationSettings with quiet hours configured.
    /// </summary>
    public NotificationSettings WithQuietHours(TimeOnly start, TimeOnly end)
    {
        return new NotificationSettings(
            new Dictionary<NotificationCategory, bool>(_categoryToggles),
            start,
            end,
            TimeZoneId);
    }

    /// <summary>
    /// Returns a new NotificationSettings with the specified timezone.
    /// </summary>
    public NotificationSettings WithTimeZone(string timeZoneId)
    {
        ValidateTimeZone(timeZoneId);
        return new NotificationSettings(
            new Dictionary<NotificationCategory, bool>(_categoryToggles),
            QuietHoursStart,
            QuietHoursEnd,
            timeZoneId);
    }

    /// <summary>
    /// Returns whether quiet hours are currently configured.
    /// </summary>
    public bool HasQuietHours => QuietHoursStart.HasValue;

    /// <summary>
    /// Determines whether the given time falls within the configured quiet hours,
    /// converting from UTC to the user's timezone.
    /// </summary>
    public bool IsInQuietHours(DateTimeOffset utcTime)
    {
        if (!HasQuietHours)
        {
            return false;
        }

        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime.UtcDateTime, timeZone);
        TimeOnly localTimeOnly = TimeOnly.FromDateTime(localTime);

        TimeOnly start = QuietHoursStart!.Value;
        TimeOnly end = QuietHoursEnd!.Value;

        // Handle overnight quiet hours (e.g., 10 PM to 7 AM)
        if (start > end)
        {
            return localTimeOnly >= start || localTimeOnly < end;
        }

        // Handle same-day quiet hours (e.g., 1 PM to 5 PM)
        return localTimeOnly >= start && localTimeOnly < end;
    }

    /// <summary>
    /// Returns the end of the current quiet hours window in UTC,
    /// which is when queued notifications should be delivered.
    /// </summary>
    public DateTimeOffset? GetQuietHoursEndUtc(DateTimeOffset utcTime)
    {
        if (!HasQuietHours)
        {
            return null;
        }

        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime.UtcDateTime, timeZone);
        TimeOnly localTimeOnly = TimeOnly.FromDateTime(localTime);

        TimeOnly start = QuietHoursStart!.Value;
        TimeOnly end = QuietHoursEnd!.Value;

        DateOnly deliveryDate;

        if (start > end)
        {
            // Overnight: if current time >= start, delivery is next day at end
            // If current time < end, delivery is same day at end
            deliveryDate = localTimeOnly >= start
                ? DateOnly.FromDateTime(localTime).AddDays(1)
                : DateOnly.FromDateTime(localTime);
        }
        else
        {
            // Same-day: delivery is same day at end
            deliveryDate = DateOnly.FromDateTime(localTime);
        }

        DateTime deliveryLocal = deliveryDate.ToDateTime(end, DateTimeKind.Unspecified);
        DateTime deliveryUtc = TimeZoneInfo.ConvertTimeToUtc(deliveryLocal, timeZone);
        return new DateTimeOffset(deliveryUtc, TimeSpan.Zero);
    }

    private static void ValidateTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new DomainException("TimeZone identifier cannot be empty.");
        }

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new DomainException($"Invalid timezone: '{timeZoneId}'.");
        }
    }

    private static void ValidateQuietHours(TimeOnly? start, TimeOnly? end)
    {
        if (start.HasValue != end.HasValue)
        {
            throw new DomainException("Both quiet hours start and end must be specified, or neither.");
        }
    }
}
