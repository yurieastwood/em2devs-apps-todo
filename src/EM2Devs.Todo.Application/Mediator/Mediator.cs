using Microsoft.Extensions.DependencyInjection;

namespace EM2Devs.Todo.Application.Mediator;

/// <summary>
/// Lightweight mediator that resolves handlers from the DI container.
/// Supports pipeline behaviors for cross-cutting concerns (ADR-010, ADR-018).
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _provider;

    public Mediator(IServiceProvider provider)
    {
        _provider = provider;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Type requestType = request.GetType();
        Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        dynamic handler = _provider.GetRequiredService(handlerType);

        Type behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        IEnumerable<dynamic> behaviors = _provider.GetServices(behaviorType).Cast<dynamic>();

        Func<Task<TResponse>> handlerDelegate = () => handler.Handle((dynamic)request, ct);

        // Build pipeline from innermost (handler) outward
        foreach (dynamic behavior in behaviors.Reverse())
        {
            Func<Task<TResponse>> next = handlerDelegate;
            handlerDelegate = () => behavior.Handle((dynamic)request, next, ct);
        }

        return handlerDelegate();
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        IEnumerable<INotificationHandler<TNotification>> handlers =
            _provider.GetServices<INotificationHandler<TNotification>>();

        foreach (INotificationHandler<TNotification> handler in handlers)
        {
            await handler.Handle(notification, ct).ConfigureAwait(false);
        }
    }
}
