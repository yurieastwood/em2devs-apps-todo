using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// Life-chapter goal that groups epics under a shared vision (premium-only).
/// </summary>
public sealed class Saga
{
    private readonly List<Epic> _epics = [];

    public SagaId Id { get; }
    public SagaTitle Title { get; private set; }
    public string Description { get; private set; }
    public string Vision { get; private set; }
    public DateOnly? TargetDate { get; private set; }
    public IReadOnlyList<Epic> Epics => _epics.AsReadOnly();

    /// <summary>
    /// Aggregate progress averaged across assigned epics (each epic weighted equally).
    /// </summary>
    public decimal Progress
    {
        get
        {
            if (_epics.Count == 0)
            {
                return 0m;
            }

            return _epics.Average(e => e.Progress);
        }
    }

    private Saga(SagaId id, SagaTitle title, string description, string vision, DateOnly? targetDate)
    {
        Id = id;
        Title = title;
        Description = description;
        Vision = vision;
        TargetDate = targetDate;
    }

    public static Saga Create(SagaTitle title, string description, string vision, DateOnly? targetDate = null)
    {
        ArgumentNullException.ThrowIfNull(title);
        return new Saga(SagaId.New(), title, description, vision, targetDate);
    }

    public void AddEpic(Epic epic)
    {
        ArgumentNullException.ThrowIfNull(epic);

        if (_epics.Any(e => e.Id == epic.Id))
        {
            throw new DomainException("Epic is already assigned to this saga.");
        }

        if (epic.SagaId is not null && epic.SagaId != Id)
        {
            throw new DomainException("Epic already belongs to another saga. Remove it from the current saga first, or move it.");
        }

        epic.AssignToSaga(Id);
        _epics.Add(epic);
    }

    public void RemoveEpic(EpicId epicId)
    {
        ArgumentNullException.ThrowIfNull(epicId);

        Epic? epic = _epics.FirstOrDefault(e => e.Id == epicId);
        if (epic is null)
        {
            throw new DomainException($"Epic with id '{epicId.Value}' is not assigned to this saga.");
        }

        epic.UnassignFromSaga();
        _epics.Remove(epic);
    }

    /// <summary>
    /// Builds a timeline snapshot: completed vs in-progress epics and an optional trajectory
    /// projecting the finish date using the observed completion rate so far.
    /// </summary>
    public SagaTimeline BuildTimeline(DateTimeOffset startedAt, DateTimeOffset now)
    {
        if (now < startedAt)
        {
            throw new DomainException("Timeline 'now' cannot be before the saga start.");
        }

        int completed = _epics.Count(e => e.IsCompleted);
        int inProgress = _epics.Count - completed;

        DateTimeOffset? projectedCompletion = null;
        if (completed > 0 && completed < _epics.Count)
        {
            double elapsedDays = (now - startedAt).TotalDays;
            double rate = completed / elapsedDays;
            int remaining = _epics.Count - completed;
            double remainingDays = remaining / rate;
            projectedCompletion = now.AddDays(remainingDays);
        }

        return new SagaTimeline(completed, inProgress, Progress, projectedCompletion);
    }
}

/// <summary>
/// Timeline snapshot for a saga.
/// </summary>
public sealed record SagaTimeline(int CompletedEpics, int InProgressEpics, decimal Progress, DateTimeOffset? ProjectedCompletion);
