#!/usr/bin/env bash
# Compares (method, path) operations in the generated OpenAPI spec (from code)
# against the hand-written contract to detect undocumented endpoints.
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

# Resolve Python binary
if command -v python3 &>/dev/null; then
    PY=python3
elif command -v python &>/dev/null; then
    PY=python
else
    echo -e "${RED}Python is required but neither 'python3' nor 'python' was found in PATH.${NC}"
    exit 1
fi

# Extract METHOD /path from generated JSON spec, excluding versioned duplicates
generated_ops=$("${PY}" -c "
import json, sys

HTTP_METHODS = {'get','post','put','patch','delete','options','head','trace'}

with open(sys.argv[1]) as f:
    spec = json.load(f)

for path, item in sorted(spec.get('paths', {}).items()):
    if '/v{version}/' in path:
        continue
    if not isinstance(item, dict):
        continue
    for method in sorted(item):
        if method.lower() in HTTP_METHODS:
            print(f'{method.upper()} {path}')
" "${GENERATED}")

# Extract METHOD /path from hand-written YAML spec
handwritten_ops=$("${PY}" -c "
import sys, yaml

with open(sys.argv[1]) as f:
    spec = yaml.safe_load(f) or {}

HTTP_METHODS = {'get','post','put','patch','delete','options','head','trace'}

for path, item in sorted((spec.get('paths') or {}).items()):
    if not isinstance(item, dict):
        continue
    for method in sorted(item):
        if method.lower() in HTTP_METHODS:
            print(f'{method.upper()} {path}')
" "${HAND_WRITTEN}")

missing=()
while IFS= read -r op; do
    [[ -z "${op}" ]] && continue
    if ! echo "${handwritten_ops}" | grep -qxF "${op}"; then
        missing+=("${op}")
    fi
done <<< "${generated_ops}"

total=$(echo "${generated_ops}" | grep -c '.' || true)

if [[ ${#missing[@]} -eq 0 ]]; then
    echo -e "${GREEN}All ${total} API operations are documented in the contract.${NC}"
    exit 0
else
    echo -e "${RED}${#missing[@]} API operation(s) missing from ${HAND_WRITTEN}:${NC}"
    for op in "${missing[@]}"; do
        echo -e "  ${RED}- ${op}${NC}"
    done
    echo ""
    echo "Add the missing operations to ${HAND_WRITTEN} to fix this check."
    exit 1
fi
