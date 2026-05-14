using System.Text;
using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using EM2Devs.Todo.Application.Commands;
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
        if (!IsKnown(format, "json", "csv"))
        {
            ModelState.AddModelError("format",
                string.IsNullOrEmpty(format)
                    ? "format is required."
                    : $"Unsupported format '{format}'. Allowed: json, csv.");
            return ValidationProblem(ModelState);
        }
        if (!IsKnown(scope, "all", "tasksOnly"))
        {
            ModelState.AddModelError("scope",
                string.IsNullOrEmpty(scope)
                    ? "scope is required."
                    : $"Unsupported scope '{scope}'. Allowed: all, tasksOnly.");
            return ValidationProblem(ModelState);
        }

        bool jsonAll = string.Equals(format, "json", StringComparison.OrdinalIgnoreCase)
            && string.Equals(scope, "all", StringComparison.OrdinalIgnoreCase);
        bool csvTasksOnly = string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase)
            && string.Equals(scope, "tasksOnly", StringComparison.OrdinalIgnoreCase);

        if (!jsonAll && !csvTasksOnly)
        {
            ModelState.AddModelError("scope",
                "format=json must pair with scope=all; format=csv must pair with scope=tasksOnly.");
            return ValidationProblem(ModelState);
        }

        if (csvTasksOnly)
        {
            Result<string> csvResult = await _mediator
                .Send(new ExportTasksAsCsvQuery(), ct)
                .ConfigureAwait(false);

            return csvResult.Match<IActionResult>(
                csv =>
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(csv);
                    string fileName = $"waypoint-tasks-{_timeProvider.GetUtcNow():yyyyMMddTHHmmssZ}.csv";
                    return File(bytes, "text/csv", fileName);
                },
                ToErrorResponse);
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
            ToErrorResponse);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteAll(
        [FromBody] DeleteAllDataRequest? request,
        CancellationToken ct)
    {
        if (request is null)
        {
            ModelState.AddModelError("body", "Request body is required.");
            return ValidationProblem(ModelState);
        }

        Result<bool> result = await _mediator
            .Send(new DeleteAllUserDataCommand(request.Confirmation), ct)
            .ConfigureAwait(false);

        return result.Match<IActionResult>(
            _ => NoContent(),
            ToErrorResponse);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(
        [FromBody] DataExportEnvelopeReadModel? envelope,
        CancellationToken ct)
    {
        if (envelope is null)
        {
            ModelState.AddModelError("body", "Import envelope is required.");
            return ValidationProblem(ModelState);
        }

        // ADR-030: schema-level constraints (null items, list-item shape, level bounds,
        // meta.recordCount bound, scope/format enums, ...) are enforced by the global
        // OpenApiRequestBodyValidationFilter before this action runs. Anything that
        // reaches this point is already schema-compliant; controller-only invariants
        // (e.g., domain rules not expressible in JSON Schema) belong below.

        Result<ImportResult> result = await _mediator
            .Send(new ImportDataCommand(envelope), ct)
            .ConfigureAwait(false);

        return result.Match<IActionResult>(
            r => Ok(new ImportResponse(r.RecordsImported)),
            ToErrorResponse);
    }

    private IActionResult ToErrorResponse(ResultError error)
    {
        int status = error is ValidationError
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;
        return Problem(error.Message, statusCode: status);
    }

    private static bool IsKnown(string? value, params string[] allowed)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }
        foreach (string a in allowed)
        {
            if (string.Equals(value, a, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}

public sealed record DeleteAllDataRequest(string? Confirmation);
public sealed record ImportResponse(int RecordsImported);
