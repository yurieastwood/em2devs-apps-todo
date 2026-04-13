using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Pure domain service that evaluates tasks for procrastination signals.
/// No infrastructure dependencies — all decisions are based on task state and completed task context.
/// </summary>
public static class ProcrastinationEvaluator
{
    private const int RescheduleThreshold = 3;
    private const int ViewWithoutActionThreshold = 5;
    private const int OverdueDaysThreshold = 7;
    private const int SkippedDaysThreshold = 4;
    private const int CompletedLowerPriorityThreshold = 3;

    private const int RescheduleWeight = 2;
    private const int ViewWithoutActionWeight = 1;
    private const int HighPrioritySkippedWeight = 3;
    private const int OverdueWeight = 2;

    private static readonly string[] _forbiddenWords = ["failure", "lazy", "behind", "overdue guilt"];

    /// <summary>
    /// Evaluates a single task for procrastination signals.
    /// Returns a ProcrastinationCandidate if any signals are detected, null otherwise.
    /// </summary>
    public static ProcrastinationCandidate? Evaluate(
        TodoTask task,
        int completedLowerPriorityTaskCount = 0,
        int consecutiveDaysInTodayView = 0,
        DateTimeOffset? evaluationDate = null)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.Status == TaskStatus.Done || task.Status == TaskStatus.Skipped)
        {
            return null;
        }

        if (task.WaitingReason is not null)
        {
            return null;
        }

        var signals = DetectSignals(task, completedLowerPriorityTaskCount,
            consecutiveDaysInTodayView, evaluationDate);

        if (signals.Count == 0)
        {
            return null;
        }

        foreach (var signal in signals)
        {
            task.AddProcrastinationSignal(signal);
        }

        var interventions = BuildInterventions();
        return new ProcrastinationCandidate(task.Id, signals, interventions);
    }

    /// <summary>
    /// Detects procrastination signals for a given task.
    /// </summary>
    public static List<ProcrastinationSignal> DetectSignals(
        TodoTask task,
        int completedLowerPriorityTaskCount = 0,
        int consecutiveDaysInTodayView = 0,
        DateTimeOffset? evaluationDate = null)
    {
        ArgumentNullException.ThrowIfNull(task);

        var signals = new List<ProcrastinationSignal>();

        if (IsRescheduledRepeatedly(task))
        {
            signals.Add(new ProcrastinationSignal(ProcrastinationSignalType.RepeatedRescheduling, RescheduleWeight));
        }

        if (IsViewedRepeatedlyWithoutAction(task))
        {
            signals.Add(new ProcrastinationSignal(ProcrastinationSignalType.RepeatedViewingWithoutAction, ViewWithoutActionWeight));
        }

        if (IsHighPrioritySkipped(task, completedLowerPriorityTaskCount, consecutiveDaysInTodayView))
        {
            signals.Add(new ProcrastinationSignal(ProcrastinationSignalType.HighPrioritySkipped, HighPrioritySkippedWeight));
        }

        if (IsOverduePastThreshold(task, evaluationDate))
        {
            signals.Add(new ProcrastinationSignal(ProcrastinationSignalType.OverduePastThreshold, OverdueWeight));
        }

        return signals;
    }

    /// <summary>
    /// Builds the standard set of intervention options with supportive messages.
    /// </summary>
    public static IReadOnlyList<InterventionOption> BuildInterventions()
    {
        return
        [
            new InterventionOption(InterventionOptionType.BreakItDown,
                "This task can feel overwhelming. Try breaking it into smaller, less intimidating subtasks."),
            new InterventionOption(InterventionOptionType.Delegate,
                "You do not have to do everything alone. Consider converting this to a shared quest or assigning it to someone."),
            new InterventionOption(InterventionOptionType.ReEvaluate,
                "It is okay to change your mind. Take a moment to decide if this task still matters to you."),
            new InterventionOption(InterventionOptionType.BossTaskIt,
                "Ready for a challenge? Promote this to a Boss Task for focused attack with bonus XP."),
            new InterventionOption(InterventionOptionType.RescheduleWithIntent,
                "Sometimes timing matters. Set a specific date with a commitment note to keep yourself on track.")
        ];
    }

    /// <summary>
    /// Generates a supportive intervention message for a procrastination candidate.
    /// The message acknowledges difficulty and suggests actionable next steps.
    /// Never contains shaming language.
    /// </summary>
    public static string GenerateInterventionMessage(ProcrastinationCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        return "This task can feel overwhelming, and that is completely okay. "
            + "Try starting with just the first step. "
            + "You have several options to help you move forward.";
    }

    /// <summary>
    /// Validates that a message uses supportive language and does not contain shaming words.
    /// </summary>
    public static bool IsSupportiveMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        string lowerMessage = message.ToUpperInvariant().ToLowerInvariant();
        foreach (string word in _forbiddenWords)
        {
            if (lowerMessage.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Generates subtask suggestions for breaking down a procrastinated task.
    /// </summary>
    public static IReadOnlyList<string> SuggestSubtasks(TodoTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return
        [
            $"Research and gather information for: {task.Title.Value}",
            $"Create an outline or plan for: {task.Title.Value}",
            $"Complete the first draft of: {task.Title.Value}",
            $"Review and finalise: {task.Title.Value}"
        ];
    }

    /// <summary>
    /// Generates re-evaluation prompts to help the user assess a procrastinated task.
    /// </summary>
    public static IReadOnlyList<string> GetReEvaluationPrompts()
    {
        return
        [
            "Does this still need to happen?",
            "What would the consequence be if you never did this?",
            "Is someone else depending on this?",
            "Would you add this task today if it were not already here?"
        ];
    }

    /// <summary>
    /// Aggregates procrastination patterns from a collection of tasks.
    /// Returns insights about most avoided categories and intervention success rate.
    /// </summary>
    public static ProcrastinationPatterns AnalysePatterns(
        IReadOnlyList<TodoTask> allTasks,
        IReadOnlyList<TodoTask> completedAfterIntervention)
    {
        ArgumentNullException.ThrowIfNull(allTasks);
        ArgumentNullException.ThrowIfNull(completedAfterIntervention);

        var procrastinatedTasks = allTasks
            .Where(t => t.ProcrastinationSignals.Count > 0)
            .ToList();

        int totalProcrastinated = procrastinatedTasks.Count;
        int completedCount = completedAfterIntervention.Count;

        double successRate = totalProcrastinated > 0
            ? (double)completedCount / totalProcrastinated * 100
            : 0;

        ProcrastinationSignalType? mostCommonSignal = procrastinatedTasks
            .SelectMany(t => t.ProcrastinationSignals)
            .GroupBy(s => s.Type)
            .OrderByDescending(g => g.Count())
            .Select(g => (ProcrastinationSignalType?)g.Key)
            .FirstOrDefault();

        return new ProcrastinationPatterns(
            totalProcrastinated,
            Math.Round(successRate, 1),
            mostCommonSignal);
    }

    private static bool IsRescheduledRepeatedly(TodoTask task)
    {
        return task.RescheduleCount >= RescheduleThreshold;
    }

    private static bool IsViewedRepeatedlyWithoutAction(TodoTask task)
    {
        return task.ViewCount >= ViewWithoutActionThreshold && task.Status == TaskStatus.Todo;
    }

    private static bool IsHighPrioritySkipped(TodoTask task, int completedLowerPriorityTaskCount,
        int consecutiveDaysInTodayView)
    {
        bool isHighOrCritical = task.Priority is TaskPriority.High or TaskPriority.Critical;
        if (!isHighOrCritical)
        {
            return false;
        }

        return consecutiveDaysInTodayView >= SkippedDaysThreshold
            && completedLowerPriorityTaskCount >= CompletedLowerPriorityThreshold
            && task.Status == TaskStatus.Todo;
    }

    private static bool IsOverduePastThreshold(TodoTask task, DateTimeOffset? evaluationDate)
    {
        if (!task.DueDate.HasValue)
        {
            return false;
        }

        var now = evaluationDate ?? DateTimeOffset.UtcNow;
        int overdueDays = (int)(now - task.DueDate.Value).TotalDays;
        return overdueDays >= OverdueDaysThreshold;
    }
}

/// <summary>
/// Aggregated procrastination pattern insights.
/// </summary>
public sealed record ProcrastinationPatterns(
    int TotalProcrastinatedTasks,
    double InterventionSuccessRate,
    ProcrastinationSignalType? MostCommonSignalType);
