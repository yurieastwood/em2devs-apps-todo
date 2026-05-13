#!/usr/bin/env bash
set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

pass()    { echo -e "${GREEN}✓ $1${NC}"; }
fail()    { echo -e "${RED}✗ $1${NC}"; exit 1; }
gate()    { echo -e "\n${YELLOW}━━━ $1 ━━━${NC}"; }
require() { command -v "$1" &>/dev/null || fail "Required tool '$1' not found — $2"; }

require dotnet       "install .NET SDK from https://dot.net"
require npx          "install Node.js from https://nodejs.org"
require schemathesis "install via 'pip install schemathesis' or 'pipx install schemathesis'"
require curl         "install via your system package manager"

echo -e "${YELLOW}═══ Commit Stage ═══${NC}"

gate "Commit — Build"
dotnet build --configuration Release /p:TreatWarningsAsErrors=true --verbosity quiet && pass "Build succeeded" || fail "Build failed — check type errors (ADR-0002)"

gate "Commit — Format"
dotnet format --verify-no-changes && pass "Formatting clean" || fail "Format violations — run 'dotnet format' to fix"

gate "Commit — Contract Lint"
npx --yes @stoplight/spectral-cli lint docs/contracts/openapi.yaml --ruleset .spectral.yaml \
  && pass "Spec valid (Spectral)" \
  || fail "Spec violation — see ADR-0004"
bash scripts/check-openapi-coverage.sh docs/contracts/openapi.yaml src/EM2Devs.Todo.Api/EM2Devs.Todo.Api.json \
  && pass "All operations documented (coverage check)" \
  || fail "Undocumented API operations — update docs/contracts/openapi.yaml"

gate "Commit — Architecture"
dotnet test tests/EM2Devs.Todo.ArchitectureTests --configuration Release --verbosity quiet && pass "Architecture rules hold" || fail "Architecture violation — see ADR-0001"

gate "Commit — Tests"
dotnet test --configuration Release --filter "Category!=Architecture" --verbosity quiet && pass "All scenarios pass" || fail "Test failure — fix production code, not the test (ADR-0003)"

gate "Commit — Security"
dotnet list package --vulnerable --include-transitive 2>&1 | grep -q "has the following vulnerable packages" && fail "Vulnerable NuGet packages found — update or replace them" || pass "No known vulnerabilities"

echo -e "${YELLOW}═══ Acceptance Stage ═══${NC}"

gate "Acceptance — Contract Test"
API_PORT=15001
dotnet run --project src/EM2Devs.Todo.Api --configuration Release --no-build \
  --urls "http://localhost:${API_PORT}" &>/dev/null &
API_PID=$!
trap "kill ${API_PID} 2>/dev/null; wait ${API_PID} 2>/dev/null" EXIT
API_READY=false
for i in $(seq 1 30); do
  curl -s "http://localhost:${API_PORT}/api/tasks" &>/dev/null && { API_READY=true; break; } || sleep 1
done
[[ "${API_READY}" == "true" ]] || fail "API did not start within 30 seconds — check port ${API_PORT}"
# --generation-allow-x00=false: prevent NULL bytes in generated strings.
# Kestrel rejects NULL bytes in request paths at the HTTP parser layer (HTTP 400, empty body)
# before ASP.NET middleware runs, so UseStatusCodePages/AddProblemDetails cannot wrap them
# in application/problem+json — they violate the contract through no fault of the application.
#
# --exclude-checks use_after_free,ensure_resource_availability: Schemathesis's stateful
# resource-lifecycle checks assume a single long-lived auth identity across the entire run.
# Our schemathesis_hooks.py mints a fresh ephemeral user per case (necessary because
# /api/account/delete permanently deactivates its caller). Resources created/deleted in one
# case are owned by a different user than the operations in another, so the tracker's
# "use after free" and "ensure resource availability" alarms fire on cross-case operations
# that legitimately return 4xx due to per-user scoping. The signal these checks would
# provide is already covered by integration tests with deterministic auth state.
schemathesis run docs/contracts/openapi.yaml --url "http://localhost:${API_PORT}" --checks all --exclude-checks use_after_free,ensure_resource_availability --phases examples,coverage,stateful --generation-allow-x00=false \
  && pass "Implementation matches contract (Schemathesis)" \
  || fail "Contract drift — implementation doesn't match spec (ADR-0004)"
kill "${API_PID}" 2>/dev/null; wait "${API_PID}" 2>/dev/null
trap - EXIT

gate "Acceptance — Mutation"
dotnet tool restore --verbosity quiet || fail "Tool restore failed — check .config/dotnet-tools.json"
dotnet stryker -f stryker-config.json && pass "All mutants killed" || fail "Mutant survived — add tests to kill it (ADR-0005)"

echo -e "\n${GREEN}━━━ ALL STAGES PASSED ━━━${NC}\n"
