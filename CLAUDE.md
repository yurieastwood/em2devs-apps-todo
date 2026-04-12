# CLAUDE.md — Claude Code Instructions

Read [`AGENTS.md`](AGENTS.md) before writing any code. It defines the full workflow, constraints, and error reference.

Read [`README.md`](README.md) for the tech stack, project structure, prerequisites, and quality pipeline.

## Quick Reference

### Setup (first time)

```bash
git config core.hooksPath scripts/hooks
dotnet tool restore
dotnet restore
cd src/EM2Devs.Todo.Web && npm install && cd -
```

### Run the App

```bash
dotnet run --project src/EM2Devs.Todo.AppHost
```

API runs on port **5001**. Aspire dashboard URL is printed in the console.

### Quality Gates

Run all gates locally:

```bash
./scripts/run-gates.sh
```

Individual checks:

| Command | What it does |
|---------|-------------|
| `dotnet build --configuration Release /p:TreatWarningsAsErrors=true` | Build with analyzers |
| `dotnet format --verify-no-changes` | Check formatting |
| `dotnet format` | Auto-fix formatting |
| `dotnet test --configuration Release` | Run all backend tests |
| `dotnet test tests/EM2Devs.Todo.ArchitectureTests` | Architecture boundary tests |
| `dotnet stryker` | Mutation testing (Domain layer) |
| `npx @stoplight/spectral-cli lint docs/contracts/openapi.yaml --ruleset .spectral.yaml` | Contract lint |
| `cd src/EM2Devs.Todo.Web && npm run check` | Svelte type check |
| `cd src/EM2Devs.Todo.Web && npm run lint` | Frontend lint |
| `cd src/EM2Devs.Todo.Web && npm run format` | Auto-fix frontend formatting |
| `cd src/EM2Devs.Todo.Web && npm run format:check` | Check frontend formatting |
| `cd src/EM2Devs.Todo.Web && npm run test` | Frontend unit tests |

### Branching & Merging

- Branch names: `feat/`, `fix/`, `chore/`, `hotfix/`, `release/`
- Commit format: `type(scope): description` — enforced by `commit-msg` hook
- Valid types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `perf`, `ci`, `build`, `revert`
- **Merge to main (offline)**: `git checkout main && git merge <branch> --no-ff`
  - The `pre-merge-commit` hook runs both Commit Stage AND Acceptance Stage
  - Merge is **blocked** if any gate fails — fix on the feature branch and retry
- **Merge to main (online)**: Create a PR via `gh pr create`, then `gh pr merge --squash`

### Key Directories

| Path | Purpose |
|------|---------|
| `docs/decisions/` | Architecture Decision Records — read before working in any area |
| `docs/features/` | BDD scenarios in Gherkin — the task backlog (`@todo` = available) |
| `docs/contracts/openapi.yaml` | OpenAPI spec — source of truth, **human approval required** to change |
| `scripts/hooks/` | Git hooks (pre-commit, pre-push, commit-msg) |
| `scripts/run-gates.sh` | Master script to run all quality gates |

### Hard Rules

- Never bypass or modify git hooks, gate scripts, or CI pipeline without human approval
- Never use primitive types for domain identifiers — use value objects (ADR-023)
- Never delete or weaken a test — fix the production code (ADR-024)
- Zero surviving mutants on Domain layer (ADR-026)
- OpenAPI contract changes require human approval (ADR-025)
- The agent that authored a PR must never review it
