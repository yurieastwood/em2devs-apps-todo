namespace EM2Devs.Todo.Application.Mediator;

/// <summary>
/// Handles a request and returns a response.
/// Each handler is a plain class with a single responsibility (ADR-010).
/// </summary>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}
