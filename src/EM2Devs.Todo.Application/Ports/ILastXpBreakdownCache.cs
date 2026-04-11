using EM2Devs.Todo.Application.ReadModels;

namespace EM2Devs.Todo.Application.Ports;

/// <summary>
/// Singleton in-memory cache for the most recent XP breakdown awarded.
/// Decoupled from the repository so the Scoped Postgres repo can share state across requests.
/// UI-only state; not durable. Persisting breakdown history is a Plan 3 concern.
/// </summary>
public interface ILastXpBreakdownCache
{
    XpBreakdownReadModel? GetCurrent();
    void SetCurrent(XpBreakdownReadModel? breakdown);
}
