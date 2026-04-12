Create a pull request for the current branch following the workflow in AGENTS.md.

1. Verify all gates pass (run `./scripts/run-gates.sh` if not already run)
2. Ensure any `@wip` scenarios touched in this branch are tagged `@done`
3. Verify the branch name follows conventional branch patterns: `feat/`, `fix/`, `chore/`, `hotfix/`, `release/`
4. Commit any remaining changes with a conventional commit message
5. Push the branch
6. Create the PR: `gh pr create` with a clear title and description referencing the scenario(s) implemented
7. Report the PR URL
