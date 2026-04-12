Run the full quality gate sequence and report results.

1. Ensure hooks are configured: `git config core.hooksPath scripts/hooks`
2. Run `./scripts/run-gates.sh`
3. If any gate fails, report which gate failed, the exact error, and what needs to be fixed
4. If all gates pass, confirm readiness for commit/push
