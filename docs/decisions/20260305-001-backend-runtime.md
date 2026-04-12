# Backend Runtime — .NET 9 (STS)

- Status: Accepted
- Date: 2026-03-05

## Context and Problem Statement

The EM2Devs.Todo backend needs a runtime that can deliver a modern API, support gamification workloads, and keep the development experience productive. The .NET ecosystem offers multiple active release tracks simultaneously, each with different feature sets and support timelines. Which .NET version should be used as the foundation for the backend?

## Decision Drivers

- Performance: throughput, latency, and memory efficiency for an API handling concurrent user requests
- Modern API patterns: minimal APIs, native AOT compilation, built-in OpenAPI generation
- Developer experience: reduced boilerplate, improved diagnostics, tooling quality
- Ecosystem maturity: NuGet package compatibility, third-party library support
- Support timeline: how long the version receives security patches and bug fixes
- Upgrade path: how straightforward it is to migrate to the next version when it becomes available

## Considered Options

- .NET 9 (STS — latest stable as of 2026-03)
- .NET 8 (LTS — long-term support, 3-year support window)
- .NET 10 (Preview — in active development, not yet GA)

## Decision Outcome

Chosen option: ".NET 9 (STS)", because it is the latest stable release and includes the most complete set of modern developer experience improvements (minimal APIs, native AOT, first-class OpenAPI generation via `Microsoft.AspNetCore.OpenApi`, improved `System.Text.Json` performance and source generation). Its 18-month support window comfortably covers the project's development and initial production lifetime, and upgrading to .NET 10 LTS when it reaches GA is well-established as a low-friction process.

### Positive Consequences

- Access to all .NET 9 performance improvements (JIT, GC, LINQ, collections)
- Built-in OpenAPI document generation without third-party packages for baseline scenarios
- Native AOT support allows exploring cold-start optimisation for container deployments
- Improved `System.Text.Json` source generation reduces serialisation overhead
- Minimal API refinements (typed results, `IEndpointFilter`, `RouteGroupBuilder`) keep API code lean
- Straightforward upgrade path to .NET 10 LTS once GA

### Negative Consequences

- STS support ends ~May 2026; the team must plan a .NET 10 upgrade before end of support
- A small number of third-party packages may not yet have fully adopted .NET 9 target frameworks (mitigated by net8.0 compatibility)

### Neutral

- .NET 10 preview packages are installable alongside .NET 9, allowing early experimentation without committing to the preview runtime in production
- No change to existing .NET knowledge base — .NET 9 is fully backward-compatible with .NET 8 library patterns

## Pros and Cons of the Options

### .NET 9 (STS — latest stable)

The current stable release at project inception. Ships with minimal API improvements, native AOT hardening, built-in OpenAPI support, and the best-in-class performance numbers in the .NET series to date.

- Good, because all modern API patterns are available without workarounds
- Good, because built-in OpenAPI generation removes `Swashbuckle` / `NSwag` as a hard dependency for baseline documentation
- Good, because native AOT is production-stable, enabling leaner container images
- Good, because upgrade to .NET 10 is a well-trodden, low-risk path
- Bad, because 18-month STS window is shorter than .NET 8 LTS; requires a planned upgrade

### .NET 8 (LTS)

The previous major release with a 3-year LTS support window ending November 2026. Fully stable and widely adopted.

- Good, because 3-year LTS provides a longer support runway without forced upgrades
- Good, because maximum third-party package compatibility
- Bad, because misses DX improvements shipped in .NET 9 (OpenAPI generation, minimal API refinements, improved diagnostics)
- Bad, because using an older stable when a newer stable is available introduces unnecessary technical lag at project inception

### .NET 10 (Preview)

The next major release, in active development at time of decision. Not yet production-ready.

- Good, because would eventually be LTS with 3-year support
- Good, because allows building against future-facing APIs
- Bad, because preview releases carry breaking changes between preview drops
- Bad, because not suitable for production use; no support SLA
- Bad, because tooling and third-party package compatibility is incomplete during preview

## More Information

- [.NET release schedule and support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [.NET 9 What's New](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview)
- [ASP.NET Core 9 What's New](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-9.0)
- Related: [ADR-004](20260305-004-api-style.md) — Minimal APIs as the primary API pattern
- Related: [ADR-005](20260305-005-orchestration.md) — .NET Aspire for service orchestration and observability
