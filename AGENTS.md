# AGENTS.md — Agent Workflow Orchestration

This file defines how you (the AI agent) operate in this codebase. Follow this workflow exactly.

## Project Context

This is a .NET 9 Clean Architecture Waypoint Todo API. The domain is intentionally simple — the focus is on pipeline guardrails, not business complexity.

Tech stack: .NET 9, C# 13, xUnit, Shouldly, NetArchTest, Spectral, Stryker.NET, GitHub Actions.

## Workflow: Before Writing Any Code

1. **Read the relevant ADR** in `docs/decisions/` for the area you're working in.
2. **Read the OpenAPI contract** in `docs/contracts/openapi.yaml` if touching the API surface.
3. **Run the existing tests** to establish a baseline: `dotnet test`

## Workflow: Implementing a Change

Follow this sequence for every change. Do not skip steps.

### Step 1: Understand the Scenario

Before writing code, identify the behavior being added or changed. Express it as: "Given [context], when [action], then [outcome]."

### Step 2: Write or Update the Test First

Write a failing test that encodes the scenario. Run `dotnet test` and confirm it **fails** (red phase). If the test passes already, it's not testing the new behavior — rewrite it.

Do NOT write the production code yet.

### Step 3: Implement the Minimum Code

Write only the production code needed to make the failing test pass. Run `dotnet test` and confirm it **passes** (green phase).

### Step 4: Run All Gates

After the test passes, run the full local gate sequence:

```bash
dotnet build /p:TreatWarningsAsErrors=true             # G1: Compiler + Roslyn analyzers as errors
dotnet format --verify-no-changes                      # G2: Format
dotnet test --filter "Category=Architecture"           # G3: Architecture fitness
dotnet test                                            # G4: All tests (including scenarios)
dotnet list package --vulnerable --include-transitive  # G5: Supply chain security
npx --yes @stoplight/spectral-cli lint docs/contracts/openapi.yaml --ruleset .spectral.yaml  # G6: Static — spec is well-formed
schemathesis run docs/contracts/openapi.yaml --url http://localhost:5001 --checks all   # G6: Dynamic — implementation matches spec
dotnet stryker -f stryker-config.json                    # G7: Mutation testing
```

### Step 5: Fix Any Gate Failures

If any gate fails:
- Read the error message carefully — it will reference a specific file and rule.
- Fix the production code, NOT the test and NOT the gate.
- Re-run the failing gate until it passes.
- Then re-run ALL gates to confirm no regressions.

### Step 6: Commit

Git hooks enforce the gates automatically — pre-commit runs G1–G5, pre-push runs G6–G7. If a hook fails, fix the issue and retry. Do NOT bypass hooks with `--no-verify`.

Write a conventional commit message:

```
feat(domain): add task status transition validation

Closes #12
```

## Constraints — Do NOT Violate These

### Architecture (see ADR-0001)
- Domain MUST NOT reference Application, Infrastructure, or Api.
- Application MUST NOT reference Infrastructure or Api.
- Infrastructure MUST NOT reference Api.
- Only Infrastructure implements interfaces defined in Application.

### Type System (see ADR-0002)
- Never use primitive `string` or `Guid` for domain identifiers — use the value objects in `EM2Devs.Todo.Domain.ValueObjects`.
- Never use `string` for task titles — use `TaskTitle`.
- Never use `string` for task status — use the `TaskStatus` enum.

### Testing (see ADR-0003)
- Never delete a test to make the suite pass. Fix the production code.
- Never modify a test assertion to be weaker. If a test is wrong, explain why and get human approval.
- Tests must be scenario-driven: test behaviors, not methods.
- Test names follow: `Should_[ExpectedBehavior]_When_[Condition]`

### API Contract (see ADR-0004)
- The OpenAPI spec in `docs/contracts/openapi.yaml` is the source of truth.
- Do not change the contract without explicit human approval.
- All API responses must match the contract schemas exactly.

### Mutation Testing (see ADR-0005)
- Zero surviving mutants allowed. Any survivor fails the pipeline.
- If mutants survive, add tests to kill them — do not lower the threshold.
- Only the Domain layer is mutated. Run `dotnet stryker -f stryker-config.json` to check locally.

### CI/CD Pipeline
- Do NOT modify CI pipeline files (`.github/workflows/`), git hooks (`scripts/hooks/`), or gate scripts (`scripts/run-gates.sh`) without explicit human approval.
- Do NOT change gate thresholds, disable gates, or alter the pipeline structure.
- If a gate blocks your work, fix the production code — not the pipeline.

## Error Reference

When a gate fails, the error will include context. Here's how to interpret them:

| Error pattern | Gate | What to do |
|---|---|---|
| `CS0029: Cannot implicitly convert type` | G1 | You're mixing value objects. Check ADR-0002. |
| `Whitespace / formatting differs` | G2 | Run `dotnet format` to auto-fix, then re-check. |
| `Types in Domain should not depend on Infrastructure` | G3 | You added a wrong dependency. Check ADR-0001. |
| `Test failed: Should_...` | G4 | Fix the production code, not the test. Check ADR-0003. |
| `Warning treated as error` | G1 | Address the analyzer warning. Do not suppress it. |
| `has the following vulnerable packages` | G5 | A NuGet dependency has a known CVE. Update or replace it. |
| `OpenAPI violation` (Spectral) | G6 | The spec document is malformed or violates a rule. Fix the spec. Check ADR-0004. |
| `Contract drift` (Schemathesis) | G6 | The running implementation doesn't match the spec. Fix the controller/DTO. Check ADR-0004. |
| `Mutant survived` / `Mutation score is below threshold` | G7 | A mutant survived — add a test that catches the mutation. Check ADR-0005. |
