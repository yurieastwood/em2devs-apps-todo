# AGENTS.md — Agent Operating Manual

This file defines how AI agents operate in this codebase. Follow it exactly.

## Project Context

Waypoint is a gamified todo app built with .NET 9 (Clean Architecture, CQRS) and SvelteKit / Svelte 5. See [`README.md`](README.md) for the full tech stack, project structure, and prerequisites.

All architectural decisions are in [`docs/decisions/`](docs/decisions/). Read the relevant ADR before working in any area.

## Agent Coordination

Agents receive tasks from a coordinator — either a human or another agent. The model is:

1. **Receive a task** from the coordinator, or pull one from a defined backlog when instructed
2. **Set your status** so the coordinator and other agents know what you are working on
3. **Do the work** following this file
4. **Report back** when the task is complete, blocked, or needs a decision (e.g., OpenAPI contract changes require human approval)
5. **Ask for the next task** — do not sit idle

Any number of agents may work in parallel. The only hard rule: **the agent that authored a PR must never be the one that reviews it.**

## Definition of Ready

A task is ready to be picked up when:

- The relevant ADR in `docs/decisions/` has been read
- The relevant BDD scenario(s) in `docs/features/` have been identified
- The branch is created from the latest `main`
- The OpenAPI contract has been reviewed if the task touches the API surface

## Definition of Done

A task is done when ALL of the following are true:

- All local checks pass (pre-commit Commit Stage + pre-push Acceptance Stage)
- Scenario tags updated (`@wip` on start, `@done` before PR)
- PR created to `main` with a conventional commit message
- PR reviewed by a different agent or human — never the author
- GitHub Copilot review comments replied to and conversations resolved
- CI pipeline green
- Merged to `main` via squash merge (`gh pr merge --squash`)
- Coordinator informed of completion

## Workflow: Implementing a Change

### Step 1: Understand the Scenario

Identify the behavior being added or changed. Express it as: "Given [context], when [action], then [outcome]." Tag the scenario `@wip` in the feature file.

### Step 2: Write the Test First

Write a failing test that encodes the scenario. Run `dotnet test` (backend) or `npm run test` (frontend) and confirm it **fails**. If the test passes already, it's not testing the new behavior — rewrite it.

Do NOT write the production code yet.

### Step 3: Implement the Minimum Code

Write only the code needed to make the failing test pass. Confirm it **passes**.

### Step 4: Run All Gates

After the test passes, the full gate sequence runs automatically via git hooks:

- **Pre-commit** runs the Commit Stage (Build, Format, Frontend Lint, Contract Lint, Architecture, Tests, Security)
- **Pre-push** runs the Acceptance Stage (Contract Test, Mutation)

See the [Quality Pipeline in README.md](README.md#quality-pipeline) for what each check validates. Frontend Lint runs conditionally — only when files in `src/EM2Devs.Todo.Web/` are staged.

### Step 5: Fix Any Gate Failures

If any gate fails:

- Read the error message — it references a specific file and rule
- Fix the **production code**, not the test and not the gate
- Re-run until all gates pass

See the [Error Reference](#error-reference) at the bottom of this file.

### Step 6: Create the PR

1. Tag the scenario(s) `@done` in the feature file
2. Commit with a conventional commit message (see [ADR-016](docs/decisions/20260305-016-code-quality.md))
3. Push — pre-push hook runs Acceptance Stage
4. Create the PR: `gh pr create`
5. Request review from another agent or human

### Step 7: Review Process

1. Reviewer reads the diff (`gh pr diff <N>`)
2. Wait for GitHub Copilot review to complete (~2-3 minutes)
3. Reviewer evaluates each Copilot comment — valid concerns go back to the author, noise is dismissed with reasoning
4. Author addresses feedback, replies to each comment, and marks conversations as resolved
5. Once CI green and review complete: `gh pr merge --squash`
6. Inform the coordinator

## Scenario Tag Lifecycle

BDD scenarios in `docs/features/` use status tags to track implementation progress:

| Tag | Meaning |
|-----|---------|
| `@todo` | Not started — no implementation exists |
| `@wip` | Work in progress — an agent is actively implementing |
| `@done` | Implemented and verified in a PR that is ready to merge (remains true once merged to `main`) |

Tag changes must be included in the PR. For the full tag taxonomy (category tags like `@core`, `@xp`, etc.), see [`docs/features/README.md`](docs/features/README.md).

## PR Workflow

Branch protection is enabled on `main`. All changes go through pull requests.

- Branch naming follows conventional branch patterns defined in [ADR-016](docs/decisions/20260305-016-code-quality.md): `feat/`, `fix/`, `chore/`, `hotfix/`, `release/`
- Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/) — enforced by the `commit-msg` hook and CI
- Both are validated in pre-push hooks and CI

## OpenAPI Contract Changes

The OpenAPI spec in `docs/contracts/openapi.yaml` is the source of truth (see [ADR-025](docs/decisions/20260314-025-api-contract-source-of-truth.md)).

Changing it requires **explicit human approval**:

1. Identify the changes needed
2. Report the proposed changes to the coordinator with a clear summary
3. **Wait for human approval** before modifying the file
4. Once approved, update the contract and verify Contract Lint and Contract Test pass

## Constraints — Do NOT Violate These

### Architecture ([ADR-022](docs/decisions/20260314-022-clean-architecture-enforcement.md))

- Domain MUST NOT reference Application, Infrastructure, or Api
- Application MUST NOT reference Infrastructure or Api
- Infrastructure MUST NOT reference Api
- Only Infrastructure implements interfaces defined in Application

### Type System ([ADR-023](docs/decisions/20260314-023-strongly-typed-domain-ids.md))

- Never use primitive `string` or `Guid` for domain identifiers — use value objects
- Never use `string` for task titles — use `TaskTitle`
- Never use `string` for task status — use the `TaskStatus` enum

### Testing ([ADR-024](docs/decisions/20260314-024-scenario-driven-testing.md))

- Never delete or weaken a test — fix the production code
- Tests must be scenario-driven: test behaviors, not methods
- Test names: `Should_[ExpectedBehaviour]_When_[Condition]`

### Mutation Testing ([ADR-026](docs/decisions/20260314-026-mutation-testing.md))

- Zero surviving mutants allowed
- If mutants survive, add tests to kill them — do not lower the threshold

### CI/CD Pipeline

- Do NOT modify CI pipeline files, git hooks, or gate scripts without explicit human approval
- Do NOT change gate thresholds, disable gates, or alter the pipeline structure
- If a gate blocks your work, fix the production code — not the pipeline

## Error Reference

| Error pattern | Check | What to do |
|---|---|---|
| `CS0029: Cannot implicitly convert type` | Commit\|Build | Mixing value objects. Check [ADR-023](docs/decisions/20260314-023-strongly-typed-domain-ids.md). |
| `Warning treated as error` | Commit\|Build | Address the analyzer warning. Do not suppress it. |
| `Whitespace / formatting differs` | Commit\|Format | Run `dotnet format` to auto-fix. |
| `Frontend format violations` | Commit\|Frontend Lint | Run `npm run format` in `src/EM2Devs.Todo.Web/`. |
| `Frontend lint violations` | Commit\|Frontend Lint | Run `npm run lint` in `src/EM2Devs.Todo.Web/`. |
| `Svelte type errors` | Commit\|Frontend Lint | Run `npm run check` in `src/EM2Devs.Todo.Web/`. |
| `OpenAPI violation` (Spectral) | Commit\|Contract Lint | Spec is malformed. Fix the spec. Check [ADR-025](docs/decisions/20260314-025-api-contract-source-of-truth.md). |
| `Undocumented API operations` | Commit\|Contract Lint | Add the missing operations to `docs/contracts/openapi.yaml`. |
| `Types in Domain should not depend on Infrastructure` | Commit\|Architecture | Wrong dependency. Check [ADR-022](docs/decisions/20260314-022-clean-architecture-enforcement.md). |
| `Test failed: Should_...` | Commit\|Tests | Fix the production code, not the test. Check [ADR-024](docs/decisions/20260314-024-scenario-driven-testing.md). |
| `has the following vulnerable packages` | Commit\|Security | A NuGet dependency has a known CVE. Update or replace it. |
| `Contract drift` (Schemathesis) | Acceptance\|Contract Test | Implementation doesn't match spec. Fix the controller/DTO. Check [ADR-025](docs/decisions/20260314-025-api-contract-source-of-truth.md). |
| `Mutant survived` | Acceptance\|Mutation | Add a test that catches the mutation. Check [ADR-026](docs/decisions/20260314-026-mutation-testing.md). |
