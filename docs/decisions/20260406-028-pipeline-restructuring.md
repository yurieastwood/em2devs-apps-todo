# Restructure CI Pipeline — Stage-Based Naming with Local-First Enforcement

- Status: Accepted
- Date: 2026-04-06

## Context and Problem Statement

The CI pipeline was originally designed as a linear G1-G7 gate model. As the project grew, checks were added organically: frontend lint (G2a), OpenAPI coverage check, Schemathesis hooks, and E2E tests — none of which fit the original numbering. G6 alone grew into three unrelated sub-phases (Spectral lint, coverage check, and Schemathesis dynamic testing) that all report as a single gate. The pipeline still works, but the naming no longer reflects the structure, making failures harder to diagnose and the system harder to extend.

How should the pipeline be restructured so that naming, grouping, and enforcement points are consistent, extensible, and aligned with industry practice?

## Decision Drivers

- Agentic development: agents push code autonomously and need synchronous, blocking feedback at the local level — moving checks to remote-only CI creates a gap where broken code can reach main
- Fast feedback: static checks (linting, format, spec validation) should run at commit time, not push time
- Clear failure attribution: when a check fails, the name should tell you what category of problem it is
- Extensibility: adding new checks should not require renumbering or reorganising existing ones
- Industry alignment: naming should be recognisable to engineers familiar with Continuous Delivery practices
- No logic changes: the pipeline should do the same work in the same enforcement points — this is a restructuring, not a redesign

## Considered Options

- Option A: Category Prefixes (e.g., `Quality:Lint`, `Contract:Static`)
- Option B: Feedback Speed Tiers (e.g., `Fast:Compile`, `Slow:Mutation`)
- Option C: Concern-Based Flat Names (e.g., `compile`, `format`, `contract-test`)
- Option D: CD Book Stage-Based (e.g., `Commit:Build`, `Acceptance:ContractTest`)
- Option E: Microsoft Three-Pipeline Model (PR pipeline, CI pipeline, CD pipeline)

## Decision Outcome

Chosen option: a hybrid of **Option D (CD Book Stage-Based)** and **Option C (Flat Names)**, with enforcement points kept local per agentic development requirements. Checks are grouped into Continuous Delivery stages (Commit, Acceptance) with descriptive flat names inside each group.

This hybrid takes the structural grouping from the CD book (stages represent increasing confidence) and the naming style from the GitHub Actions community and Microsoft's baseline architecture (descriptive, no numbering). It keeps all current checks in local git hooks (pre-commit, pre-push) rather than moving them to remote-only CI, which is critical for agentic development where agents must not be able to push broken code to main.

### Structural Changes

Two static checks — Spectral lint and OpenAPI coverage check — move from pre-push to pre-commit. They are fast, deterministic, require no running server, and catch the same class of issue as `dotnet format`. This gives earlier feedback (commit time instead of push time) and reduces pre-push duration.

G6 is broken into two separate checks: **Contract Lint** (static, pre-commit) and **Contract Test** (dynamic, pre-push). These are fundamentally different types of validation that were incorrectly bundled under one gate number.

The redundant `dotnet build` in the pre-push hook is removed. The pre-commit build already produces the generated OpenAPI JSON needed by the coverage check, and `dotnet run --no-build` is used for Schemathesis.

### Local Hooks — New Structure

**`commit-msg`** — unchanged.

**`pre-commit` — Commit Stage:**

| Check | What | Time |
|-------|------|------|
| Build | `dotnet build` (warnings-as-errors) | ~3s |
| Format | `dotnet format --verify-no-changes` | ~2s |
| Frontend Lint | `npm run check + lint + format:check` (if frontend files staged) | ~5s |
| Contract Lint | Spectral + coverage check (method,path pairs) | ~5s |
| Architecture | `dotnet test ArchitectureTests` | ~1s |
| Tests | `dotnet test` (unit, integration, smoke) | ~15s |
| Security | `dotnet list package --vulnerable` | ~3s |

**`pre-push` — Acceptance Stage:**

| Check | What | Time |
|-------|------|------|
| Branch Name | Conventional naming validation | ~0s |
| Contract Test | Start API, Schemathesis (coverage + stateful), stop API | ~35s |
| Mutation | Stryker.NET | ~4m |

### CI Workflow — New Structure

```
Hygiene|Branch Name          (PR only)
Hygiene|Commit Messages      (PR only)

Commit|Build
Commit|Format
Commit|Frontend Lint
Commit|Contract Lint         (depends: Commit|Build)
Commit|Architecture         (depends: Commit|Build)
Commit|Tests                (depends: Commit|Build)
Commit|Frontend Tests
Commit|Security             (depends: Commit|Build)

Acceptance|Contract Test     (depends: Commit|Build)
Acceptance|Mutation          (depends: Commit|Tests)
Acceptance|E2E               (depends: Commit|Build, Commit|Frontend Tests)
```

### Positive Consequences

- Failures are self-describing: "Commit|Contract Lint failed" tells you it's a spec issue caught at the static level, not a running-server problem
- Spectral and coverage check feedback moves 5 minutes earlier (commit vs push)
- G6 is no longer a monolithic gate — static and dynamic contract checks are independent
- Adding new checks is obvious: decide if it's Commit or Acceptance stage, give it a descriptive name
- Naming aligns with the CD book (Commit/Acceptance stages) which is the most widely recognised pipeline model in industry
- Local-first enforcement is preserved — agents cannot push broken code to main
- No logic changes — same checks, same enforcement points, better organisation

### Negative Consequences

- The `Stage|Name` prefix is slightly verbose in CI dashboard display names
- Developers familiar with the G1-G7 numbering need to learn the new names (one-time cost)
- ADR-025 and any documentation referencing "Gate 6" needs updating

### Neutral

- The G1-G7 numbering is retired completely — no gate numbers remain
- Pre-push hook duration decreases slightly (no redundant build, no Spectral/coverage)
- Pre-commit hook duration increases slightly (~5s for Spectral + coverage check)
- Total pipeline work is unchanged — same checks, same order of confidence

## Pros and Cons of the Options

### Option A: Category Prefixes

Group by concern: `Quality:Lint`, `Contract:Static`, `Security`.

- Good, because maps to developer mental model ("is this a contract problem or a test problem?")
- Good, because categories are extensible without renumbering
- Bad, because `Category:Sub` notation is not standard across CI platforms
- Bad, because no inherent ordering — categories don't express confidence progression

### Option B: Feedback Speed Tiers

Name by speed: `Fast:Compile`, `Mid:Tests`, `Slow:Mutation`.

- Good, because matches developer experience of the pipeline (fast results first)
- Good, because makes dependency ordering self-documenting
- Bad, because speed is relative and changes as the codebase grows — a "Fast" check today may be "Mid" tomorrow
- Bad, because tiers don't tell you what failed, only how long it took

### Option C: Concern-Based Flat Names

No hierarchy: `compile`, `format`, `contract-lint`, `mutation`.

- Good, because simplest option — no numbering, no hierarchy, each name is self-explanatory
- Good, because aligns with GitHub Actions community conventions
- Bad, because no grouping — CI dashboard is a flat list of 12+ jobs
- Bad, because loses the pedagogical value of staged confidence progression

### Option D: CD Book Stage-Based

Group by CD stage: `Commit:Build`, `Acceptance:ContractTest`, `Production:Smoke`.

- Good, because rooted in the most widely cited CD model (Humble & Farley)
- Good, because stages express increasing confidence — each level answers a different question
- Good, because aligns with MinimumCD's pipeline reference architecture for single-team deployables
- Good, because maps naturally to local hooks: pre-commit = Commit stage, pre-push = Acceptance stage
- Good, because a future Production stage fits without restructuring
- Bad, because the `Stage:Job` prefix is verbose
- Bad, because hygiene checks (branch name, commit messages) don't fit cleanly into either stage

### Option E: Microsoft Three-Pipeline Model

Separate pipelines: PR pipeline (fast), CI pipeline (integration), CD pipeline (deployment).

- Good, because aligns with Microsoft's Azure DevOps baseline architecture
- Good, because clear separation of purpose — each pipeline has a distinct trigger
- Good, because PR pipeline is fast (~40s), heavy checks run post-merge in CI
- Bad, because post-merge CI failures leave main in a broken state until fixed
- Bad, because fundamentally incompatible with agentic development where agents push autonomously — agents need synchronous blocking feedback before code reaches the remote, not asynchronous post-merge notifications
- Bad, because "stop the line" policy when main breaks requires cultural discipline that automated agents cannot exercise

## More Information

### Industry References

- **Continuous Delivery (Humble & Farley, 2010):** Defines the Commit Stage / Acceptance Stage / Production model. Stages represent increasing confidence. The Commit stage runs fast, deterministic checks; the Acceptance stage runs heavier integration and contract tests.
- **MinimumCD (minimumcd.org):** Codifies minimum CD activities. Provides a pipeline reference architecture for single-team deployables with stages ordered by defect detection priority. Draws a hard line: pre-merge checks must be deterministic; non-deterministic checks gate production, not merge. Our Schemathesis and Stryker checks are technically non-deterministic by this definition (they involve a running server), but we keep them pre-push because agentic development requires local-first enforcement.
- **Microsoft Azure Well-Architected Framework (OE:06):** Recommends quality gates throughout code promotion, layered pipelines reflecting different lifecycles, and the same artifacts across all environments. Uses descriptive flat names within PR/CI/CD pipelines rather than numbered gates.
- **Microsoft Azure DevOps Baseline Architecture:** Defines three pipelines (PR, CI, CD) with descriptive step names. The CI pipeline runs integration tests post-merge and publishes artifacts. We adopt the naming style but not the enforcement topology, keeping heavy checks local.
- **GitHub Actions community:** Uses descriptive flat names (`build`, `test`, `lint`, `e2e`). No numbering. We adopt this naming style within each stage group.

### What Changed vs. Prior ADRs

- **ADR-011 (CI/CD — GitHub Actions):** The pipeline platform remains GitHub Actions. The job structure changes from G1-G7 numbered gates to stage-grouped descriptive names. This ADR supersedes the pipeline structure section of ADR-011.
- **ADR-025 (OpenAPI Contract as Source of Truth):** References to "Gate 6" should be updated to "Commit|Contract Lint" (for Spectral) and "Acceptance|Contract Test" (for Schemathesis). The validation strategy is unchanged; only the naming and placement of static checks changes.
- **ADR-026 (Mutation Testing):** References to "G7" should be updated to "Acceptance|Mutation". The testing strategy is unchanged.

### Open Questions for Future Discussion

- **Determinism boundary:** MinimumCD says pre-merge checks must be deterministic. Our Schemathesis and Stryker checks are not purely deterministic (they involve running servers and real test execution). We keep them pre-push for agentic safety, but this is a conscious deviation from MinimumCD's guidance that should be revisited if flakiness becomes an issue.
- **Production stage:** The pipeline currently covers Commit and Acceptance stages. A Production stage (deploy, smoke, health checks, SLO monitoring) will be added when we have a deployment target. The stage-based naming model accommodates this without restructuring.
