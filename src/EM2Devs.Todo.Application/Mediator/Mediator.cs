using Microsoft.Extensions.DependencyInjection;

namespace EM2Devs.Todo.Application.Mediator;

/// <summary>
/// Lightweight mediator that resolves handlers from the DI container.
/// ~30 lines of infrastructure per ADR-010.
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

        Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        dynamic handler = _provider.GetRequiredService(handlerType);

        return handler.Handle((dynamic)request, ct);
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
