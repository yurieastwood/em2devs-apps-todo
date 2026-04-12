# Waypoint

[![CI Pipeline](https://github.com/yurieastwood/em2devs-apps-todo/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/yurieastwood/em2devs-apps-todo/actions/workflows/ci.yml) [![BDD Docs](https://github.com/yurieastwood/em2devs-apps-todo/actions/workflows/docs-bdd.yml/badge.svg?branch=main)](https://github.com/yurieastwood/em2devs-apps-todo/actions/workflows/docs-bdd.yml) [![ADR Docs](https://github.com/yurieastwood/em2devs-apps-todo/actions/workflows/docs-adr.yml/badge.svg?branch=main)](https://github.com/yurieastwood/em2devs-apps-todo/actions/workflows/docs-adr.yml)
![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
[![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)](LICENSE)

A gamified productivity app that turns task management into an RPG-style progression system. Complete quests, earn XP, level up, unlock skill trees, and collaborate with others — all while getting things done.

## Features

| Category | Highlights |
|---|---|
| **Core** | Tasks, quests (epics & sagas), recurring tasks, boss tasks, notifications |
| **Progression** | XP, levelling, skill trees, titles & ranks, streaks, seasonal content |
| **Intelligence** | Energy-aware scheduling, capacity modelling, time estimation, daily brief, procrastination detection |
| **Social** | Guilds, accountability partners, leaderboards, shared quests, challenge mode |
| **Reflection** | Weekly review, journey timeline, insight cards, annual wrapped |
| **Onboarding** | Progressive disclosure — clean interface that reveals depth as engagement increases |
| **Data** | Local-first with sync, full data export and deletion |
| **Monetisation** | Free and Premium tiers — no pay-to-win; cosmetics and advanced features only |

Full BDD specifications in Gherkin format are available under [`docs/features/`](docs/features/).

## Tech Stack

| Concern | Choice |
|---|---|
| Backend | .NET 9, REST Minimal APIs + SignalR |
| Frontend | SvelteKit / Svelte 5 (runes) |
| Database | PostgreSQL with JSONB |
| Data Access | EF Core (writes) + Dapper (reads) |
| Orchestration | .NET Aspire |
| Authentication | Auth0 (social logins) |
| Caching | Redis (via Aspire) |
| Background Jobs | Quartz.NET |
| Observability | Grafana stack (Prometheus + Loki + Tempo), Serilog, Aspire Dashboard (dev) |
| CI/CD | GitHub Actions |
| Testing | xUnit + Shouldly + NSubstitute + Testcontainers / Vitest + Playwright |

## Architecture

Waypoint follows **Clean Architecture** with **CQRS** via a custom lightweight mediator. Domain events drive side effects. The dependency rule is enforced at CI time by architecture tests.

```
Domain  <──  Application  <──  Infrastructure
                  ^                ^
                  └──── Api ───────┘
```

- **Domain** — models, value objects, domain events, interfaces
- **Application** — commands, queries, handlers, validators
- **Infrastructure** — EF Core, Dapper, Auth0, Redis integrations
- **Api** — Minimal API endpoints, filters, middleware

## Project Structure

```
waypoint/
├── docs/
│   ├── decisions/          # Architecture Decision Records (MADR 3.0)
│   └── features/           # 29 BDD feature specs (Gherkin)
├── src/
│   ├── EM2Devs.Todo.AppHost/          # .NET Aspire orchestrator
│   ├── EM2Devs.Todo.ServiceDefaults/  # Shared Aspire config (OTel, health checks)
│   ├── EM2Devs.Todo.Api/             # Minimal API endpoints
│   ├── EM2Devs.Todo.Domain/          # Domain layer
│   ├── EM2Devs.Todo.Application/     # CQRS handlers
│   ├── EM2Devs.Todo.Infrastructure/  # Data access & integrations
│   └── EM2Devs.Todo.Web/            # SvelteKit frontend
├── tests/
│   ├── EM2Devs.Todo.Domain.UnitTests/
│   ├── EM2Devs.Todo.Application.UnitTests/
│   ├── EM2Devs.Todo.Api.UnitTests/
│   ├── EM2Devs.Todo.Infrastructure.IntegrationTests/
│   ├── EM2Devs.Todo.ArchitectureTests/
│   └── EM2Devs.Todo.E2E/            # Playwright
├── .editorconfig
├── Directory.Build.props             # Centralised build config
└── EM2Devs.Todo.sln
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/) (for the SvelteKit frontend)
- [Docker](https://www.docker.com/) (required by Aspire for PostgreSQL, Redis, and observability containers)

## Getting Started

```bash
# Clone the repository
git clone https://github.com/yurieastwood/em2devs-apps-todo.git
cd em2devs-apps-todo

# Start the full stack via Aspire
dotnet run --project src/EM2Devs.Todo.AppHost
```

The Aspire dashboard is available at the URL printed in the console (e.g., `https://localhost:17178/login?t=<token>`). The API port is configured to **5001**.

### Applying Database Migrations

Migrations are applied automatically at startup when `AUTO_MIGRATE=true` is set by the AppHost (see [ADR-020](docs/decisions/20260305-020-db-migrations.md)). To apply them manually instead:

1. Open the Aspire dashboard and find the **postgres** resource to get the connection details (host, port, password)
2. Restore local tools and apply the migration:

```bash
dotnet tool restore
dotnet dotnet-ef database update \
  --project src/EM2Devs.Todo.Infrastructure \
  --startup-project src/EM2Devs.Todo.Api \
  --connection "Host=localhost;Port=<port>;Database=tododb;Username=postgres;Password=<password>"
```

## Quality Pipeline

Automated checks enforce code quality at two stages ([ADR-028](docs/decisions/20260406-028-pipeline-restructuring.md)):

### Commit Stage (pre-commit hook + CI)

| Check | What it validates |
|-------|------------------|
| Build | Strongly-typed value objects + Roslyn analyzers (`TreatWarningsAsErrors`) |
| Format | `.editorconfig` + `dotnet format` |
| Frontend Lint | Svelte type check + ESLint + Prettier (when frontend files staged) |
| Contract Lint | Spectral (spec structure) + coverage check (all operations documented) |
| Architecture | NetArchTest rules enforcing Clean Architecture layer boundaries |
| Tests | Behaviour tests via xUnit + Shouldly |
| Security | `dotnet list package --vulnerable --include-transitive` |

### Acceptance Stage (pre-push hook + CI)

| Check | What it validates |
|-------|------------------|
| Contract Test | Schemathesis property-testing against running API |
| Mutation | Stryker.NET on Domain layer — zero surviving mutants |
| E2E | Playwright end-to-end tests (CI only) |

Run all checks locally:

```bash
./scripts/run-gates.sh
```

## Architecture Decision Records

All architectural decisions are documented as ADRs using [MADR 3.0](https://adr.github.io/madr/) format in [`docs/decisions/`](docs/decisions/).

## For AI Agents

Read [AGENTS.md](AGENTS.md) before writing any code. It defines the workflow, tool sequencing, and constraints.

## Code Quality

- **Conventional commits** and **conventional branches** enforced via git hooks (`git config core.hooksPath scripts/hooks`)
- **SonarAnalyzer** + **StyleCop** for .NET static analysis
- **ESLint** + **Prettier** for the frontend
- **Architecture tests** (NetArchTest) enforce Clean Architecture layer boundaries at CI time

## License

This project is licensed under the [GNU Affero General Public License v3.0](LICENSE).
