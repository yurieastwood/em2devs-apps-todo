using EM2Devs.Todo.Application.ReadModels;

namespace EM2Devs.Todo.Application.Ports;

public interface IPlayerProfileRepository
{
    Task<PlayerProfile> GetProfileAsync(CancellationToken ct = default);
}
