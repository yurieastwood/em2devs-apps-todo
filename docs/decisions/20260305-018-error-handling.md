# Error Handling — Result Pattern + Problem Details (RFC 9457)

- Status: Accepted
- Date: 2026-03-05

## Context and Problem Statement

EM2Devs.Todo has multiple layers where errors can originate: domain validation in CQRS command handlers, infrastructure failures in repositories, HTTP-level failures in the API layer, and user-facing errors in the SvelteKit frontend. Without a consistent, cross-layer error handling strategy, each layer invents its own conventions — some throw exceptions, some return nulls, some return custom error objects — making the system hard to test, hard to debug, and inconsistent for frontend consumers. What error handling strategy should span the CQRS pipeline, the API layer, and the frontend?

## Decision Drivers

- Consistency: the same error shape must flow from domain handler through API response to the frontend, without translation surprises
- Testability: handlers must be unit-testable for both success and failure paths without catching exceptions
- HTTP standards compliance: API error responses must follow an established standard so generic HTTP tooling (Scalar, Postman, monitoring agents) can interpret them
- Frontend developer experience: the SvelteKit client must receive typed, predictable error responses and have clear extension points for displaying them
- No exception-driven control flow: exceptions must be reserved for genuinely unexpected failures, not expected business rule violations

## Considered Options

- Result pattern in handlers + Problem Details (RFC 9457) at the API layer
- Exception-based with a global exception handler
- Custom error DTOs

## Decision Outcome

Chosen option: "Result pattern in handlers + Problem Details (RFC 9457) at the API layer", because it eliminates exception-driven control flow in business logic, produces HTTP error responses that conform to an established RFC standard, and gives the frontend a stable, typed contract for errors at every layer. The strategy is layered, with each layer having a clear responsibility:

**1. CQRS handlers — Result pattern (`Result<T>`)**

Command and query handlers return `Result<T>` (a discriminated union of success and typed error) rather than throwing exceptions for expected failures. A domain handler knows about business rule violations (e.g., completing an already-completed todo, a user not found); these are modelled as explicit error types, not exceptions. This makes handlers trivially unit-testable — assert the returned result type, not an exception.

A lightweight `Result<T>` type is implemented in the domain project (or a shared kernel). Common error cases are modelled as sealed record types (e.g., `NotFoundError`, `ValidationError`, `ConflictError`) so callers can switch on the error discriminant exhaustively.

**2. Validation — FluentValidation in the mediator pipeline**

A `ValidationBehavior<TRequest, TResponse>` pipeline behaviour runs FluentValidation before any handler is invoked. If validation fails, the behaviour short-circuits and returns a `Result` wrapping a `ValidationError` containing the field-level failures. This keeps validation logic out of handlers and ensures it runs unconditionally for every command.

**3. API layer — Problem Details (RFC 9457)**

Minimal API endpoint handlers receive `Result<T>` from MediatR, map the result to an appropriate HTTP status code, and use .NET 9's `TypedResults.Problem()` to construct RFC 9457-compliant `ProblemDetails` responses for failures. The mapping is done once per endpoint or via a shared extension method — `result.Match(TypedResults.Ok, err => err.ToProblemDetails())`.

The `ProblemDetails` response shape is:

```json
{
  "type": "https://tools.ietf.org/html/rfc9457",
  "title": "Todo not found",
  "status": 404,
  "detail": "No todo with id 'abc-123' exists.",
  "instance": "/api/v1/todos/abc-123",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

Extensions (extra fields beyond the RFC spec) may be added for validation errors, e.g., `errors` containing field-level messages.

**4. Safety net — Global `IExceptionHandler`**

An `IExceptionHandler` implementation registered in the middleware pipeline catches any exception that escapes the normal flow (infrastructure faults, unhandled edge cases). It logs the exception with full context (structured logging via OpenTelemetry), and returns a 500 Problem Details response. Stack traces are never included in the response body in any environment.

**5. Frontend (SvelteKit) — typed error handling**

The SvelteKit API client types error responses against the `ProblemDetails` schema. Unhandled load errors are caught by `+error.svelte` pages. The `handleError` hook in `hooks.server.ts` logs client-side errors to the observability pipeline. Toast notifications surface user-actionable errors; system errors show a generic message. No raw error messages from the server are exposed directly in the UI.

### Positive Consequences

- Handlers are purely functional with respect to errors — no hidden exception paths, full testability
- `Result<T>` makes all failure modes explicit in the type signature; callers cannot ignore errors
- Problem Details is an established RFC (9457, superseding RFC 7807); any HTTP-aware tooling can interpret the error shape
- .NET 9's `TypedResults.Problem()` builds compliant Problem Details responses with zero boilerplate
- `traceId` in every Problem Details response links an API error directly to the distributed trace in the observability backend
- FluentValidation in the pipeline ensures validation is never skipped, regardless of which endpoint triggers a command
- The SvelteKit frontend has a stable, typed error contract — no guessing at error shapes

### Negative Consequences

- The `Result<T>` pattern adds a mapping step in every endpoint handler (unwrap result, map error to Problem Details); this is mechanical but not free
- Developers unfamiliar with the Result pattern need a brief orientation; the pattern must be documented and enforced via code review
- Two parallel error paths (Result for expected errors, exceptions for unexpected) require discipline to maintain the boundary correctly

### Neutral

- The `Result<T>` type can be implemented with a lightweight custom type or a well-maintained library (e.g., `ErrorOr`, `FluentResults`, `Ardalis.Result`); the choice of library is a separate implementation detail and does not affect the architectural decision
- Problem Details extensions (`errors` field for validation) are permitted by RFC 9457 and do not break conforming clients that ignore unknown fields

## Pros and Cons of the Options

### Result Pattern in Handlers + Problem Details (RFC 9457) at the API Layer

A layered approach: domain handlers express failure through explicit return types; the API layer translates those to standardised HTTP error responses.

- Good, because handler unit tests assert return types, not exception types — simpler and faster test setup
- Good, because all failure modes are visible in method signatures; implicit failures cannot be silently swallowed
- Good, because Problem Details is an RFC standard — frontend, monitoring, and API gateway tooling can interpret errors uniformly
- Good, because .NET 9 provides `TypedResults.Problem()` natively, so no extra library is needed for the HTTP layer
- Good, because the `traceId` field in Problem Details enables direct correlation between user-reported errors and distributed traces
- Bad, because the mapping step (Result -> Problem Details) is mechanical and must be done consistently; a shared extension method is required to prevent inconsistency across endpoints

### Exception-Based with a Global Exception Handler

Handlers throw typed or untyped exceptions on failure; a global middleware catches all exceptions and maps them to HTTP responses.

- Good, because the programming model is familiar — no new abstractions required
- Good, because a single global handler centralises all error-to-HTTP mapping in one place
- Bad, because exceptions for control flow are significantly more expensive than value returns (stack unwinding, allocation of exception objects)
- Bad, because exception paths are invisible in method signatures; callers have no type-level indication of which exceptions may be thrown
- Bad, because unit testing requires `Assert.Throws`, which is more brittle than asserting return values
- Bad, because a global handler must maintain a growing map of exception type to HTTP status code as the domain grows, creating a coupling bottleneck

### Custom Error DTOs

Handlers return custom error objects specific to this project; the API layer serialises them to a project-specific JSON error shape.

- Good, because full control over the error shape
- Good, because no dependency on an external standard
- Bad, because reinventing RFC 9457, which already defines a well-adopted standard for HTTP error responses
- Bad, because generic tooling (Scalar, Postman, APM agents) cannot interpret a custom shape
- Bad, because the frontend team must implement a custom error parser instead of using an existing Problem Details client library
- Bad, because the error shape drifts over time without the discipline imposed by an external standard

## More Information

- [RFC 9457 — Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc9457)
- [TypedResults.Problem — ASP.NET Core (.NET 9)](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.typedresults.problem)
- [IExceptionHandler — ASP.NET Core (.NET 8+)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling#iexceptionhandler)
- [FluentValidation with MediatR pipeline](https://docs.fluentvalidation.net/en/latest/aspnet.html)
- [SvelteKit error handling](https://kit.svelte.dev/docs/errors)
- Related: [ADR-004](20260305-004-api-style.md) — REST Minimal APIs as the API layer
- Related: [ADR-010](20260305-010-cqrs-mediator.md) — CQRS mediator pipeline where ValidationBehavior runs
