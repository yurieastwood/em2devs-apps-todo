namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// An earned title representing sustained behaviour achievement.
/// Titles are permanently earned and never revoked.
/// </summary>
public sealed record Title
{
    public TitleType Type { get; }
    public DateOnly EarnedOn { get; }

    public Title(TitleType type, DateOnly earnedOn)
    {
        Type = type;
        EarnedOn = earnedOn;
    }

    public static string DisplayName(TitleType type) =>
        type switch
        {
            TitleType.EarlyBird => "Early Bird",
            TitleType.MorningArchitect => "Morning Architect",
            TitleType.NightOwl => "Night Owl",
            TitleType.MarathonBuilder => "Marathon Builder",
            TitleType.BossSlayer => "Boss Slayer",
            TitleType.StreakMaster => "Streak Master",
            TitleType.QuestCloser => "Quest Closer",
            TitleType.ConsistentPlanner => "Consistent Planner",
            TitleType.TeamAnchor => "Team Anchor",
            _ => throw new ArgumentOutOfRangeException(
                nameof(type), type, "Unknown title type.")
        };
}
