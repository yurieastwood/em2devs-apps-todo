namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Types of challenges available in the system.
/// Global challenges are system-generated; Guild challenges are created by guild members.
/// </summary>
public enum ChallengeType
{
    Global,
    Guild
}
