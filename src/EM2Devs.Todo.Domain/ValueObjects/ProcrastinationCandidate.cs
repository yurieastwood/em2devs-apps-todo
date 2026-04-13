using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a task identified as a procrastination candidate, combining signals, urgency, and interventions.
/// </summary>
public sealed record ProcrastinationCandidate
{
    public TaskId TaskId { get; }
    public IReadOnlyList<ProcrastinationSignal> Signals { get; }
    public int UrgencyScore { get; }
    public IReadOnlyList<InterventionOption> AvailableInterventions { get; }

    public ProcrastinationCandidate(
        TaskId taskId,
        IReadOnlyList<ProcrastinationSignal> signals,
        IReadOnlyList<InterventionOption> availableInterventions)
    {
        ArgumentNullException.ThrowIfNull(taskId);
        ArgumentNullException.ThrowIfNull(signals);
        ArgumentNullException.ThrowIfNull(availableInterventions);

        if (signals.Count == 0)
        {
            throw new Exceptions.DomainException("A procrastination candidate must have at least one signal.");
        }

        TaskId = taskId;
        Signals = signals;
        UrgencyScore = signals.Sum(s => s.Weight);
        AvailableInterventions = availableInterventions;
    }
}
