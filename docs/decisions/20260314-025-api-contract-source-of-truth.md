# OpenAPI Contract as Source of Truth

- Status: Accepted
- Date: 2026-03-14

## Context and Problem Statement

AI agents working on API endpoints tend to drift from the intended contract — adding fields, changing status codes, or altering response shapes without realising they have broken downstream consumers. In a services context, this drift causes integration failures that surface late. How can the API contract be enforced as the authoritative definition of the API surface?

## Decision Drivers

- The contract must be written (or approved) by a human before implementation begins
- Agents must implement controllers and DTOs to match the contract exactly
- Drift between contract and implementation must be detected automatically

## Considered Options

- OpenAPI spec as source of truth with static + dynamic validation (Gate 6)
- Code-first OpenAPI generation from controllers
- Contract tests using consumer-driven contracts (Pact)

## Decision Outcome

Chosen option: "OpenAPI spec as source of truth with two-phase validation", because it establishes a human-approved contract that agents must conform to, with both static and dynamic enforcement.

The OpenAPI specification lives at `docs/contracts/openapi.yaml` and is the source of truth for the API surface.

### Validation (Gate 6)

**Phase 1 — Static (Spectral):** Lints the OpenAPI spec document for structural correctness, naming conventions, and completeness.

**Phase 2 — Dynamic (Schemathesis):** Starts the running API and property-tests every operation against the spec, verifying that real HTTP responses conform to the declared schemas, status codes, and content types.

Phase 1 catches spec document problems. Phase 2 catches implementation drift.

### Positive Consequences

- Adding a new endpoint requires updating the contract first, then implementing
- Response DTOs must match contract schemas exactly
- Both spec quality and implementation conformance are validated automatically

### Negative Consequences

- Breaking changes to the contract require a new ADR documenting the migration path
- The contract must not be modified by the agent without explicit human approval — this adds friction, but the friction is intentional

## More Information

- Enforcement: Gate 6 in the CI pipeline and pre-push hook
- Related: [ADR-017](20260305-017-api-versioning.md) — URL path versioning for the API
- Related: [ADR-004](20260305-004-api-style.md) — REST Minimal APIs as the API pattern
