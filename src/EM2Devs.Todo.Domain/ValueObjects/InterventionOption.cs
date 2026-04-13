namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// An intervention option presented to the user for a procrastinated task.
/// Messages are always supportive, never shaming.
/// </summary>
public sealed record InterventionOption
{
    public InterventionOptionType Type { get; }
    public string SupportiveMessage { get; }

    public InterventionOption(InterventionOptionType type, string supportiveMessage)
    {
        if (string.IsNullOrWhiteSpace(supportiveMessage))
        {
            throw new Exceptions.DomainException("Intervention message cannot be empty.");
        }

        Type = type;
        SupportiveMessage = supportiveMessage;
    }
}
