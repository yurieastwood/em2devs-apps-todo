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
            ValidationError v => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9457",
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = v.Message,
                Extensions =
                {
                    ["traceId"] = Activity.Current?.Id,
                    ["errors"] = v.Errors
                }
            },
            NotFoundError n => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9457",
                Title = "Resource not found",
                Status = StatusCodes.Status404NotFound,
                Detail = n.Message,
                Extensions = { ["traceId"] = Activity.Current?.Id }
            },
            ConflictError c => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9457",
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = c.Message,
                Extensions = { ["traceId"] = Activity.Current?.Id }
            },
            _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9457",
                Title = "An error occurred",
                Status = StatusCodes.Status500InternalServerError,
                Detail = error.Message,
                Extensions = { ["traceId"] = Activity.Current?.Id }
            }
        };

        return new ObjectResult(problem) { StatusCode = problem.Status };
    }
}
