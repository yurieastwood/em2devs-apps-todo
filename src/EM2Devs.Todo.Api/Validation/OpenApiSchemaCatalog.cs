using NJsonSchema;
using NSwag;

namespace EM2Devs.Todo.Api.Validation;

/// <summary>
/// Loads <c>docs/contracts/openapi.yaml</c> once at startup and exposes the
/// resolved <c>application/json</c> request-body <see cref="JsonSchema"/> for each
/// documented operation. Keyed by (uppercase HTTP method, OpenAPI path template).
///
/// Implements the runtime half of ADR-025 (OpenAPI contract as source of truth).
/// See ADR-030 for the design rationale.
/// </summary>
public sealed class OpenApiSchemaCatalog
{
    private readonly Dictionary<(string Method, string Path), JsonSchema> _bodies;

    private OpenApiSchemaCatalog(Dictionary<(string, string), JsonSchema> bodies) =>
        _bodies = bodies;

    /// <summary>
    /// Try to resolve the JSON request-body schema for a given operation.
    /// Returns <c>null</c> when the operation is not documented or has no JSON body.
    /// </summary>
    public JsonSchema? GetRequestBodySchema(string httpMethod, string openApiPath)
    {
        ArgumentNullException.ThrowIfNull(httpMethod);
        ArgumentNullException.ThrowIfNull(openApiPath);
        return _bodies.TryGetValue((httpMethod.ToUpperInvariant(), openApiPath), out JsonSchema? schema)
            ? schema
            : null;
    }

    /// <summary>
    /// Load the catalog from the OpenAPI YAML at <paramref name="yamlPath"/>.
    /// </summary>
    public static async Task<OpenApiSchemaCatalog> LoadAsync(string yamlPath, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yamlPath);

        OpenApiDocument document = await OpenApiYamlDocument.FromFileAsync(yamlPath, ct).ConfigureAwait(false);

        Dictionary<(string, string), JsonSchema> bodies = new();

        foreach (KeyValuePair<string, OpenApiPathItem> pathEntry in document.Paths)
        {
            string openApiPath = pathEntry.Key;
            foreach (KeyValuePair<string, OpenApiOperation> opEntry in pathEntry.Value)
            {
                string method = opEntry.Key.ToUpperInvariant();
                OpenApiOperation operation = opEntry.Value;

                if (operation.RequestBody is null)
                {
                    continue;
                }

                if (!operation.RequestBody.Content.TryGetValue("application/json", out OpenApiMediaType? media))
                {
                    continue;
                }

                if (media?.Schema is not { } schema)
                {
                    continue;
                }

                bodies[(method, openApiPath)] = schema;
            }
        }

        return new OpenApiSchemaCatalog(bodies);
    }
}
