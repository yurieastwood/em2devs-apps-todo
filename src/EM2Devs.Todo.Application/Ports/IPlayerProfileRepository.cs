using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Ports;

public interface IPlayerProfileRepository
{
    Task<PlayerProfile> GetProfileAsync(CancellationToken ct = default);
    Task AwardXpAsync(ExperiencePoints xp, XpBreakdownReadModel? breakdown = null, CancellationToken ct = default);
}
