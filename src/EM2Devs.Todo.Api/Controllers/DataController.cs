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

        // Per the OpenAPI schema, list items are non-nullable. Reject envelopes that
        // contain null entries (caught by Schemathesis property-based testing).
        if (HasNullItems(envelope.Tasks)
            || HasNullItems(envelope.Quests)
            || HasNullItems(envelope.Epics)
            || HasNullItems(envelope.WeeklyReviews)
            || HasNullItems(envelope.InsightCards)
            || HasNullItems(envelope.TimelineEvents)
            || HasNullItems(envelope.XpHistory)
            || HasNullItems(envelope.SkillTreeProgress)
            || HasNullItems(envelope.TitlesEarned))
        {
            ModelState.AddModelError("body", "List items in the import envelope must not be null.");
            return ValidationProblem(ModelState);
        }

        // Per the OpenAPI schema, `sagas` items are objects. The C# binding accepts any
        // JSON shape into JsonElement, so verify each item explicitly.
        foreach (System.Text.Json.JsonElement saga in envelope.Sagas)
        {
            if (saga.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                ModelState.AddModelError("sagas", "sagas items must be JSON objects.");
                return ValidationProblem(ModelState);
            }
        }

        // Per the OpenAPI schema, level fields are optional — absence falls back to
        // the starting-state defaults (1, 0, 0). Only reject when an *explicit* value
        // violates its declared schema bounds.
        if (envelope.Level is { Current: < 1 }
            || envelope.Level is { Xp: < 0 }
            || envelope.Level is { LongestStreak: < 0 })
        {
            ModelState.AddModelError("level",
                "level.current must be >= 1; level.xp and level.longestStreak must be >= 0.");
            return ValidationProblem(ModelState);
        }

        // Per the OpenAPI schema, meta.recordCount has minimum: 0.
        if (envelope.Meta is { RecordCount: < 0 })
        {
            ModelState.AddModelError("meta.recordCount", "meta.recordCount must be >= 0.");
            return ValidationProblem(ModelState);
        }

        Result<ImportResult> result = await _mediator
            .Send(new ImportDataCommand(envelope), ct)
            .ConfigureAwait(false);

        return result.Match<IActionResult>(
            r => Ok(new ImportResponse(r.RecordsImported)),
            ToErrorResponse);
    }

    private static bool HasNullItems<T>(IReadOnlyList<T>? items) where T : class
    {
        if (items is null)
        {
            return false;
        }
        foreach (T item in items)
        {
            if (item is null)
            {
                return true;
            }
        }
        return false;
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
