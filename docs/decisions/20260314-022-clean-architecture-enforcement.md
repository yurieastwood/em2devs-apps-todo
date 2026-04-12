# Clean Architecture Enforcement via Architecture Tests

- Status: Accepted
- Date: 2026-03-14

## Context and Problem Statement

AI coding agents tend to take the shortest path, which often means creating direct dependencies between layers that should be decoupled. Without enforceable boundaries, agents will reference database classes from domain logic, call HTTP clients from use cases, and generally erode the architecture within a few iterations. How can the Clean Architecture layer boundaries defined in ADR-015 be enforced automatically?

## Decision Drivers

- Agents must receive immediate, actionable feedback when they violate layer boundaries
- Enforcement must run both locally (pre-commit) and in CI — no escape hatch
- Error messages must reference the specific ADR and the violating type so the agent can self-correct

## Considered Options

- NetArchTest rules enforced as unit tests (Gate 3)
- Manual code review for dependency violations
- Roslyn analyzers with custom rules

## Decision Outcome

Chosen option: "NetArchTest rules as unit tests", because they integrate naturally with the existing test runner, produce clear pass/fail results that agents understand, and can be included in both pre-commit hooks and CI without additional tooling.

The architecture tests live in `EM2Devs.Todo.ArchitectureTests` and enforce the following forbidden dependencies:

- **Domain** must not reference Application, Infrastructure, or Api
- **Application** must not reference Infrastructure or Api
- **Infrastructure** must not reference Api

### Positive Consequences

- Every interface must be defined in Application, implemented in Infrastructure
- Domain logic must be pure — no I/O, no frameworks, no NuGet packages beyond the BCL
- Agents receive a compile-time-like gate that blocks commits violating the architecture
- Error messages include the violating type name and the ADR reference

### Negative Consequences

- New dependencies between layers require a new ADR and human approval — this adds friction, but that friction is intentional
- NetArchTest operates on compiled assemblies, so a successful build is a prerequisite

## More Information

- Related: [ADR-015](20260305-015-repo-structure.md) — defines the Clean Architecture layer structure this ADR enforces
- Related: [ADR-014](20260305-014-testing.md) — architecture tests are part of the test strategy
- Enforcement: Gate 3 in the CI pipeline and pre-commit hook
