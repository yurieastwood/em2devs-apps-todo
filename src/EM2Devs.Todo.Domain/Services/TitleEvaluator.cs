using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Pure domain service that evaluates whether a player has earned a title
/// based on sustained behaviour requirements.
/// Titles require both a minimum action count and actions spread over a minimum
/// number of distinct days, preventing burst achievements.
/// </summary>
public static class TitleEvaluator
{
    /// <summary>
    /// Evaluates a title requirement against a collection of qualifying actions.
    /// Returns a <see cref="TitleProgress"/> indicating whether the title is earned
    /// and showing progress toward earning it.
    /// </summary>
    public static TitleProgress Evaluate(
        TitleRequirement requirement,
        IReadOnlyCollection<TitleQualifyingAction> actions,
        DateOnly evaluationDate)
    {
        ArgumentNullException.ThrowIfNull(requirement);
        ArgumentNullException.ThrowIfNull(actions);

        int count = actions.Count;
        int distinctDays = actions
            .Select(a => a.OccurredOn)
            .Distinct()
            .Count();

        bool countMet = count >= requirement.RequiredCount;
        bool daysMet = distinctDays >= requirement.RequiredDistinctDays;

        if (countMet && daysMet)
        {
            return TitleProgress.Earned(requirement.TitleType);
        }

        return TitleProgress.InProgress(
            requirement.TitleType,
            count,
            requirement.RequiredCount,
            distinctDays,
            requirement.RequiredDistinctDays,
            requirement.ActionLabel);
    }
}
