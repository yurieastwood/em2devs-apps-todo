namespace EM2Devs.Todo.Application.Mediator;

public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> continuation, CancellationToken ct);
}
