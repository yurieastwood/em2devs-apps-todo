using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetCurrentSeasonQuery : IRequest<Result<CurrentSeasonReadModel>>;

public sealed class GetCurrentSeasonQueryHandler
    : IRequestHandler<GetCurrentSeasonQuery, Result<CurrentSeasonReadModel>>
{
    private readonly TimeProvider _timeProvider;

    public GetCurrentSeasonQueryHandler(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Task<Result<CurrentSeasonReadModel>> Handle(GetCurrentSeasonQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        int quarter = ((today.Month - 1) / 3) + 1;
        int quarterStartMonth = ((quarter - 1) * 3) + 1;
        DateOnly startDate = new(today.Year, quarterStartMonth, 1);
        DateOnly endDate = startDate.AddMonths(3).AddDays(-1);

        string[] seasonNames = ["Season of the Architect", "Season of the Explorer", "Season of the Scholar", "Season of the Guardian"];
        string[] themes = ["Building & Creating", "Discovery & Adventure", "Learning & Growth", "Health & Resilience"];

        string name = seasonNames[(quarter - 1) % 4];
        string theme = themes[(quarter - 1) % 4];
        int daysRemaining = endDate.DayNumber - today.DayNumber;

        Season season = new(name, theme, startDate, endDate, []);

        var questLine = new SeasonalQuestLineReadModel(
            SeasonalQuestLine.MaxStages, 1, 0, 0, false);

        Result<CurrentSeasonReadModel> result = new CurrentSeasonReadModel(
            name, theme, startDate, endDate, Math.Max(0, daysRemaining),
            season.IsActive(today), questLine, []);

        return Task.FromResult(result);
    }
}
