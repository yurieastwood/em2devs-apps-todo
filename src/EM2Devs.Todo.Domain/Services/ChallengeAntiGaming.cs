using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Pure domain service that evaluates whether a task qualifies for challenge scoring.
/// Anti-gaming measures ensure fair competition by enforcing minimum quality thresholds.
/// Tasks must be Normal+ difficulty, have titles of at least 10 characters,
/// and have been open for at least 5 minutes before completion.
/// </summary>
public static class ChallengeAntiGaming
{
    /// <summary>Minimum title length for challenge-eligible tasks.</summary>
    public const int MinTitleLength = 10;

    /// <summary>Minimum time a task must be open before completion to count (in minutes).</summary>
    public const int MinOpenMinutes = 5;

    /// <summary>Minimum difficulty for challenge-eligible tasks.</summary>
    public const TaskDifficulty MinDifficulty = TaskDifficulty.Normal;

    /// <summary>
    /// Evaluates whether a completed task meets challenge eligibility requirements.
    /// Returns true if the task qualifies for challenge scoring.
    /// </summary>
    public static bool IsEligible(
        TaskTitle title,
        TaskDifficulty difficulty,
        DateTimeOffset createdAt,
        DateTimeOffset completedAt)
    {
        if (!MeetsTitleLengthRequirement(title))
        {
            return false;
        }

        if (!MeetsDifficultyThreshold(difficulty))
        {
            return false;
        }

        if (!MeetsMinimumOpenTime(createdAt, completedAt))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the task difficulty meets the minimum threshold (Normal or above).
    /// Trivial and Easy tasks are excluded from challenge scoring.
    /// </summary>
    public static bool MeetsDifficultyThreshold(TaskDifficulty difficulty)
    {
        return difficulty >= MinDifficulty;
    }

    /// <summary>
    /// Checks if the task title meets the minimum length requirement.
    /// </summary>
    public static bool MeetsTitleLengthRequirement(TaskTitle title)
    {
        ArgumentNullException.ThrowIfNull(title);
        return title.Value.Length >= MinTitleLength;
    }

    /// <summary>
    /// Checks if the task was open for at least the minimum duration before completion.
    /// </summary>
    public static bool MeetsMinimumOpenTime(DateTimeOffset createdAt, DateTimeOffset completedAt)
    {
        TimeSpan openDuration = completedAt - createdAt;
        return openDuration.TotalMinutes >= MinOpenMinutes;
    }

    /// <summary>
    /// Evaluates whether a task completion falls within a challenge's time window.
    /// Tasks completed during the challenge window count regardless of creation date.
    /// </summary>
    public static bool IsWithinChallengeWindow(
        DateTimeOffset completedAt,
        DateTimeOffset challengeStart,
        DateTimeOffset challengeEnd)
    {
        return completedAt >= challengeStart && completedAt <= challengeEnd;
    }
}
