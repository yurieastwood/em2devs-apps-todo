using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// Statistics snapshot for a quest chain.
/// </summary>
public sealed record QuestChainStats(
    int TotalInstances,
    int CompletedInstances,
    decimal CompletionRate,
    TimeSpan AverageTimeToComplete,
    int ConsecutiveCompletionStreak,
    ExperiencePoints TotalXpEarned);

/// <summary>
/// Recurring quest template that automatically generates Quest instances on a given cadence,
/// records their outcomes, and rewards consistency with bonus XP multipliers.
/// </summary>
public sealed class QuestChain
{
    private readonly List<TaskTitle> _taskTemplate = [];
    private readonly List<QuestChainInstance> _history = [];
    public static readonly TimeSpan DefaultInstanceDeadline = TimeSpan.FromHours(24);

    public QuestChainId Id { get; }
    public QuestTitle Title { get; private set; }
    public RecurrencePattern Cadence { get; private set; }
    public DayOfWeek? DayOfWeek { get; }
    public ExperiencePoints TotalXpEarned { get; private set; }
    public IReadOnlyList<TaskTitle> TaskTemplate => _taskTemplate.AsReadOnly();
    public IReadOnlyList<QuestChainInstance> History => _history.AsReadOnly();

    private QuestChain(QuestChainId id, QuestTitle title, RecurrencePattern cadence,
        DayOfWeek? dayOfWeek, IEnumerable<TaskTitle> taskTemplate)
    {
        Id = id;
        Title = title;
        Cadence = cadence;
        DayOfWeek = dayOfWeek;
        _taskTemplate.AddRange(taskTemplate);
        TotalXpEarned = new ExperiencePoints(0);
    }

    public static QuestChain Create(QuestTitle title, RecurrencePattern cadence,
        IEnumerable<TaskTitle> taskTemplate, DayOfWeek? dayOfWeek = null)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(taskTemplate);

        List<TaskTitle> templates = [.. taskTemplate];
        if (templates.Count == 0)
        {
            throw new DomainException("Quest chain requires at least one task template.");
        }

        return new QuestChain(QuestChainId.New(), title, cadence, dayOfWeek, templates);
    }

    /// <summary>
    /// Generates the next chain instance as a concrete Quest whose due date is
    /// <see cref="DefaultInstanceDeadline"/> after <paramref name="scheduledOn"/>.
    /// </summary>
    public Quest GenerateInstance(Guid userId, DateOnly scheduledOn)
    {
        DateOnly deadline = scheduledOn.AddDays(1);
        DateTimeOffset taskDue = new DateTimeOffset(
            new DateTime(deadline.Year, deadline.Month, deadline.Day, 0, 0, 0, DateTimeKind.Utc));
        Quest quest = Quest.Create(Title, $"Instance of chain {Title.Value}", deadline);

        foreach (TaskTitle taskTitle in _taskTemplate)
        {
            TodoTask task = TodoTask.Create(userId, taskTitle, dueDate: taskDue);
            quest.AddTask(task);
        }

        _history.Add(new QuestChainInstance(quest.Id, scheduledOn, Completed: false, TimeToComplete: null));
        return quest;
    }

    /// <summary>
    /// Records the outcome of a previously generated instance.
    /// </summary>
    public void RecordInstanceOutcome(QuestId questId, bool completed, TimeSpan? timeToComplete)
    {
        ArgumentNullException.ThrowIfNull(questId);

        int index = _history.FindIndex(h => h.QuestId == questId);
        if (index < 0)
        {
            throw new DomainException("Instance is not part of this chain's history.");
        }

        QuestChainInstance existing = _history[index];
        _history[index] = existing with { Completed = completed, TimeToComplete = timeToComplete };
    }

    public void AddXpEarned(ExperiencePoints xp)
    {
        ArgumentNullException.ThrowIfNull(xp);
        TotalXpEarned = TotalXpEarned.Add(xp);
    }

    /// <summary>
    /// Number of most-recent consecutive completed instances.
    /// </summary>
    public int ConsecutiveCompletionStreak
    {
        get
        {
            int streak = 0;
            for (int i = _history.Count - 1; i >= 0; i--)
            {
                if (!_history[i].Completed)
                {
                    break;
                }

                streak++;
            }

            return streak;
        }
    }

    /// <summary>
    /// Consistency multiplier: +10% per prior consecutive completion, capped at 2.0x.
    /// A streak of 0 yields 1.0; streak of 4 yields 1.4; streak of 10 or more yields 2.0.
    /// </summary>
    public decimal GetConsistencyBonusMultiplier()
    {
        int streak = ConsecutiveCompletionStreak;
        decimal multiplier = 1.0m + (streak * 0.1m);
        return Math.Min(multiplier, 2.0m);
    }

    public QuestChainStats GetStats()
    {
        int total = _history.Count;
        int completed = _history.Count(h => h.Completed);
        decimal rate = total == 0 ? 0m : (decimal)completed / total;
        IEnumerable<TimeSpan> times = _history
            .Where(h => h.Completed && h.TimeToComplete.HasValue)
            .Select(h => h.TimeToComplete!.Value);
        TimeSpan avg = times.Any()
            ? TimeSpan.FromTicks((long)times.Average(t => t.Ticks))
            : TimeSpan.Zero;
        return new QuestChainStats(total, completed, rate, avg, ConsecutiveCompletionStreak, TotalXpEarned);
    }

    /// <summary>
    /// Adds a new task title to the chain's template for future instances.
    /// </summary>
    public void AddTemplateTask(TaskTitle taskTitle)
    {
        ArgumentNullException.ThrowIfNull(taskTitle);
        if (_taskTemplate.Any(t => t.Value == taskTitle.Value))
        {
            throw new DomainException("Task already exists in the chain template.");
        }

        _taskTemplate.Add(taskTitle);
    }
}
