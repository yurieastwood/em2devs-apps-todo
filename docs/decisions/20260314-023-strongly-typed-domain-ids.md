# Strongly-Typed Domain Identifiers and Value Objects

- Status: Accepted
- Date: 2026-03-14

## Context and Problem Statement

AI agents frequently swap function arguments when types are identical. A method like `CreateTask(string title, string createdBy)` invites the agent to pass arguments in the wrong order — and the compiler will not catch it because both are `string`. This class of bug is subtle, passes code review, and surfaces in production. How can the type system be leveraged to prevent primitive obsession and argument-swapping errors?

## Decision Drivers

- The compiler (Gate 1) should catch type-swapping errors at build time, not at runtime
- Domain concepts with identity or semantic meaning should be distinguishable at the type level
- Value objects should validate on construction and be immutable

## Considered Options

- Strongly-typed value objects as C# `record` types
- Raw primitives (`string`, `Guid`) with naming conventions
- Source-generated strongly-typed IDs (e.g., StronglyTypedId library)

## Decision Outcome

Chosen option: "Strongly-typed value objects as C# record types", because they are simple, require no external dependencies, validate on construction, and make the compiler enforce type safety without additional tooling.

All domain concepts that carry identity or semantic meaning are wrapped in distinct value objects in `EM2Devs.Todo.Domain.ValueObjects`:

| Concept | Type | Underlying |
|---------|------|-----------|
| Task identifier | `TaskId` | `Guid` |
| Task title | `TaskTitle` | `string` (1-200 chars, non-empty) |
| Task status | `TaskStatus` | `enum` (Todo, InProgress, Done) |

### Positive Consequences

- The compiler enforces type safety — you cannot pass a `TaskTitle` where a `TaskId` is expected
- Validation happens on construction; invalid values cannot exist in the domain
- Mapping between value objects and DTOs happens in Application or Infrastructure, never in Domain

### Negative Consequences

- Slightly more boilerplate in value object definitions
- Any new domain concept with identity requires a new value object — never a raw primitive

## More Information

- Enforcement: Gate 1 (compiler) catches type mismatches at build time
- Related: [ADR-015](20260305-015-repo-structure.md) — value objects live in `EM2Devs.Todo.Domain`
