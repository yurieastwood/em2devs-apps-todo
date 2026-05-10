using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/data")]
[Route("api/v{version:apiVersion}/data")]
public sealed class DataController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly TimeProvider _timeProvider;

    public DataController(
        IMediator mediator,
        IOptions<Microsoft.AspNetCore.Mvc.JsonOptions> jsonOptions,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(jsonOptions);
        _mediator = mediator;
        _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
        _timeProvider = timeProvider;
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string? format,
        [FromQuery] string? scope,
        CancellationToken ct)
    {
        if (!string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("format",
                string.IsNullOrEmpty(format) ? "format is required." : $"Unsupported format '{format}'. Allowed: json.");
            return ValidationProblem(ModelState);
        }
        if (!string.Equals(scope, "all", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("scope",
                string.IsNullOrEmpty(scope) ? "scope is required." : $"Unsupported scope '{scope}'. Allowed: all.");
            return ValidationProblem(ModelState);
        }

        Result<DataExportEnvelopeReadModel> result = await _mediator
            .Send(new ExportDataQuery(DataExportFormat.Json, DataExportScope.All), ct)
            .ConfigureAwait(false);

        return result.Match<IActionResult>(
            envelope =>
            {
                byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(envelope, _jsonOptions);
                string fileName = $"waypoint-export-{_timeProvider.GetUtcNow():yyyyMMddTHHmmssZ}.json";
                return File(bytes, "application/json", fileName);
            },
            error =>
            {
                int status = error is ValidationError ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
                return Problem(error.Message, statusCode: status);
            });
    }
}
