# Read-Model Trio Tracked as Dapper Migration Candidates

- Status: Accepted
- Date: 2026-05-03

## Context and Problem Statement

ADR-009 splits data access between EF Core (write side) and Dapper (read side). When implementing the Postgres equivalents of `IInsightCardRepository`, `IEnergyCheckInRepository`, `ITimelineRepository`, and `IWeeklyReflectionRepository`, all four are read-model-shaped — they hydrate flat rows for display and don't participate in domain command flows.

Strictly applying ADR-009 in the same change would have required mixing EF Core and Dapper inside a single PR closing the persistence gap. Doing so doubles the testing surface (EF Core integration tests + Dapper integration tests) and obscures the structural progress (closing the in-memory gap). How should we treat the read-model repositories given the deliberate ADR-009 deviation?

## Decision Drivers

- ADR-009 prescribes Dapper for read paths.
- The persistence gap (six in-memory-only repositories) was the highest-priority correctness concern; closing it cleanly was the goal of this work.
- Mixing Dapper and EF Core inside a single batch doubles the integration-test footprint and the DI plumbing.
- We have no profiling evidence yet that any of these read paths are hot.

## Considered Options

- Honor ADR-009 up front — implement the four read-model repos in Dapper inside the same batch.
- Implement everything uniformly in EF Core and silently accept the deviation.
- Implement uniformly in EF Core and explicitly track the deviation as Dapper migration candidates with a follow-up signal in the code and an ADR.

## Decision Outcome

Chosen option: "Implement uniformly in EF Core and explicitly track the deviation," because it ships the persistence gap fix in one focused batch while keeping the ADR-009 deviation visible to future contributors. The TODO comments at the top of each affected repo file point at this ADR, making the trade-off discoverable from the implementation rather than buried in commit history.

The four read-model-shaped repositories — `PostgresInsightCardRepository`, `PostgresEnergyCheckInRepository`, `PostgresTimelineRepository`, `PostgresWeeklyReflectionRepository` — are tagged as **Dapper migration candidates** to be revisited when at least one of the following becomes true:

1. Profiling shows one or more of these repos as hot enough to justify the projection layer.
2. The query shape outgrows what EF Core projection can express ergonomically (e.g., complex CTEs, multi-table joins, materialized-view-style aggregations).
3. Schema reaches a point where a separate read-model table or materialised view earns its keep.

Until then, all four remain on EF Core. Each carries a `// TODO(ADR-029)` comment at the top so the deviation is visible from the implementation file.

### Positive Consequences

- Single PR closes the in-memory persistence gap with a uniform pattern.
- Tests use a single integration framework (Testcontainers + EF Core).
- No new tooling or DI plumbing for Dapper.
- The deviation is explicit and discoverable, not silent.

### Negative Consequences

- ADR-009 deviation is now real, not hypothetical. Future contributors may model new read paths after the EF Core impls and never migrate.
- If a hot path emerges, the swap to Dapper is a one-by-one migration rather than an architecture-wide pattern.

### Neutral

- The interface contracts remain unchanged. A future Dapper migration can replace the implementations without touching callers.

## Pros and Cons of the Options

### Honor ADR-009 up front (mixed EF Core + Dapper)

- Good, because the architecture stays uniform with the prescribed read/write split.
- Good, because hot paths land on the optimised reader from day one.
- Bad, because it doubles the integration-test footprint inside a single batch (EF Core integration tests + Dapper integration tests).
- Bad, because it adds Dapper-specific DI plumbing while the primary goal (closing the in-memory gap) is structural.
- Bad, because there is no profiling evidence today that any of these read paths are hot enough to justify the cost.

### Uniformly EF Core, silent deviation

- Good, because it's the simplest path to closing the gap.
- Bad, because future contributors lose visibility into the trade-off and may model new read paths after the EF Core impls indefinitely.

### Uniformly EF Core, explicitly tracked (chosen)

- Good, because it ships the gap fix in one focused batch.
- Good, because the deviation is discoverable both from the implementation files (TODO comments) and from the ADR record.
- Good, because the swap to Dapper is straightforward when a trigger emerges (interface contracts unchanged).
- Bad, because the architecture has a documented inconsistency until at least one read-model is migrated.

## More Information

- ADR-009: Data Access — EF Core for Writes, Dapper for Reads.
- Local design spec: `docs/superpowers/specs/2026-05-03-postgres-persistence-design.md` (gitignored).
- Affected files:
  - `src/EM2Devs.Todo.Infrastructure/Persistence/PostgresInsightCardRepository.cs`
  - `src/EM2Devs.Todo.Infrastructure/Persistence/PostgresEnergyCheckInRepository.cs`
  - `src/EM2Devs.Todo.Infrastructure/Persistence/PostgresTimelineRepository.cs`
  - `src/EM2Devs.Todo.Infrastructure/Persistence/PostgresWeeklyReflectionRepository.cs`
