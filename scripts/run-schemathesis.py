#!/usr/bin/env python3
"""Run Schemathesis with inference disabled for stateful testing.

The TOML config in v4.14.3 does not support stateful.inference.algorithms,
but the Python API does. This wrapper disables link inference so only
explicit OpenAPI Links are used — preventing Schemathesis from reusing
parent IDs as child IDs in compound resource endpoints.

Usage: python3 scripts/run-schemathesis.py <spec-path> <base-url>
"""

import sys

if len(sys.argv) != 3:
    print(f"Usage: {sys.argv[0]} <spec-path> <base-url>", file=sys.stderr)
    sys.exit(2)

import schemathesis
from schemathesis.config import StatefulPhaseConfig

spec_path = sys.argv[1]
base_url = sys.argv[2]

schema = schemathesis.from_path(spec_path, base_url=base_url)

# Disable inference — only explicit OpenAPI Links are followed
schema.config.phases.stateful = StatefulPhaseConfig(
    inference={"algorithms": []},
)

exit_code = 0
for event in schema.execute():
    if hasattr(event, "status") and event.status == "failure":
        exit_code = 1

sys.exit(exit_code)
