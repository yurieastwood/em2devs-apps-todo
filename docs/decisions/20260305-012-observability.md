# Observability — Grafana Stack + Serilog + Aspire Dashboard

- Status: Accepted
- Date: 2026-03-05

## Context and Problem Statement

A production-grade application requires visibility into all three observability signals: structured logs, runtime metrics, and distributed traces. Without this visibility, diagnosing failures, tracking performance regressions, and understanding system behaviour under load is guesswork. The gamification engine in particular — XP awards, streak tracking, achievement checks — produces background workloads whose execution must be traceable end-to-end. Additionally, the development inner loop should not require a full production observability stack to inspect traces and logs locally. Which observability tools and approach should cover both development and production?

## Decision Drivers

- Three signals coverage: logs, metrics, and traces must all be captured and queryable
- OpenTelemetry compatibility: signals should flow through the OTel standard to avoid vendor lock-in
- Aspire integration: the project already uses .NET Aspire, which configures OTel ServiceDefaults; the observability stack should build on that foundation rather than replace it
- Open-source: the production stack must be deployable without per-seat or per-host licensing fees
- .NET ecosystem fit: the structured logging library should be widely adopted in the .NET community and support rich property enrichment
- Development experience: developers should be able to inspect traces, logs, and metrics during local development without external accounts or heavy infrastructure setup

## Considered Options

- Grafana stack (Prometheus + Loki + Tempo + Grafana)
- Azure Monitor / Application Insights
- Datadog / New Relic
- Seq + Prometheus

## Decision Outcome

Chosen option: "Multi-layer approach using Aspire Dashboard for development and Grafana stack for production, with Serilog as the structured logging library throughout", because it covers all three observability signals with open-source tools, integrates cleanly with the OTel pipeline that Aspire ServiceDefaults already configures, and provides a zero-config inner loop experience for developers.

The approach has three layers:

**Development (local):** The .NET Aspire Dashboard (`aspire-dashboard`) provides built-in trace, log, and metric visualisation at zero configuration cost. It is automatically available when running via the AppHost. Developers can inspect traces for any request or background job without setting up Prometheus or Grafana locally.

**Production:** The Grafana stack runs in containers:
- **Prometheus** — scrapes metrics exposed by the `aspire-dashboard` OTel metrics endpoint and application `/metrics` endpoint
- **Loki** — receives structured log streams forwarded from the OTel log exporter
- **Tempo** — receives distributed traces via the OTel OTLP exporter
- **Grafana** — unified dashboard layer querying Prometheus, Loki, and Tempo; supports correlation between signals (e.g., click a log line to jump to its trace)

**Structured logging:** Serilog is used throughout the .NET backend. It is the de facto standard for structured logging in the .NET community, with a rich enricher ecosystem (request IDs, correlation IDs, machine name, environment). The OTel sink (`Serilog.Sinks.OpenTelemetry`) routes log events into the same OTel pipeline already configured by Aspire ServiceDefaults, meaning logs, metrics, and traces share the same propagation context.

During development, the Grafana stack containers can optionally be started via Aspire AppHost resource definitions, mirroring the production topology for debugging production-specific observability behaviour.

### Positive Consequences

- All three signals are correlated through shared OTel trace and span IDs, enabling root-cause analysis that jumps between logs, metrics, and traces for the same request
- Aspire Dashboard provides immediate, zero-cost observability during the development inner loop; developers can inspect gamification event chains (TodoCompleted -> XpAwarded -> AchievementChecked) as traces without any infrastructure overhead
- Grafana's unified query layer (Explore) lets operators navigate from a Prometheus metric spike to the contributing Loki log lines to the Tempo trace, all in one tool
- The OTel-first design means the backend emits to an OTel collector; swapping the backend storage (e.g., replacing Tempo with Jaeger) requires only exporter reconfiguration, not code changes
- Serilog's enricher pipeline ensures every log event carries correlation IDs and request context automatically

### Negative Consequences

- Self-hosting Prometheus, Loki, Tempo, and Grafana in production requires operational capacity: upgrades, storage sizing for Loki and Tempo, Prometheus retention management
- Grafana stack container configuration (datasource provisioning, dashboard JSON) must be version-controlled and maintained alongside the application
- The OTel sink for Serilog (`Serilog.Sinks.OpenTelemetry`) adds a NuGet dependency; if the sink lags behind the OTel SDK version, compatibility issues may arise

### Neutral

- Grafana Alloy (formerly Agent) can be introduced later as an OTel Collector replacement if centralised scraping, buffering, or transformation of signals is needed in production
- Aspire Dashboard is development-only; it does not provide long-term signal retention and is not a production replacement for the Grafana stack

## Pros and Cons of the Options

### Grafana Stack (Prometheus + Loki + Tempo + Grafana)

A composable, open-source observability stack where each component handles one signal and Grafana provides unified querying and dashboarding.

- Good, because fully open-source with no per-host or per-seat licensing
- Good, because each component integrates natively with OTel exporters; no proprietary SDK required
- Good, because Grafana's correlation features (Explore, exemplars, trace-to-logs links) connect all three signals in a single UI
- Good, because containerised deployment via Docker Compose or Aspire resource definitions mirrors the production topology during development
- Bad, because self-hosting adds operational overhead: storage, retention policies, and upgrades must be managed
- Bad, because initial setup (datasource provisioning, dashboards) requires upfront configuration work

### Azure Monitor / Application Insights

Microsoft's managed observability service, tightly integrated with Azure hosting.

- Good, because fully managed; no infrastructure to operate
- Good, because first-class .NET SDK integration and automatic instrumentation for ASP.NET Core
- Bad, because Azure-specific; creates lock-in that limits portability to other cloud providers
- Bad, because costs at scale (data ingestion, retention) can grow significantly; pricing is per GB ingested
- Bad, because the Application Insights SDK is a proprietary layer on top of OTel, making future migration harder

### Datadog / New Relic

Commercial SaaS observability platforms with comprehensive out-of-the-box integrations.

- Good, because turn-key setup with auto-instrumentation, dashboards, and alerting included
- Good, because no infrastructure to operate; managed retention and scaling
- Bad, because high per-host or per-user SaaS costs are disproportionate for a project at this scale
- Bad, because proprietary agents and SDKs introduce vendor lock-in at the instrumentation level

### Seq + Prometheus

Seq provides excellent structured log search and dashboarding for .NET applications; Prometheus handles metrics separately.

- Good, because Seq is purpose-built for .NET structured logs and has a polished search UI
- Good, because Prometheus is the de facto standard for metrics
- Bad, because Seq does not handle distributed traces; a third tool (e.g., Jaeger) would still be needed for full three-signal coverage
- Bad, because Seq has a commercial license for teams; Loki is a like-for-like open-source alternative that integrates directly with Grafana
- Bad, because two separate UIs (Seq for logs, Grafana for metrics/traces) reduce the ability to correlate signals in one place

## More Information

- [OpenTelemetry .NET SDK](https://opentelemetry.io/docs/languages/net/)
- [Serilog OpenTelemetry sink](https://github.com/serilog/serilog-sinks-opentelemetry)
- [Grafana Loki documentation](https://grafana.com/docs/loki/latest/)
- [Grafana Tempo documentation](https://grafana.com/docs/tempo/latest/)
- [Aspire Dashboard documentation](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/overview)
- Related: [ADR-005](20260305-005-orchestration.md) — .NET Aspire configures OTel ServiceDefaults, which is the foundation this observability stack builds on
