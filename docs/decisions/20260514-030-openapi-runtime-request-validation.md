# OpenAPI-Driven Runtime Request-Body Validation

- Status: Accepted
- Date: 2026-05-14

## Context and Problem Statement

ADR-025 declares `docs/contracts/openapi.yaml` the source of truth for the API surface and uses Schemathesis (Acceptance Stage) to detect implementation drift. Recent gate runs surfaced a recurring pattern of **schema-violation acceptances** on `POST /api/data/import` — Schemathesis sent payloads the contract explicitly forbids (e.g., `meta.recordCount: -1`, `sagas: [[null, null]]`, `meta.scope: "AAA"`, `level: {"current": 0}`) and the API returned 200.

The root cause is that ASP.NET Core model binding deserializes JSON into the strongly-typed C# record but does **not** enforce any JSON Schema constraints (`minimum`, `maximum`, `maxLength`, `enum`, item-type) declared in the contract on primitive fields. The contract says `recordCount: minimum: 0`; the binding produces `int RecordCount = -1` without complaint. Every constraint is silently lost at the wire boundary.

We patched three concrete instances on `POST /api/data/import` with hand-rolled guards in the controller. Each fix unmasked the next failure. The import body alone declares ~60 such constraints across nested entity snapshots. Continuing to hand-roll guards is brittle, drifts from the spec as it grows, and will never reach completeness across 67 operations.

## Decision Drivers

- The OpenAPI contract is already the source of truth (ADR-025) — runtime validation should consult it directly, not duplicate it in C#.
- Schema drift between contract and implementation is the failure mode Schemathesis is designed to catch; the fix must make Schemathesis green by enforcing what the contract declares, not by loosening the contract.
- ASP.NET model binding's primitive coercion gaps are systemic; one fix per gap is unsustainable.
- 400 responses must remain `application/problem+json` (RFC 9457) per ADR-018.
- The gate (pre-merge-commit) must pass without manual intervention.

## Considered Options

- Hand-rolled per-field guards in each controller action.
- Generate C# validators from the OpenAPI spec at build time (e.g., NSwag client/server generator).
- Runtime middleware that loads the OpenAPI document at startup and validates incoming request bodies against the matched operation's `requestBody` schema before the action executes.

## Decision Outcome

Chosen option: **"Runtime middleware that loads the OpenAPI document at startup and validates incoming request bodies against the matched operation's `requestBody` schema"**, because it consumes the contract directly, scales with the spec without code changes, and produces the same `application/problem+json` shape the rest of the pipeline already returns.

### Mechanism

A scoped `IAsyncActionFilter` (registered globally in `Program.cs`) intercepts every authenticated request that targets a documented operation. For requests carrying a JSON body, the filter:

1. Resolves the matching OpenAPI operation by (method, route template).
2. Reads the raw body, parses it as `JsonElement`, and validates it against the operation's `requestBody.content["application/json"].schema` using **NJsonSchema**.
3. On validation failure: short-circuits with HTTP 400 and an RFC 9457 `application/problem+json` body whose `errors` map lists each violation by JSON-pointer path.
4. On success: rewinds the body stream so the controller's `[FromBody]` binding sees the same bytes.

NJsonSchema is the library of record because it ships first-class support for the OpenAPI 3.0 JSON Schema dialect (including `nullable: true` semantics), parses YAML, and exposes per-pointer validation errors. It is widely used in the .NET ecosystem and does not introduce a transitive native dependency.

### Scope

- **In scope:** request bodies for every documented operation (the surface Schemathesis exercises most aggressively).
- **Out of scope (this ADR):** path and query parameters. ASP.NET's existing route binding already enforces basic type/format (e.g., `:guid`); the remaining gaps (`enum`, `minimum` on integer query params) are a follow-up.
- **Out of scope (this ADR):** response body validation. Production runtime validation of responses adds latency without proportional safety; Schemathesis covers response conformance offline.

### Replaced Code

The hand-rolled guards added in `fix(api): tighten /api/data/import contract validation (3 gaps)` (`HasNullItems`, the sagas item-shape check, the level-bounds check, the `meta.recordCount` check) become redundant once the middleware is in place and are removed.

### Positive Consequences

- One implementation, every operation, every constraint — including future schema additions — covered automatically.
- The C# layer stops drifting from the spec because the spec **is** the validator.
- Schemathesis stops flagging schema-violation acceptances as a class.
- 400 responses retain the established `application/problem+json` shape.

### Negative Consequences

- Adds a NuGet dependency (`NJsonSchema`) and a parsed-OpenAPI document held in process memory (~hundreds of KB).
- A first-request startup tax to parse the YAML once; subsequent requests pay only the JSON-validate cost.
- The validation step duplicates work for endpoints that already have FluentValidation rules on command DTOs — but the duplication is at a different layer (wire vs. domain) and protects against different drift.

### Neutral

- The OpenAPI document path (`docs/contracts/openapi.yaml`) is read at startup from the API project's working directory; in container deployments it must be copied into the published output. Build configuration already copies `docs/contracts/openapi.yaml` for Spectral lint and Schemathesis; the same artifact is reused.

## Pros and Cons of the Options

### Hand-rolled per-field guards

- Good, because requires no new dependency.
- Good, because each guard is local and obvious.
- Bad, because it scales O(constraints × operations) — unsustainable across 67 operations and ~60 import-body constraints.
- Bad, because guards drift from the spec as the spec evolves; nothing prevents removal of a constraint in YAML from being silently un-enforced in C#.
- Bad, because each new failure surfaces only after Schemathesis exercises that exact field.

### Build-time validator generation (NSwag)

- Good, because validators are pure C#, no startup cost.
- Good, because generation is a build step, visible in the artifact.
- Bad, because the generator's coverage of OpenAPI 3.0 constructs (`nullable: true`, `oneOf`, `allOf`, `additionalProperties: false`) is uneven and historically lossy.
- Bad, because regenerated code lives in the repo and inflates diffs on every spec change.
- Bad, because debugging a generated validator is harder than reading a single middleware.

### Runtime middleware against the spec (chosen)

- Good, because the spec is the validator — no translation step.
- Good, because new schema constraints take effect with zero code change.
- Good, because validation errors naturally produce RFC 9457 ProblemDetails on the same pipeline.
- Bad, because adds a NuGet dependency.
- Bad, because adds startup parsing cost and per-request validation overhead (measured below).

## More Information

- ADR-025: OpenAPI Contract as Source of Truth — this ADR implements the missing **runtime** enforcement half.
- ADR-018: Error Handling — the 400 responses returned by this middleware conform to RFC 9457.
- Library: [NJsonSchema](https://github.com/RicoSuter/NJsonSchema).
- Code locations:
  - `src/EM2Devs.Todo.Api/Validation/OpenApiRequestBodyValidationFilter.cs` (filter)
  - `src/EM2Devs.Todo.Api/Validation/OpenApiSchemaCatalog.cs` (parsed-spec cache)
  - `src/EM2Devs.Todo.Api/Program.cs` (registration)
