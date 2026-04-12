# Background Jobs — Quartz.NET

- Status: Accepted
- Date: 2026-03-05

## Context and Problem Statement

The gamification engine requires periodic background work that cannot be driven purely by user-initiated requests. Specific workloads include daily streak evaluation (did the user complete at least one task today?), leaderboard recomputation, achievement threshold checks, and notification scheduling. These jobs must run reliably across service restarts, support cron-style scheduling, and integrate with the existing PostgreSQL database and observability stack. Which background job framework should be used?

## Decision Drivers

- Open-source licensing: no commercial license required for production use
- Cron scheduling: support for cron expressions to define recurring job schedules precisely
- Job persistence: jobs and their state must survive service restarts; in-memory-only schedulers are insufficient
- PostgreSQL compatibility: job persistence should use the same database already in the stack
- DI integration: jobs must be able to resolve services from the .NET DI container
- Observability: job execution should produce OpenTelemetry traces visible in the .NET Aspire dashboard

## Considered Options

- Quartz.NET
- Hangfire
- .NET `IHostedService` + `PeriodicTimer`
- Wolverine scheduling

## Decision Outcome

Chosen option: "Quartz.NET", because it is fully open-source (Apache 2.0), supports cron expressions, persists job state in PostgreSQL via `Quartz.Serialization.Json` and the `Quartz.Impl.AdoJobStore`, integrates with the .NET DI container via `AddQuartz` and `AddQuartzHostedService`, and produces OpenTelemetry traces that appear natively in the .NET Aspire dashboard.

During initial development, simple recurring work can start with `IHostedService` + `PeriodicTimer` to keep the setup minimal. Quartz.NET is introduced when job persistence and cron flexibility become necessary — the transition is additive rather than disruptive.

### Positive Consequences

- Cron expressions give precise, human-readable control over job schedules
- Job state persisted in PostgreSQL survives pod restarts and redeployments without losing scheduled work
- Single database for both application data and job state — no additional infrastructure
- DI-resolved jobs can access repositories, the mediator, and other application services without service locator patterns
- OpenTelemetry integration surfaces job durations and failure traces in the Aspire dashboard alongside API request traces
- Apache 2.0 license imposes no commercial restrictions

### Negative Consequences

- Quartz.NET configuration is verbose relative to `IHostedService`; initial setup requires care to register jobs, triggers, and the job store correctly
- PostgreSQL job store requires schema initialisation (Quartz provides SQL scripts for this) — an additional migration concern separate from EF Core Migrations
- Job clustering (running across multiple instances) requires explicit configuration to avoid duplicate execution

### Neutral

- The initial development phase can use `IHostedService` + `PeriodicTimer` for simplicity, with Quartz.NET introduced incrementally when persistence is required — no big-bang switch
- Quartz.NET has been in the .NET ecosystem since .NET Framework; it is stable and widely used, though its API surface reflects its age

## Pros and Cons of the Options

### Quartz.NET

A mature, open-source job scheduling library for .NET with cron support, persistent job stores, and broad ecosystem integration.

- Good, because fully open-source (Apache 2.0) with no commercial licensing restrictions
- Good, because cron expression scheduling covers all gamification timing requirements
- Good, because PostgreSQL job store persistence means jobs survive restarts without data loss
- Good, because first-class DI integration via `AddQuartz` keeps job classes idiomatic
- Good, because OpenTelemetry support produces Aspire-visible traces for job execution monitoring
- Bad, because more configuration ceremony than simpler alternatives
- Bad, because PostgreSQL job store schema must be initialised separately from EF Core Migrations

### Hangfire

A popular .NET background job library with a web dashboard, retry support, and multiple storage backends.

- Good, because intuitive API and a built-in web dashboard for job monitoring
- Good, because strong community and documentation
- Good, because PostgreSQL storage backend available
- Bad, because some features — particularly advanced recurring job management and the full dashboard — require Hangfire Pro (commercial license)
- Bad, because the free tier's feature set is more limited than Quartz.NET's open-source offering for cron-heavy workloads

### .NET `IHostedService` + `PeriodicTimer`

Built-in .NET primitives for running background work on a recurring interval without any third-party dependencies.

- Good, because zero dependencies — fully in-box with .NET
- Good, because simple and easy to reason about for straightforward periodic tasks
- Good, because natural starting point for early development before persistence requirements solidify
- Bad, because no persistence — if the service restarts, in-progress or pending executions are lost
- Bad, because no retry mechanism for failed jobs
- Bad, because no cron expression support — interval-only scheduling is less precise for time-sensitive gamification rules (e.g., "evaluate streaks at midnight UTC")
- Bad, because no built-in dashboard or observability integration beyond manual instrumentation

### Wolverine scheduling

Wolverine's built-in scheduled message delivery, which supports delayed and recurring message dispatch with durable storage.

- Good, because unified with Wolverine messaging if already adopted for command handling
- Good, because durable scheduling backed by a persistent store
- Bad, because Wolverine's primary value is distributed messaging; using it solely for scheduling brings in unnecessary complexity
- Bad, because the team chose a custom lightweight mediator (see [ADR-010](20260305-010-cqrs-mediator.md)) rather than adopting Wolverine — coupling the scheduling concern to Wolverine would re-introduce the heavier framework dependency

## More Information

- [Quartz.NET documentation](https://www.quartz-scheduler.net/documentation/)
- [Quartz.NET DI integration (`Quartz.Extensions.Hosting`)](https://github.com/quartznet/quartznet)
- [Quartz.NET OpenTelemetry support](https://www.quartz-scheduler.net/documentation/quartz-3.x/packages/opentelemetry.html)
- Related: [ADR-003](20260305-003-database.md) — PostgreSQL used for both application data and Quartz job store persistence
- Related: [ADR-010](20260305-010-cqrs-mediator.md) — Custom mediator used by job handlers to dispatch commands and publish domain events
- Related: [ADR-012](20260305-012-observability.md) — OpenTelemetry configuration that surfaces Quartz job traces in the Aspire dashboard
