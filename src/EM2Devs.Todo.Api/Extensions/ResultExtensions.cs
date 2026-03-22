using System.Diagnostics;
using EM2Devs.Todo.Domain;
using Microsoft.AspNetCore.Mvc;

namespace EM2Devs.Todo.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToHttpResult<T>(
        this Result<T> result,
        Func<T, IActionResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return result.Match(
            onSuccess,
            error => error.ToProblemResult());
    }

    private static ObjectResult ToProblemResult(this ResultError error)
    {
        ProblemDetails problem = error switch
        {
            ValidationError v => CreateProblem(
                "Validation failed", StatusCodes.Status400BadRequest, v.Message, v.Errors),
            NotFoundError n => CreateProblem(
                "Resource not found", StatusCodes.Status404NotFound, n.Message),
            ConflictError c => CreateProblem(
                "Conflict", StatusCodes.Status409Conflict, c.Message),
            _ => CreateProblem(
                "An error occurred", StatusCodes.Status500InternalServerError, error.Message)
        };

        return new ObjectResult(problem)
        {
            StatusCode = problem.Status,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static ProblemDetails CreateProblem(
        string title,
        int status,
        string detail,
        IDictionary<string, string[]>? errors = null)
    {
        ProblemDetails problem = new()
        {
            Type = "https://tools.ietf.org/html/rfc9457",
            Title = title,
            Status = status,
            Detail = detail
        };

        string? traceId = Activity.Current?.Id;
        if (traceId is not null)
        {
            problem.Extensions["traceId"] = traceId;
        }

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        return problem;
    }
}
