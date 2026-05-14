using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NJsonSchema;
using NJsonSchema.Validation;

namespace EM2Devs.Todo.Api.Validation;

/// <summary>
/// Global resource filter that validates the raw JSON request body of every
/// documented operation against its OpenAPI <c>application/json</c>
/// <c>requestBody</c> schema, before model binding runs.
///
/// On validation failure, short-circuits with HTTP 400
/// <c>application/problem+json</c> (RFC 9457) carrying a per-pointer
/// <c>errors</c> map. On success, rewinds the body stream so the controller's
/// <c>[FromBody]</c> binding sees the same bytes.
///
/// Implements ADR-030: the C# layer enforces what the OpenAPI contract
/// declares, without duplicating the constraints in hand-rolled guards.
/// </summary>
public sealed partial class OpenApiRequestBodyValidationFilter : IAsyncResourceFilter
{
    private readonly OpenApiSchemaCatalog _catalog;

    public OpenApiRequestBodyValidationFilter(OpenApiSchemaCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    public async Task OnResourceExecutionAsync(
        ResourceExecutingContext context,
        ResourceExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        HttpRequest request = context.HttpContext.Request;

        if (!HasJsonBody(request))
        {
            await next().ConfigureAwait(false);
            return;
        }

        string? openApiPath = ResolveOpenApiPath(context.HttpContext);
        if (openApiPath is null)
        {
            await next().ConfigureAwait(false);
            return;
        }

        JsonSchema? schema = _catalog.GetRequestBodySchema(request.Method, openApiPath);
        if (schema is null)
        {
            await next().ConfigureAwait(false);
            return;
        }

        request.EnableBuffering();
        string body;
        using (StreamReader reader = new(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync().ConfigureAwait(false);
        }
        request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
        {
            // Empty bodies are handled by the existing controller logic
            // (each controller decides whether an empty body is a 400).
            await next().ConfigureAwait(false);
            return;
        }

        ICollection<ValidationError> errors = schema.Validate(body);
        if (errors.Count == 0)
        {
            await next().ConfigureAwait(false);
            return;
        }

        context.Result = BuildProblemResult(errors);
    }

    private static bool HasJsonBody(HttpRequest request)
    {
        if (request.ContentLength is 0)
        {
            return false;
        }
        string? contentType = request.ContentType;
        return !string.IsNullOrEmpty(contentType)
            && contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ResolveOpenApiPath(HttpContext httpContext)
    {
        Endpoint? endpoint = httpContext.GetEndpoint();
        if (endpoint is not RouteEndpoint routeEndpoint)
        {
            return null;
        }

        string raw = routeEndpoint.RoutePattern.RawText ?? string.Empty;
        if (string.IsNullOrEmpty(raw))
        {
            return null;
        }

        // ASP.NET route templates look like "api/tasks/{taskId:guid}"; the OpenAPI
        // contract uses "/api/tasks/{taskId}". Prepend a leading slash and strip
        // route constraints from each segment.
        string normalised = "/" + raw.TrimStart('/');
        return RouteConstraintRegex().Replace(normalised, "{$1}");
    }

    private static BadRequestObjectResult BuildProblemResult(ICollection<ValidationError> errors)
    {
        Dictionary<string, string[]> grouped = errors
            .GroupBy(static e => string.IsNullOrEmpty(e.Path) ? "body" : e.Path.TrimStart('#', '/'))
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => $"{e.Kind}").Distinct().ToArray());

        ValidationProblemDetails problem = new(grouped)
        {
            Type = "https://tools.ietf.org/html/rfc9457",
            Title = "Request does not conform to the OpenAPI contract.",
            Status = StatusCodes.Status400BadRequest,
        };

        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" },
        };
    }

    [GeneratedRegex(@"\{([^{}:]+):[^{}]+\}")]
    private static partial Regex RouteConstraintRegex();
}
