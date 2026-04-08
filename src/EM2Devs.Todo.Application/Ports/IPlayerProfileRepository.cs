using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Ports;

public interface IPlayerProfileRepository
{
    Task<PlayerProfileReadModel> GetProfileAsync(CancellationToken ct = default);
    Task AwardXpAsync(ExperiencePoints xp, XpBreakdownReadModel? breakdown = null, CancellationToken ct = default);
    Task RecordCompletionAsync(DateOnly completionDate, CancellationToken ct = default);
    Task ProcessDayEndAsync(DateOnly evaluationDate, CancellationToken ct = default);
}
