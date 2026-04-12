Implement the next available BDD scenario following the workflow in AGENTS.md.

1. Look for scenarios tagged `@todo` in `docs/features/`. Pick the first one unless a specific scenario is given: $ARGUMENTS
2. Read the relevant ADR(s) referenced in the feature file or related to the area
3. Tag the scenario `@wip` in the feature file
4. Create a branch: `feat/<short-description>` from latest `main`
5. Write a failing test that encodes the scenario. Run `dotnet test` and confirm it FAILS
6. Implement the minimum code to make the test pass. Confirm it PASSES
7. Run `./scripts/run-gates.sh` and fix any failures
8. Tag the scenario `@done` in the feature file
9. Commit with a conventional commit message
10. Push and create a PR: `gh pr create`
11. Report what was done and any decisions made
