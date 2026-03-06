# Database Migrations — EF Core Migrations via CI/CD Pipeline

- Status: Accepted
- Date: 2026-03-05

## Context and Problem Statement

As the application evolves, the PostgreSQL database schema must evolve with it. In a containerised production environment, multiple application instances may start concurrently, and the database must be in a consistent state before any instance begins serving traffic. Applying migrations at application startup is a well-known antipattern in this scenario because it creates race conditions and makes rollback difficult. A migration strategy is needed that is safe for multi-instance deployments, auditable, and integrable with the CI/CD pipeline. Which approach should manage database schema evolution?

## Decision Drivers

- Safety in multi-instance deployments: migrations must not be applied concurrently by competing instances
- Auditability: schema changes must be traceable — what changed, when, and in which deployment
- CI/CD integration: migrations should run as an automated pipeline step, not a manual operation
- Developer experience: local development workflow must remain simple and fast
- Existing tooling: prefer to leverage EF Core's built-in migration infrastructure already in use for writes
- Rollback strategy: failed migrations should prevent deployment, not leave the schema in an inconsistent state

## Considered Options

- EF Core Migrations applied at app startup (`Database.MigrateAsync()`)
- EF Core Migrations applied via a dedicated CI/CD pipeline step (`dotnet ef database update`)
- DbUp / FluentMigrator (SQL-file-based migration frameworks)

## Decision Outcome

Chosen option: "EF Core Migrations via CI/CD pipeline step", because it eliminates the race condition risk inherent in startup-time migration, keeps schema changes explicit and auditable in the deployment pipeline, and leverages the EF Core migration infrastructure already in use for the write side without introducing additional tooling. Migrations run as a `dotnet ef database update` step in the GitHub Actions workflow before new container images are deployed. EF Core's `__EFMigrationsHistory` table ensures idempotency — already-applied migrations are skipped automatically.

### Positive Consequences

- No race condition risk: a single pipeline step applies migrations before any application instance starts
- Deployment pipeline enforces migration success as a gate — if migrations fail, container deployment does not proceed
- `__EFMigrationsHistory` provides a complete, queryable audit log of applied migrations in the database itself
- EF Core generates migration SQL that can be reviewed in pull requests before merging (`dotnet ef migrations script`)
- Local development workflow remains simple: developers run `dotnet ef database update` from the CLI or use `Database.EnsureCreated()` / `MigrateAsync()` in development-only startup code
- No additional tools or frameworks are required beyond EF Core, which is already a project dependency

### Negative Consequences

- The CI/CD pipeline must have network access to the production PostgreSQL instance during the migration step, which requires careful secrets management and network configuration
- Rollback requires a reverse migration or manual intervention — EF Core does not automatically roll back applied migrations on failure; the pipeline must be configured to fail-fast before container swap
- Developers must remember to generate and commit migration files when changing the EF Core model; missing migrations are caught in CI but can slow the feedback loop

### Neutral

- For local development environments, `Database.EnsureCreated()` or `MigrateAsync()` in a development startup path remains acceptable — the pipeline-only rule applies specifically to staging and production
- The migration step is a `dotnet` CLI invocation that is simple to reproduce locally for debugging pipeline issues

## Pros and Cons of the Options

### EF Core Migrations applied at app startup

Call `Database.MigrateAsync()` during `IHostedService` startup or in the `Program.cs` initialisation block so that the application automatically applies pending migrations when it starts.

- Good, because zero pipeline configuration required — migrations are self-contained in the application
- Good, because developers get automatic schema updates without remembering to run CLI commands
- Bad, because in a containerised deployment with multiple instances starting simultaneously, multiple processes will attempt to apply the same migrations concurrently, risking data corruption or migration failures
- Bad, because a failed migration crashes the application at startup, making the failure mode harder to distinguish from other startup errors
- Bad, because there is no explicit deployment gate — a bad migration reaches the database at the same moment traffic starts flowing

### EF Core Migrations via CI/CD pipeline step

Apply `dotnet ef database update` as a dedicated step in the GitHub Actions deployment pipeline, before container images are deployed to the runtime environment.

- Good, because a single sequential step eliminates concurrent migration execution entirely
- Good, because a failed migration fails the pipeline and prevents the new container from being deployed, protecting the running version
- Good, because migration SQL can be reviewed in PRs via `dotnet ef migrations script` output committed to the repository
- Good, because `__EFMigrationsHistory` provides a durable, queryable audit log directly in the database
- Good, because no new tools are needed — EF Core CLI tools are already part of the developer toolchain
- Bad, because the pipeline must be granted direct database access, which adds secrets management surface area
- Bad, because developers must remember to generate and commit EF Core migration files with model changes

### DbUp / FluentMigrator (SQL-file-based migration frameworks)

Use a dedicated SQL-file migration framework where each schema change is a hand-written SQL script applied in sequence and tracked in a migrations history table.

- Good, because migrations are plain SQL files, readable by DBAs and developers without ORM knowledge
- Good, because complete control over the exact SQL applied — no ORM query translation surprises
- Bad, because duplicates what EF Core already provides via its `Migrations` folder and `__EFMigrationsHistory` table — two migration histories would need to be kept in sync
- Bad, because developers must write raw SQL migrations alongside EF Core model changes, increasing the per-change effort and the risk of divergence between the model and the schema
- Bad, because introduces an additional dependency and toolchain entry point when EF Core CLI already covers the requirement

## More Information

- [EF Core Migrations documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [`dotnet ef migrations script` — generating idempotent SQL scripts](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying#sql-scripts)
- [DbUp documentation](https://dbup.readthedocs.io/)
- [FluentMigrator documentation](https://fluentmigrator.github.io/)
- Related: [ADR-009](20260305-009-data-access.md) — EF Core used for the write side; its Migrations feature is the basis for this strategy
- Related: [ADR-011](20260305-011-ci-cd.md) — GitHub Actions CI/CD pipeline that hosts the migration step
- Related: [ADR-003](20260305-003-database.md) — PostgreSQL as the target database for all migrations
