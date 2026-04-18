using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EM2Devs.Todo.Api.Middleware;

public sealed class RejectUnknownQueryParametersFilter : IActionFilter
{
    private static readonly HashSet<string> _globallyIgnored = new(StringComparer.OrdinalIgnoreCase)
    {
        "api-version"
    };

    public void OnActionExecuting(ActionExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        IQueryCollection query = context.HttpContext.Request.Query;
        if (query.Count == 0)
        {
            return;
        }

        HashSet<string> allowed = new(StringComparer.OrdinalIgnoreCase);
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            if (parameter.BindingInfo?.BindingSource == Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Query
                || parameter.BindingInfo?.BindingSource is null)
            {
                allowed.Add(parameter.Name);
            }
        }

        foreach (string key in query.Keys)
        {
            if (!allowed.Contains(key) && !_globallyIgnored.Contains(key))
            {
                context.Result = new BadRequestObjectResult(new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Title = "Unknown query parameter",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = $"Query parameter '{key}' is not recognized by this endpoint."
                });
                return;
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
