using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetEnergyProfileQuery : IRequest<Result<EnergyProfileReadModel>>;

public sealed class GetEnergyProfileQueryHandler
    : IRequestHandler<GetEnergyProfileQuery, Result<EnergyProfileReadModel>>
{
    private readonly IEnergyCheckInRepository _repository;
    private readonly TimeProvider _timeProvider;

    public GetEnergyProfileQueryHandler(IEnergyCheckInRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<EnergyProfileReadModel>> Handle(GetEnergyProfileQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<EnergyCheckIn> checkIns = await _repository.GetRecentAsync(60, ct).ConfigureAwait(false);

        string? currentLevel = null;
        EnergyCheckIn? today = await _repository.GetTodayAsync(ct).ConfigureAwait(false);
        if (today is not null)
        {
            currentLevel = today.Level.ToString();
        }

        var byDay = new Dictionary<DayOfWeek, EnergyLevel>();
        foreach (EnergyCheckIn checkIn in checkIns)
        {
            DayOfWeek day = checkIn.RecordedAt.DayOfWeek;
            byDay.TryAdd(day, checkIn.Level);
        }

        EnergyProfile profile = EnergyProfile.FromCheckIns(byDay);
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromDataPoints(checkIns.Count);

        string? insufficientDataMessage = profile.HasSufficientData
            ? null
            : EnergyProfile.InsufficientDataMessage;

        return new EnergyProfileReadModel(
            currentLevel,
            profile.HasSufficientData,
            confidence.Score,
            confidence.IsHigh ? "High" : confidence.IsModerate ? "Moderate" : "Low",
            checkIns.Count,
            insufficientDataMessage);
    }
}
