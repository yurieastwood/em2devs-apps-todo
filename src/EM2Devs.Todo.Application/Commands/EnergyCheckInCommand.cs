using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Commands;

public sealed record EnergyCheckInCommand(string Level) : IRequest<Result<EnergyCheckInResult>>;

public sealed record EnergyCheckInResult(string Level, bool IsUpdate, bool HasFluctuated);

public sealed class EnergyCheckInCommandHandler
    : IRequestHandler<EnergyCheckInCommand, Result<EnergyCheckInResult>>
{
    private readonly IEnergyCheckInRepository _repository;
    private readonly TimeProvider _timeProvider;

    public EnergyCheckInCommandHandler(IEnergyCheckInRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<EnergyCheckInResult>> Handle(EnergyCheckInCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Enum.TryParse<EnergyLevel>(request.Level, ignoreCase: true, out EnergyLevel level))
        {
            return new ValidationError($"Invalid energy level '{request.Level}'. Valid values: Low, Medium, High, Peak.");
        }

        DateTimeOffset now = _timeProvider.GetUtcNow();
        EnergyCheckIn? existing = await _repository.GetTodayAsync(ct).ConfigureAwait(false);

        if (existing is not null)
        {
            existing.UpdateLevel(level, now);
            await _repository.UpdateAsync(existing, ct).ConfigureAwait(false);
            return new EnergyCheckInResult(level.ToString(), IsUpdate: true, existing.HasFluctuated);
        }

        EnergyCheckIn checkIn = EnergyCheckIn.Create(level, now);
        await _repository.AddAsync(checkIn, ct).ConfigureAwait(false);
        return new EnergyCheckInResult(level.ToString(), IsUpdate: false, HasFluctuated: false);
    }
}
