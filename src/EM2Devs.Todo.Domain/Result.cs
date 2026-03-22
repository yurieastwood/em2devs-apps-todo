using System.Diagnostics.CodeAnalysis;

namespace EM2Devs.Todo.Domain;

public abstract record ResultError(string Message);

public sealed record NotFoundError(string Message) : ResultError(Message);

public sealed record ValidationError(
    string Message,
    IDictionary<string, string[]>? Errors = null) : ResultError(Message);

public sealed record ConflictError(string Message) : ResultError(Message);

[SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
    Justification = "Factory methods are the idiomatic API for Result<T>")]
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly ResultError? _error;

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
    }

    private Result(ResultError error)
    {
        _error = error;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(ResultError error) => new(error);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<ResultError, TResult> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        return IsSuccess ? onSuccess(_value!) : onError(_error!);
    }

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(ResultError error) => Failure(error);
}
