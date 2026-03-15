using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a time duration in minutes for task estimation (ADR-0002).
/// Must be greater than zero.
/// </summary>
public sealed record TimeEstimate
{
    public int Minutes { get; }

    private TimeEstimate(int minutes)
    {
        Minutes = minutes;
    }

    public static TimeEstimate FromMinutes(int minutes)
    {
        if (minutes < 0)
        {
            throw new DomainException("Time estimate cannot be negative.");
        }

        if (minutes == 0)
        {
            throw new DomainException("Time estimate must be greater than zero.");
        }

        return new TimeEstimate(minutes);
    }
}
