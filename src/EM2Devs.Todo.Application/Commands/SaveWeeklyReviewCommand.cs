using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;

namespace EM2Devs.Todo.Application.Commands;

/// <summary>
/// Persists the authenticated user's reflection for a given review week.
/// When <see cref="WeekOf"/> is null, the current week's Sunday (UTC) is used.
/// Replaces any prior reflection saved for the same week.
/// </summary>
public sealed record SaveWeeklyReviewCommand(
    string WhatWentWell,
    string WhatDragged,
    string Adjustment,
    DateOnly? WeekOf = null) : IRequest<Result<WeeklyReflectionReadModel>>;

public sealed class SaveWeeklyReviewCommandHandler
    : IRequestHandler<SaveWeeklyReviewCommand, Result<WeeklyReflectionReadModel>>
{
    private readonly IWeeklyReflectionRepository _repository;
    private readonly TimeProvider _timeProvider;

    public SaveWeeklyReviewCommandHandler(
        IWeeklyReflectionRepository repository,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<WeeklyReflectionReadModel>> Handle(SaveWeeklyReviewCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateOnly today = DateOnly.FromDateTime(now.UtcDateTime);
        DateOnly weekOf = request.WeekOf ?? GetWeeklyReviewQueryHandler.GetWeekOfSunday(today);

        string wentWell = request.WhatWentWell.Trim();
        string dragged = request.WhatDragged.Trim();
        string adjustment = request.Adjustment.Trim();

        await _repository
            .SaveAsync(weekOf, wentWell, dragged, adjustment, now, ct)
            .ConfigureAwait(false);

        WeeklyReflectionReadModel reflection = new(wentWell, dragged, adjustment, now);
        return reflection;
    }
}
