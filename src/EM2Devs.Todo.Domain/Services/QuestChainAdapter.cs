using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Pure domain service that evolves a quest chain's template based on observed behaviour
/// across its instances.
/// </summary>
public static class QuestChainAdapter
{
    /// <summary>
    /// Detects a recurring weekly quest pattern from a sequence of past quest completions.
    /// Returns <c>null</c> if no stable cadence is detected.
    ///
    /// A weekly pattern is detected when the same title appears at least 3 times and the
    /// gaps between consecutive completions are all within 6-8 days.
    /// </summary>
    public static QuestChainPattern? DetectPattern(IEnumerable<QuestCompletionRecord> history)
    {
        ArgumentNullException.ThrowIfNull(history);

        IEnumerable<IGrouping<string, QuestCompletionRecord>> groups = history
            .GroupBy(h => h.Title.Value);

        foreach (IGrouping<string, QuestCompletionRecord> group in groups)
        {
            List<QuestCompletionRecord> ordered = [.. group.OrderBy(r => r.CompletedOn)];
            if (ordered.Count < 3)
            {
                continue;
            }

            if (AllGapsWeekly(ordered))
            {
                return new QuestChainPattern(ordered[0].Title, RecurrencePattern.Weekly, ordered.Count);
            }
        }

        return null;
    }

    private static bool AllGapsWeekly(List<QuestCompletionRecord> ordered)
    {
        for (int i = 1; i < ordered.Count; i++)
        {
            int gap = ordered[i].CompletedOn.DayNumber - ordered[i - 1].CompletedOn.DayNumber;
            if (gap < 6 || gap > 8)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Detects tasks that have been manually added to the last <paramref name="minOccurrences"/>
    /// instances of the chain but are not yet part of its template. Useful for suggesting
    /// template evolution.
    /// </summary>
    public static IReadOnlyList<TaskTitle> SuggestTemplateAdditions(
        QuestChain chain,
        IReadOnlyList<IReadOnlyList<TaskTitle>> recentInstanceTasks,
        int minOccurrences)
    {
        ArgumentNullException.ThrowIfNull(chain);
        ArgumentNullException.ThrowIfNull(recentInstanceTasks);

        if (minOccurrences <= 0)
        {
            throw new Exceptions.DomainException("Minimum occurrences must be positive.");
        }

        HashSet<string> templateTitles = [.. chain.TaskTemplate.Select(t => t.Value)];
        Dictionary<string, (int Count, TaskTitle Title)> counts = [];

        foreach (IReadOnlyList<TaskTitle> instanceTasks in recentInstanceTasks)
        {
            HashSet<string> seenInInstance = [];
            foreach (TaskTitle title in instanceTasks)
            {
                if (templateTitles.Contains(title.Value))
                {
                    continue;
                }

                if (!seenInInstance.Add(title.Value))
                {
                    continue;
                }

                if (counts.TryGetValue(title.Value, out (int Count, TaskTitle Title) existing))
                {
                    counts[title.Value] = (existing.Count + 1, existing.Title);
                }
                else
                {
                    counts[title.Value] = (1, title);
                }
            }
        }

        return [.. counts.Values.Where(v => v.Count >= minOccurrences).Select(v => v.Title)];
    }
}
