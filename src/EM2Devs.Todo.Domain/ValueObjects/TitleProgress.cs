namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents the evaluation result of a title requirement, including whether it has been
/// earned and the progress toward earning it.
/// </summary>
public sealed record TitleProgress
{
    public TitleType TitleType { get; }
    public bool IsEarned { get; }
    public int ProgressPercentage { get; }
    public string RemainingDescription { get; }

    private TitleProgress(TitleType titleType, bool isEarned, int progressPercentage, string remainingDescription)
    {
        TitleType = titleType;
        IsEarned = isEarned;
        ProgressPercentage = progressPercentage;
        RemainingDescription = remainingDescription;
    }

    internal static TitleProgress Earned(TitleType titleType) =>
        new(titleType, isEarned: true, progressPercentage: 100, remainingDescription: string.Empty);

    internal static TitleProgress InProgress(
        TitleType titleType,
        int currentCount,
        int requiredCount,
        int currentDistinctDays,
        int requiredDistinctDays,
        string actionLabel)
    {
        int countProgress = (int)((long)currentCount * 100 / requiredCount);
        int daysProgress = (int)((long)currentDistinctDays * 100 / requiredDistinctDays);

        int overallProgress = Math.Min(countProgress, daysProgress);
        overallProgress = Math.Min(overallProgress, 99);
        overallProgress = Math.Max(overallProgress, 0);

        string remaining = BuildRemainingDescription(
            currentCount, requiredCount,
            currentDistinctDays, requiredDistinctDays,
            actionLabel);

        return new TitleProgress(titleType, isEarned: false, progressPercentage: overallProgress, remainingDescription: remaining);
    }

    private static string BuildRemainingDescription(
        int currentCount,
        int requiredCount,
        int currentDistinctDays,
        int requiredDistinctDays,
        string actionLabel)
    {
        if (currentCount < requiredCount)
        {
            int remaining = requiredCount - currentCount;
            return $"{remaining} more {actionLabel} needed";
        }

        int daysRemaining = requiredDistinctDays - currentDistinctDays;
        return $"{daysRemaining} more days of consistent completions needed";
    }
}
