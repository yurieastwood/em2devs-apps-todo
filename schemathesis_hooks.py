"""Schemathesis hooks — disable stateful link inference.

The TOML config in v4.14.3 does not expose stateful.inference.algorithms,
but the Python API supports it. This hook disables automatic link inference
after the schema is loaded, so only explicit OpenAPI Links are followed
in stateful testing.

This prevents Schemathesis from reusing parent IDs as child IDs in
compound resource endpoints (quest->tasks, epic->quests).
"""

import schemathesis


@schemathesis.hook
def after_load_schema(context, schema):
    schema.config.phases.stateful.inference.algorithms = []
