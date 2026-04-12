# Orchestration — .NET Aspire

- Status: Accepted
- Date: 2026-03-05

## Context and Problem Statement

EM2Devs.Todo is a multi-service system: a .NET API backend, a SvelteKit frontend, a PostgreSQL database, and (later) a Redis cache and background worker. During development, starting all services consistently, wiring up connection strings, and having visibility into distributed traces and health is a significant source of friction. In production, the same services need container images and deployment manifests. What tooling should orchestrate services locally and generate deployment artifacts?

## Decision Drivers

- Local developer experience: a new team member should be able to start the entire stack with a single command
- Observability out of the box: distributed traces, structured logs, and health check dashboards without bespoke configuration
- Service discovery: services must resolve each other's addresses without hardcoded ports in every developer's config file
- Deployment artifact generation: local orchestration tooling should produce container-ready artifacts (Docker Compose, Kubernetes manifests, Azure Container Apps bicep) without manual authoring
- Multi-service orchestration: must handle .NET services and non-.NET services (SvelteKit, PostgreSQL container, Redis container)
- Resilience defaults: standard HTTP retry policies and circuit breakers should be available to all services without per-service boilerplate

## Considered Options

- .NET Aspire (AppHost + ServiceDefaults)
- Docker Compose only
- Manual setup (separate terminal tabs, environment files, no shared tooling)

## Decision Outcome

Chosen option: ".NET Aspire", because it solves the local multi-service orchestration problem with minimal configuration while simultaneously producing cloud-ready deployment artifacts. The `AppHost` project becomes the single entry point — running it starts the API, the SvelteKit frontend (as an executable resource), PostgreSQL (as a container resource), and Redis. Aspire injects connection strings and service URLs via environment variables, eliminating per-developer `.env` juggling. `ServiceDefaults` wires OpenTelemetry, health checks, and Polly resilience policies into every .NET service automatically. The Aspire dashboard provides real-time traces, logs, and metrics during development without any additional infrastructure.

Aspire is a development-time and build-time tool — it does not run in production. It produces container images and deployment manifests (Docker Compose, Kubernetes, Azure Container Apps via `azd`) that run on any container platform.

### Positive Consequences

- Single `dotnet run --project AppHost` starts the entire stack; no manual service coordination
- OpenTelemetry traces span the API and worker automatically via `ServiceDefaults`; traces are visible in the Aspire dashboard immediately
- Health check endpoints are registered on all .NET services via `ServiceDefaults` with no per-service code
- Polly retry and circuit-breaker policies are available to all services via `AddServiceDefaults()` with sensible defaults
- `azd` integration generates Azure Container Apps deployment with one command when the cloud target is decided
- Non-.NET services (SvelteKit, PostgreSQL, Redis) are orchestrated as first-class resources alongside .NET services
- Connection strings and service URLs are injected at runtime; no hardcoded ports or connection strings in source control

### Negative Consequences

- Aspire AppHost is an additional project in the solution; new contributors must understand its role
- Aspire's component packages (e.g., `Aspire.Hosting.PostgreSQL`) pin compatible container image versions; teams must update these when upgrading infrastructure versions
- The Aspire dashboard is only available during local development; production observability requires a separate backend (see [ADR-012](20260305-012-observability.md))

### Neutral

- Aspire does not prescribe a production hosting platform; it generates artifacts for multiple targets (Docker Compose, ACA, Kubernetes)
- The AppHost project is a .NET console app and can be version-controlled, code-reviewed, and tested like any other project

## Pros and Cons of the Options

### .NET Aspire (AppHost + ServiceDefaults)

A developer-focused orchestration framework from Microsoft that combines a fluent `AppHost` API with a `ServiceDefaults` extension package and a local dashboard.

- Good, because one F5 (or `dotnet run`) starts all services with correct environment wiring
- Good, because OpenTelemetry, health checks, and Polly resilience are pre-configured for all .NET services via `AddServiceDefaults()`
- Good, because the local dashboard provides distributed traces, structured logs, and environment variable inspection with no additional setup
- Good, because `azd` integration generates Azure Container Apps deployment manifests directly from the AppHost graph
- Good, because non-.NET workloads (SvelteKit, databases, caches) integrate as container or executable resources
- Good, because connection strings are injected via environment variables; no secrets committed to source control
- Bad, because introduces an additional project (`AppHost`) that developers must understand
- Bad, because Aspire is a Microsoft-first tool; deepest integration is with Azure, though Docker Compose output works for any platform

### Docker Compose Only

A YAML-based multi-container definition used to start and link services locally.

- Good, because Docker Compose is universal — works with any language, framework, or cloud provider
- Good, because no additional tooling or SDK dependency beyond Docker Desktop
- Bad, because no built-in observability; developers must manually add OpenTelemetry collector, Jaeger, or similar
- Bad, because service discovery relies on Docker network aliases; local port mapping and `appsettings` overrides must be maintained manually
- Bad, because no deployment artifact generation beyond the Compose file itself; Kubernetes or ACA manifests must be authored separately
- Bad, because resilience policies (retries, circuit breakers) require explicit per-service configuration with no shared defaults

### Manual Setup

Each service started independently via separate terminal processes, with connection strings configured per-developer via `.env` files or `appsettings.Development.json`.

- Good, because no tooling dependency; works in any environment
- Bad, because onboarding is slow — new developers must discover and configure each service independently
- Bad, because port conflicts, incorrect connection strings, and stale environment variables are common failure modes
- Bad, because no shared observability; debugging distributed flows requires correlating disparate log streams manually
- Bad, because generating deployment artifacts is entirely manual

## More Information

- [.NET Aspire documentation](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview)
- [.NET Aspire ServiceDefaults](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults)
- [Aspire + Azure Developer CLI (azd)](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/azure-developer-cli-vs-aspire)
- Related: [ADR-001](20260305-001-backend-runtime.md) — .NET 9 backend runtime
- Related: [ADR-006](20260305-006-hosting.md) — Container-based hosting strategy
- Related: [ADR-008](20260305-008-caching.md) — Redis cache, provisioned via Aspire
- Related: [ADR-012](20260305-012-observability.md) — Production observability strategy
