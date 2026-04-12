namespace EM2Devs.Todo.Application.Mediator;

/// <summary>
/// Dispatches requests to their handlers and publishes notifications.
/// Resolved from DI; callers never reference handlers directly (ADR-010).
/// </summary>
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
    Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
        where TNotification : INotification;
}
