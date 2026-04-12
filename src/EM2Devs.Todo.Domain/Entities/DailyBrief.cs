using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// A personalised daily plan recommending a prioritised task sequence.
/// Users can accept, modify, or dismiss the brief.
/// </summary>
public sealed class DailyBrief
{
    public DailyBriefId Id { get; }
    public DateOnly Date { get; }
    public IReadOnlyList<TaskId> RecommendedTaskIds { get; private set; }
    public DailyBriefStatus Status { get; private set; }

    private DailyBrief(DailyBriefId id, DateOnly date, IReadOnlyList<TaskId> recommendedTaskIds)
    {
        Id = id;
        Date = date;
        RecommendedTaskIds = recommendedTaskIds;
        Status = DailyBriefStatus.Generated;
    }

    public static DailyBrief Create(DateOnly date, IReadOnlyList<TaskId> recommendedTaskIds)
    {
        ArgumentNullException.ThrowIfNull(recommendedTaskIds);

        if (recommendedTaskIds.Count == 0)
        {
            throw new DomainException("Daily brief must contain at least one task.");
        }

        return new DailyBrief(DailyBriefId.New(), date, recommendedTaskIds);
    }

    public void Accept()
    {
        Status = DailyBriefStatus.Accepted;
    }

    public void Dismiss()
    {
        Status = DailyBriefStatus.Dismissed;
    }

    public void Modify(IReadOnlyList<TaskId> newTaskIds)
    {
        ArgumentNullException.ThrowIfNull(newTaskIds);

        if (newTaskIds.Count == 0)
        {
            throw new DomainException("Modified brief must contain at least one task.");
        }

        RecommendedTaskIds = newTaskIds;
        Status = DailyBriefStatus.Modified;
    }
}
