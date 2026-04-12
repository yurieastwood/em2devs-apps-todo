using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class LastXpBreakdownCache : ILastXpBreakdownCache
{
    private readonly object _lock = new();
    private XpBreakdownReadModel? _breakdown;

    public XpBreakdownReadModel? GetCurrent()
    {
        lock (_lock) { return _breakdown; }
    }

    public void SetCurrent(XpBreakdownReadModel? breakdown)
    {
        lock (_lock) { _breakdown = breakdown; }
    }
}
