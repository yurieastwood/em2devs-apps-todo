# Performance Testing Strategy — k6

- Status: Accepted (gate naming partially superseded by [ADR-028](20260406-028-pipeline-restructuring.md))
- Date: 2026-03-19

## Context and Problem Statement

EM2Devs.Todo needs a strategy for validating performance characteristics — response times, throughput, and error rates — under load. Without performance testing, degradation patterns such as slow endpoints, resource leaks, and connection pool exhaustion will only surface in production. As the application grows (quest hierarchy, gamification, real-time notifications), the surface area for performance regressions increases. What tooling and approach should we use to detect these before release?

## Decision Drivers

- Developer-friendly tooling that fits the existing test-first and CI-driven automation approach
- Can run locally during development and in CI pipelines as a pre-release gate
- Supports load, stress, and endurance/soak testing patterns
- Good reporting for identifying bottlenecks (P50, P95, P99 latency, throughput, error rate)
- Active community and ecosystem for extensions and integrations

## Considered Options

- k6 (JavaScript-based, Grafana Labs, widely adopted, CI-friendly)
- NBomber (.NET-native, C# test scripts, integrates with .NET test runners)
- Artillery (YAML-based, easy CI integration, Node.js ecosystem)

## Decision Outcome

Chosen option: "k6", because it is the most widely adopted performance testing tool with excellent reporting, readable JavaScript test scripts, and seamless CI pipeline integration. It supports all three testing patterns the project requires (load, stress, endurance) and produces structured output that can feed into the existing Grafana observability stack (see ADR-012). Test scripts are version-controlled alongside the codebase and reviewed like any other code.

### Testing Patterns

**Load testing** — Validate normal operating conditions. Ramp concurrent users from 1 to N over a defined period, measuring P50/P95/P99 latency and requests per second. The primary SLO target: P99 < 200ms for task CRUD operations at 100 concurrent users.

**Stress testing** — Find the breaking point. Push past expected capacity to verify the system degrades gracefully — returning 503 Service Unavailable rather than crashing, corrupting data, or hanging indefinitely. Validates that rate limiting and circuit breakers (configured via Aspire ServiceDefaults) behave correctly under extreme load.

**Endurance/soak testing** — Detect slow-burn failures. Sustain moderate load for hours to surface memory leaks, connection pool exhaustion, EF Core context disposal issues, and Quartz.NET job accumulation. Run as a nightly scheduled job in CI, not on every push.

### When to Introduce

Phase 2, after Auth0 integration and the SvelteKit frontend generate realistic traffic patterns with authenticated users, session handling, and multi-step flows. Running performance tests against an unauthenticated API with no frontend would produce misleading baselines.

Performance tests run as a **pre-release gate**, not a pre-push gate — they are too slow (minutes, not seconds) for every developer push. They execute on the staging environment after all other gates (G1–G7) pass.

### SLO Targets

| Endpoint | Concurrent Users | P99 Latency | Error Rate |
|---|---|---|---|
| POST /api/tasks | 100 | < 200ms | < 0.1% |
| GET /api/tasks | 100 | < 150ms | < 0.1% |
| PATCH /api/tasks/{id}/status | 100 | < 200ms | < 0.1% |
| GET /api/profile | 100 | < 100ms | < 0.1% |

### Positive Consequences

- Performance regressions are caught before production via automated CI gate
- SLO targets are codified and version-controlled, not tribal knowledge
- k6 output integrates with the existing Grafana stack for trend analysis across releases
- JavaScript test scripts are readable by the entire team without learning a new language
- Load testing locally helps developers identify N+1 queries and missing indexes during development

### Negative Consequences

- k6 requires a separate runtime (Go binary) — not a .NET tool; must be installed on CI runners
- JavaScript test scripts are separate from the .NET test suite; not discoverable via `dotnet test`
- Meaningful load testing requires a staging environment with production-like data; local testing has limited value for absolute numbers

## Pros and Cons of the Options

### k6

A modern load testing tool by Grafana Labs, using JavaScript ES6 test scripts and a Go-based runtime.

- Good, because widely adopted with strong community, documentation, and ecosystem
- Good, because JavaScript test scripts are readable and version-controllable
- Good, because CLI-first design integrates naturally into CI pipelines (GitHub Actions, Azure DevOps)
- Good, because structured JSON/CSV output feeds directly into Grafana dashboards for trend analysis
- Good, because supports all three patterns: load, stress, and endurance/soak
- Good, because threshold-based pass/fail enables automated gating in CI
- Bad, because requires installing the k6 binary on CI runners (not a NuGet package)
- Bad, because test scripts are JavaScript, not C# — context switching for .NET developers

### NBomber

A .NET-native load testing framework using C# test scripts, integrated with .NET test runners.

- Good, because C# test scripts — no language switching for .NET developers
- Good, because runs via `dotnet test` — integrates with existing test infrastructure
- Good, because strong .NET ecosystem integration (DI, configuration, logging)
- Bad, because smaller community and ecosystem compared to k6
- Bad, because reporting and visualization require additional setup; no native Grafana integration
- Bad, because less widely adopted — fewer examples, tutorials, and community support

### Artillery

A YAML-based load testing tool from the Node.js ecosystem.

- Good, because YAML test definitions are concise and easy to author for simple scenarios
- Good, because built-in CI integration and cloud execution mode
- Bad, because YAML-based definitions become unwieldy for complex test scenarios with custom logic
- Bad, because Node.js dependency adds another runtime to the CI environment
- Bad, because less control over fine-grained timing and protocol-level behaviour compared to k6

## More Information

- [k6 documentation](https://grafana.com/docs/k6/latest/)
- [k6 GitHub Actions integration](https://grafana.com/docs/k6/latest/set-up/set-up-for-ci/)
- [k6 thresholds for CI gating](https://grafana.com/docs/k6/latest/using-k6/thresholds/)
- Related: [ADR-014](20260305-014-testing-strategy.md) — Testing strategy (unit, integration, E2E)
- Related: [ADR-012](20260305-012-observability.md) — Observability (Grafana stack for metrics/traces)
