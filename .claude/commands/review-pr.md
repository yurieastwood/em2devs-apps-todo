Review a pull request following the review process in AGENTS.md.

PR to review: $ARGUMENTS

1. Read the PR diff: `gh pr diff <number>`
2. Check that the PR follows the Definition of Done in AGENTS.md
3. Verify scenario tags are updated (`@done` before merge)
4. Verify conventional commit messages
5. Check for constraint violations (architecture, type system, testing rules)
6. Wait for GitHub Copilot review to complete if not yet done
7. For each concern found, leave a review comment with the specific issue and reference to the relevant ADR or constraint
8. If the PR looks good, approve it
9. Report the review outcome
