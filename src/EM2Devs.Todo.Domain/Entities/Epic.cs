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

    public void AddQuest(Quest quest)
    {
        ArgumentNullException.ThrowIfNull(quest);

        if (_quests.Any(q => q.Id == quest.Id))
        {
            throw new DomainException("Quest is already assigned to this epic.");
        }

        _quests.Add(quest);
    }
}
