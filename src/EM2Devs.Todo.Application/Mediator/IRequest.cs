namespace EM2Devs.Todo.Application.Mediator;

/// <summary>
/// Marker interface for a request that returns a response.
/// Mirrors MediatR v11 shape per ADR-010.
/// </summary>
public interface IRequest<TResponse>;
