using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class Epic
{
    private readonly List<Quest> _quests = [];

    public EpicId Id { get; }
    public EpicTitle Title { get; private set; }
    public string Description { get; private set; }
    public DateOnly? TargetDate { get; private set; }
    public bool IsCompleted { get; private set; }
    public SagaId? SagaId { get; private set; }
    public IReadOnlyList<Quest> Quests => _quests.AsReadOnly();

    public decimal Progress
    {
        get
        {
            if (_quests.Count == 0)
            {
                return 0m;
            }

            decimal totalProgress = _quests.Sum(q => (decimal)q.Progress);
            return totalProgress / _quests.Count;
        }
    }

    private Epic(EpicId id, EpicTitle title, string description, DateOnly? targetDate)
    {
        Id = id;
        Title = title;
        Description = description;
        TargetDate = targetDate;
    }

    public static Epic Create(EpicTitle title, string description, DateOnly? targetDate = null)
    {
        return new Epic(EpicId.New(), title, description, targetDate);
    }

    /// <summary>
    /// Rebuilds an epic from a persisted snapshot. Quests are owned by the Quest aggregate
    /// and are not re-attached here.
    /// </summary>
    public static Epic Reconstitute(
        EpicId id,
        EpicTitle title,
        string description,
        DateOnly? targetDate,
        bool isCompleted,
        SagaId? sagaId)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(description);

        return new Epic(id, title, description, targetDate)
        {
            IsCompleted = isCompleted,
            SagaId = sagaId,
        };
    }

    public void AddQuest(Quest quest)
    {
        ArgumentNullException.ThrowIfNull(quest);

        if (_quests.Any(q => q.Id == quest.Id))
        {
            throw new DomainException("Quest is already assigned to this epic.");
        }

        _quests.Add(quest);
    }

    public void RemoveQuest(QuestId questId)
    {
        ArgumentNullException.ThrowIfNull(questId);

        Quest? quest = _quests.FirstOrDefault(q => q.Id == questId);
        if (quest is null)
        {
            throw new DomainException($"Quest with id '{questId.Value}' is not assigned to this epic.");
        }

        _quests.Remove(quest);
    }

    public void AssignToSaga(SagaId sagaId)
    {
        ArgumentNullException.ThrowIfNull(sagaId);

        if (SagaId is not null)
        {
            throw new DomainException("Epic already belongs to a saga. Remove it from the current saga first, or move it.");
        }

        SagaId = sagaId;
    }

    public void UnassignFromSaga()
    {
        if (SagaId is null)
        {
            throw new DomainException("Epic is not assigned to any saga.");
        }

        SagaId = null;
    }

    public void Complete()
    {
        if (IsCompleted)
        {
            throw new DomainException("Epic is already completed.");
        }

        if (_quests.Count == 0)
        {
            throw new DomainException("Cannot complete an epic with no quests.");
        }

        if (Progress < 100m)
        {
            throw new DomainException("Cannot complete an epic when not all quests are done.");
        }

        IsCompleted = true;
    }
}
