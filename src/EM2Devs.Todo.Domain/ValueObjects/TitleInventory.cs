namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Collection of earned titles with active title selection.
/// Titles are permanently earned and never revoked.
/// </summary>
public sealed record TitleInventory
{
    private readonly List<Title> _earnedTitles;

    public IReadOnlyList<Title> EarnedTitles => _earnedTitles.AsReadOnly();
    public TitleType? ActiveTitle { get; }

    public TitleInventory(IEnumerable<Title> earnedTitles, TitleType? activeTitle)
    {
        _earnedTitles = earnedTitles?.ToList()
            ?? throw new ArgumentNullException(nameof(earnedTitles));

        if (activeTitle is not null && !_earnedTitles.Exists(t => t.Type == activeTitle))
        {
            throw new Exceptions.DomainException(
                "Active title must be one of the earned titles.");
        }

        ActiveTitle = activeTitle;
    }

    public static TitleInventory Empty() => new([], null);

    public TitleInventory AwardTitle(Title title)
    {
        ArgumentNullException.ThrowIfNull(title);

        if (_earnedTitles.Exists(t => t.Type == title.Type))
        {
            return this;
        }

        List<Title> updated = [.. _earnedTitles, title];
        return new TitleInventory(updated, ActiveTitle);
    }

    public TitleInventory SelectActiveTitle(TitleType type)
    {
        if (!_earnedTitles.Exists(t => t.Type == type))
        {
            throw new Exceptions.DomainException(
                "Cannot select a title that has not been earned.");
        }

        return new TitleInventory(_earnedTitles, type);
    }

    public bool HasTitle(TitleType type) =>
        _earnedTitles.Exists(t => t.Type == type);
}
