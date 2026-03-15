# Mutation Testing with Stryker.NET

- Status: Accepted
- Date: 2026-03-14

## Context and Problem Statement

Scenario-driven tests (ADR-024) verify that defined behaviours work correctly, but they do not guarantee that the tests would catch regressions. An AI agent can write production code that happens to pass existing tests while containing logic that is never actually validated — dead conditions, redundant branches, or boundary checks that no test exercises. Code coverage metrics do not solve this: a test can execute every line without asserting on the result. How can the quality of tests themselves be validated?

## Decision Drivers

- Surviving mutants represent real test gaps — logic that can change without any test noticing
- The gate must produce actionable feedback pointing to specific lines and mutations
- Mutation testing is slow; it should run only where it provides the most value

## Considered Options

- Stryker.NET mutation testing scoped to the Domain layer
- Full-codebase mutation testing
- No mutation testing (rely on coverage metrics)

## Decision Outcome

Chosen option: "Stryker.NET scoped to the Domain layer", because the Domain contains the core business rules (value object validation, entity behaviour, status transitions) where test gaps are most dangerous. Application, Infrastructure, and Api layers are better validated by integration and contract tests, not mutation testing.

### Zero surviving mutants

The invariant is simple: no mutant may survive. A surviving mutant means production code can be changed in a meaningful way without any test noticing — that is a test gap, and test gaps are bugs waiting to happen.

Stryker enforces this via score thresholds (all set to 100%). This is achievable because the Domain is intentionally small and every behaviour is covered by scenario-driven tests.

### Configuration

Stryker is installed as a local dotnet tool (`.config/dotnet-tools.json`) and configured via `stryker-config.json` at the repository root.

### Positive Consequences

- Surviving mutants produce actionable feedback: specific lines and mutations that no test catches
- Complements ADR-024 — scenario-driven tests define what to test, mutation testing validates how well those tests detect regressions

### Negative Consequences

- Mutation testing is slow relative to other gates (runs the full test suite per mutant) — it runs last in the gate sequence
- Scope is limited to Domain; other layers rely on integration and contract tests

## More Information

- Enforcement: Gate 7 in the CI pipeline and pre-push hook; runs after Gate 4 (tests must pass first)
- Related: [ADR-024](20260314-024-scenario-driven-testing.md) — the testing strategy this ADR extends
- Related: [ADR-014](20260305-014-testing.md) — tooling choices for the test stack
