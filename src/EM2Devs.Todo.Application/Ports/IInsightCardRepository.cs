using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Ports;

public interface IInsightCardRepository
{
    Task<IReadOnlyList<InsightCard>> GetForCurrentUserAsync(bool includeRead, CancellationToken ct = default);
    Task<InsightCard?> GetByIdAsync(InsightCardId id, CancellationToken ct = default);
    Task AddAsync(InsightCard card, CancellationToken ct = default);
    Task SaveAsync(InsightCard card, CancellationToken ct = default);
}
