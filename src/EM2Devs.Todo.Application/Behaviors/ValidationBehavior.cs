using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Domain;
using FluentValidation;
using FluentValidation.Results;

namespace EM2Devs.Todo.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> continuation, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(continuation);

        if (!_validators.Any())
        {
            return await continuation().ConfigureAwait(false);
        }

        ValidationContext<TRequest> context = new(request);

        ValidationResult[] results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct))).ConfigureAwait(false);

        Dictionary<string, string[]> errors = results
            .SelectMany(r => r.Errors)
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

        if (errors.Count > 0)
        {
            ValidationError validationError = new("Validation failed.", errors);

            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                // Use reflection to call Result<T>.Failure(error)
                return (TResponse)typeof(TResponse)
                    .GetMethod(nameof(Result<object>.Failure))!
                    .Invoke(null, [validationError])!;
            }

            throw new ValidationException(results.SelectMany(r => r.Errors));
        }

        return await continuation().ConfigureAwait(false);
    }
}
