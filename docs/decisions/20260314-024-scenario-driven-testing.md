# Scenario-Driven Testing Strategy

- Status: Accepted
- Date: 2026-03-14

## Context and Problem Statement

AI agents generating tests tend toward two failure modes: (1) generating tests that mirror implementation details rather than behaviour, producing brittle tests that break on every refactor, and (2) generating tests that chase coverage numbers with trivial assertions that do not catch real bugs. Both modes produce a green test suite that provides false confidence. How should the testing strategy be structured to ensure agents write meaningful tests?

## Decision Drivers

- Tests must encode behaviours, not methods — they should survive refactoring
- The division of responsibility between human and agent must be explicit
- Test failures must block commits and produce actionable error messages

## Considered Options

- Scenario-driven tests (Given/When/Then structure)
- Method-level unit tests with coverage targets
- Property-based testing exclusively

## Decision Outcome

Chosen option: "Scenario-driven tests", because they focus on observable behaviour rather than implementation details, making them resilient to refactoring and meaningful as living documentation.

### Test structure

Each test answers: "Given [context], when [action], then [outcome]."

### Test naming

`Should_[ExpectedBehavior]_When_[Condition]`

### Test categories

Tests use `[Trait("Category", "...")]` for gate filtering:

| Category | Gate | What it tests |
|----------|------|--------------|
| `Domain` | G4 | Domain entity behaviour and value object validation |
| `Application` | G4 | Use case orchestration with stubbed ports |
| `Architecture` | G3 | Layer dependency rules via NetArchTest |
| `Api` | G4 | HTTP contract conformance via WebApplicationFactory |

### Human vs. agent ownership

- **Human** defines which scenarios exist (the Given/When/Then specification)
- **Agent** implements the production code to make those scenarios pass
- **Agent** may propose additional edge-case scenarios, but the human approves them before they are committed

### Positive Consequences

- Tests cover every defined behaviour, not just every line of code
- Refactoring production code should not break tests
- Coverage metrics are informational, not a gate

### Negative Consequences

- Requires discipline to write scenarios before implementation
- Tests may not cover every line of code (acceptable trade-off)

## More Information

- Enforcement: Gate 4 runs all tests; failures block the commit
- The AGENTS.md file explicitly forbids deleting or weakening tests — the agent must fix production code instead
- Related: [ADR-014](20260305-014-testing.md) — tooling choices for the test stack
- Related: [ADR-026](20260314-026-mutation-testing.md) — mutation testing validates how well scenario tests detect regressions
