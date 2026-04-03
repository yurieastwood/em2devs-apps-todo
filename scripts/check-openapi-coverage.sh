#!/usr/bin/env bash
# Compares paths in the generated OpenAPI spec (from code) against the
# hand-written contract to detect undocumented endpoints.
#
# Usage: bash scripts/check-openapi-coverage.sh <hand-written.yaml> <generated.json>
set -euo pipefail

HAND_WRITTEN="${1:?Usage: $0 <hand-written.yaml> <generated.json>}"
GENERATED="${2:?Usage: $0 <hand-written.yaml> <generated.json>}"

RED='\033[0;31m'
GREEN='\033[0;32m'
NC='\033[0m'

[[ -f "${HAND_WRITTEN}" ]] || { echo -e "${RED}Hand-written spec not found: ${HAND_WRITTEN}${NC}"; exit 1; }
[[ -f "${GENERATED}" ]]    || { echo -e "${RED}Generated spec not found: ${GENERATED}${NC}"; exit 1; }

# Extract paths from generated JSON spec, excluding versioned duplicates (/api/v{version}/...)
generated_paths=$(python3 -c "
import json, sys
with open('${GENERATED}') as f:
    spec = json.load(f)
for path in sorted(spec.get('paths', {}).keys()):
    if '/v{version}/' not in path:
        print(path)
")

# Extract paths from hand-written YAML spec (grep lines matching /api/ pattern)
handwritten_paths=$(grep -E '^\s{2}/api/' "${HAND_WRITTEN}" | sed 's/://;s/^[[:space:]]*//' | sort -u)

missing=()
while IFS= read -r path; do
    [[ -z "${path}" ]] && continue
    if ! echo "${handwritten_paths}" | grep -qxF "${path}"; then
        missing+=("${path}")
    fi
done <<< "${generated_paths}"

total=$(echo "${generated_paths}" | grep -c '.' || true)

if [[ ${#missing[@]} -eq 0 ]]; then
    echo -e "${GREEN}All ${total} API endpoints are documented in the contract.${NC}"
    exit 0
else
    echo -e "${RED}${#missing[@]} API endpoint(s) missing from ${HAND_WRITTEN}:${NC}"
    for path in "${missing[@]}"; do
        echo -e "  ${RED}- ${path}${NC}"
    done
    echo ""
    echo "Add the missing paths to ${HAND_WRITTEN} to fix this check."
    exit 1
fi
