namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A procrastination signal detected for a task, with a type and weight contributing to urgency.
/// </summary>
public sealed record ProcrastinationSignal
{
    public ProcrastinationSignalType Type { get; }
    public int Weight { get; }

    public ProcrastinationSignal(ProcrastinationSignalType type, int weight)
    {
        if (weight <= 0)
        {
            throw new Exceptions.DomainException("Procrastination signal weight must be positive.");
        }

        Type = type;
        Weight = weight;
    }
}
