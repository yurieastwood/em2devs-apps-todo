# Repo Structure — Monorepo with Clean Architecture Layers

- Status: Accepted
- Date: 2026-03-05

## Context and Problem Statement

The EM2Devs.Todo project consists of multiple distinct concerns: a .NET 9 Minimal API backend, a SvelteKit frontend, an Aspire AppHost orchestrator, shared Aspire ServiceDefaults, EF Core migrations, and multiple test projects. These components must be co-located in a way that makes the Clean Architecture layer boundaries explicit, keeps project naming consistent and predictable, enables Aspire's project reference model to function correctly, and allows a developer new to the codebase to orient themselves quickly. How should the repository be structured?

## Decision Drivers

- Clean Architecture compliance: the directory layout must reinforce the Domain → Application → Infrastructure → Api dependency direction; layer boundaries should be visible at a glance
- Aspire project structure: the AppHost must reference all service projects; placing everything in a single `src/` tree makes relative project references straightforward
- Naming convention: all .NET projects follow the `EM2Devs.Todo.{Concern}` convention to communicate project purpose without ambiguity and to keep NuGet package IDs consistent if packages are ever published
- Test organisation: test projects are separated from production source to prevent accidental production references to test-only packages; tests are organised per Clean Architecture concern
- Developer navigation: a developer unfamiliar with the codebase should be able to infer where a feature's logic lives by reading the directory tree alone

## Considered Options

- Monorepo with Clean Architecture layer structure (all projects under `src/`, all tests under `tests/`)
- Monorepo with feature-first organisation (features as top-level directories containing their own API, domain, and test subdirectories)
- Polyrepo (separate repositories for frontend and backend)

## Decision Outcome

Chosen option: "Monorepo with Clean Architecture layers", because it keeps the entire delivery unit — backend, frontend, orchestration, and tests — under a single version history, simplifies cross-cutting changes (e.g., updating a shared NuGet version affects all projects in one PR), and makes the Clean Architecture dependency direction explicit at the file-system level.

**Repository layout:**

```
em2devs-apps-todo/
├── docs/
│   └── decisions/                                 # ADRs (managed by log4brains)
├── src/
│   ├── EM2Devs.Todo.AppHost/                      # .NET Aspire orchestrator — references all service projects
│   ├── EM2Devs.Todo.ServiceDefaults/              # Shared Aspire config: OTel, health checks, resilience
│   ├── EM2Devs.Todo.Api/                          # .NET 9 Minimal API — endpoint definitions, filters, middleware
│   ├── EM2Devs.Todo.Domain/                       # Domain models, value objects, domain events, interfaces
│   ├── EM2Devs.Todo.Application/                  # CQRS commands, queries, handlers, validators (ref: ADR-010)
│   ├── EM2Devs.Todo.Infrastructure/               # EF Core DbContext, Dapper queries, Auth0, Redis (ref: ADR-009)
│   └── EM2Devs.Todo.Web/                          # SvelteKit frontend (ref: ADR-002)
├── tests/
│   ├── EM2Devs.Todo.Api.UnitTests/                # API endpoint and middleware unit tests
│   ├── EM2Devs.Todo.Application.UnitTests/        # Command/query handler unit tests
│   ├── EM2Devs.Todo.Domain.UnitTests/             # Domain model and domain event unit tests
│   ├── EM2Devs.Todo.Infrastructure.IntegrationTests/  # EF Core + PostgreSQL + Redis (Testcontainers)
│   ├── EM2Devs.Todo.ArchitectureTests/            # NetArchTest — enforces Clean Architecture boundaries
│   └── EM2Devs.Todo.E2E/                          # Playwright end-to-end tests (ref: ADR-014)
├── .github/
│   └── workflows/                                 # GitHub Actions pipeline definitions (ref: ADR-011)
├── .editorconfig                                   # Compiler-enforced formatting rules
├── Directory.Build.props                           # Centralised NuGet versions, shared analyzer config
├── EM2Devs.Todo.sln                               # Solution file referencing all src/ and tests/ projects
└── README.md
```

**Clean Architecture dependency rule** (enforced by `EM2Devs.Todo.ArchitectureTests`):

```
Domain  <──  Application  <──  Infrastructure
                    ^                ^
                    └──── Api ───────┘
```

Domain has no references to other application projects. Application references Domain only. Infrastructure references Domain and Application (for interfaces). Api references Application and Infrastructure (for DI registration).

**Naming convention:** All .NET projects follow `EM2Devs.Todo.{Concern}`, where `{Concern}` communicates the Clean Architecture layer (`Domain`, `Application`, `Infrastructure`, `Api`) or infrastructure role (`AppHost`, `ServiceDefaults`). Test projects append the test type: `.UnitTests`, `.IntegrationTests`, `.ArchitectureTests`, `.E2E`.

**`Directory.Build.props`** at the repository root centralises:
- `<TargetFramework>net9.0</TargetFramework>` for all .NET projects
- `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`
- Shared NuGet package versions (central package management)
- Shared analyzer configuration (SonarAnalyzer, StyleCop)

### Positive Consequences

- A single `git clone` gives a developer everything needed to run the full stack via Aspire AppHost — no separate frontend or backend clone required
- Cross-cutting changes (e.g., bumping a shared NuGet version, renaming a domain type) are atomic: one PR, one review, one merge
- The Clean Architecture layer boundaries are visible as first-class directory names; a developer can navigate to the correct project by reading the folder name alone
- `Directory.Build.props` eliminates version drift between projects and reduces the maintenance surface for build configuration
- Architecture tests enforce the dependency rules at CI time, preventing accidental layer violations from accumulating silently

### Negative Consequences

- The solution file grows as the project grows; large solutions can be slower to load in IDEs without filtering
- Frontend (`EM2Devs.Todo.Web`) and backend live in the same repository; a team that wants to give the frontend team a minimal clone experience would need sparse checkout, which adds tooling complexity
- `Directory.Build.props` centralisation means a misconfiguration affects all projects simultaneously; care is required when modifying it

### Neutral

- The SvelteKit frontend is a Node.js project and does not participate in the `.sln` solution file; it is included in the monorepo as a directory within `src/` alongside the .NET projects
- Aspire's `EM2Devs.Todo.AppHost` project references the .NET service projects directly via `<ProjectReference>`; the SvelteKit frontend is launched by Aspire as an npm process resource

## Pros and Cons of the Options

### Monorepo with Clean Architecture layers

All components live in one repository, organised by Clean Architecture concern under `src/` and by test type under `tests/`.

- Good, because atomic cross-cutting changes — one PR covers backend, frontend, and tests for a feature
- Good, because single `dotnet run --project src/EM2Devs.Todo.AppHost` starts the entire stack
- Good, because `Directory.Build.props` and `.editorconfig` apply consistently across all projects without duplication
- Good, because Clean Architecture layer names are first-class in the directory tree, making navigation intuitive
- Bad, because repository grows in size and scope as the project matures; eventually may warrant a split

### Monorepo with feature-first organisation

Top-level directories represent features (e.g., `todos/`, `achievements/`, `auth/`), each containing their own API routes, domain models, and tests.

- Good, because all code related to a feature is co-located — a developer working on achievements touches only one top-level directory
- Bad, because Clean Architecture boundaries are not visible in the directory structure; enforcing them requires stricter tooling and convention
- Bad, because shared infrastructure (EF Core DbContext, auth middleware) does not belong cleanly to any single feature and becomes an unnamed residual directory
- Bad, because Aspire project references and solution structure do not map cleanly to a feature-first layout

### Polyrepo (separate frontend and backend repositories)

Frontend and backend live in separate repositories, potentially with a third repository for shared types or contracts.

- Good, because independent deployment pipelines; frontend can deploy without triggering backend CI
- Good, because teams can be granted scoped access to only the relevant repository
- Bad, because cross-cutting changes (e.g., adding a new API endpoint that the frontend consumes) require coordinated PRs across multiple repositories
- Bad, because shared configuration (linting rules, commit conventions) must be duplicated or extracted to a separate config repository
- Bad, because at the project's current scale, the operational overhead of managing multiple repositories outweighs the isolation benefit

## More Information

- [Clean Architecture — Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [.NET Aspire project structure](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/app-host-overview)
- [MSBuild Directory.Build.props](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory)
- [NetArchTest](https://github.com/BenMorris/NetArchTest)
- Related: [ADR-005](20260305-005-orchestration.md) — Aspire AppHost references all service projects within this structure
- Related: [ADR-014](20260305-014-testing.md) — Test project naming and organisation defined here
- Related: [ADR-011](20260305-011-ci-cd.md) — GitHub Actions workflows live in `.github/workflows/` within this layout
- Related: [ADR-016](20260305-016-code-quality.md) — `.editorconfig` and `Directory.Build.props` are root-level files in this structure
