namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of evaluating whether a user should switch to lighter tasks
/// due to a mid-day energy dip.
/// </summary>
public sealed record EnergyShiftRecommendation
{
    public bool ShouldSuggestLighterTasks { get; }
    public string? Message { get; }

    private EnergyShiftRecommendation(bool shouldSuggestLighterTasks, string? message)
    {
        ShouldSuggestLighterTasks = shouldSuggestLighterTasks;
        Message = message;
    }

    public static EnergyShiftRecommendation NoShift()
    {
        return new EnergyShiftRecommendation(false, null);
    }

    public static EnergyShiftRecommendation SuggestLighterTasks(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return new EnergyShiftRecommendation(true, message);
    }
}
