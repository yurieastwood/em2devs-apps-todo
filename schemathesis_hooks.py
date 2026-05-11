"""Schemathesis hooks + auth provider.

Two responsibilities:

1. Disable automatic stateful link inference so only explicit OpenAPI Links
   are followed (prevents Schemathesis from reusing parent IDs as child IDs
   in compound endpoints like quest->tasks, epic->quests).

2. Authenticate each request with a JWT obtained by registering a *fresh*
   ephemeral user. We can't reuse a single seeded account because the
   ``POST /api/account/delete`` endpoint permanently deactivates its caller —
   once Schemathesis exercises that operation mid-run, any subsequent case
   using the same account would fail authentication and cascade into
   "Missing authentication" warnings across many operations.

   By registering a fresh user per credential request, each case is isolated:
   if a case happens to delete its account, only that ephemeral user is
   destroyed and the next case gets a new one.
"""

from __future__ import annotations

import itertools
import json
import os
import urllib.error
import urllib.request
import uuid
from typing import Optional

import schemathesis

_RUN_ID = uuid.uuid4().hex[:8]
_COUNTER = itertools.count(1)
_PASSWORD = "Schemathesis-12345"


def _register_ephemeral(base_url: str) -> Optional[str]:
    """Register a fresh ephemeral user and return their JWT.

    Each call uses a unique email so registration always succeeds even when
    a prior case left a previously-registered user in the deactivated state.
    """
    idx = next(_COUNTER)
    email = f"schemathesis-{_RUN_ID}-{idx}@example.test"
    data = json.dumps({
        "email": email,
        "password": _PASSWORD,
        "displayName": f"Schemathesis User {idx}",
    }).encode("utf-8")
    req = urllib.request.Request(
        base_url.rstrip("/") + "/api/auth/register",
        data=data,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=10) as resp:  # noqa: S310 — local test target
            body = json.loads(resp.read().decode("utf-8"))
            return body.get("token")
    except (urllib.error.URLError, urllib.error.HTTPError, ValueError) as exc:
        print(f"[schemathesis_hooks] Register failed: {exc}")
        return None


@schemathesis.hook
def after_load_schema(context, schema):
    # Keep link inference disabled so only explicit OpenAPI Links drive stateful tests.
    schema.config.phases.stateful.inference.algorithms = []


@schemathesis.auth()
class EphemeralBearerAuth:
    """Mint a JWT for a fresh user on each credential request.

    Schemathesis caches credentials between cases by default; this provider
    refreshes per case so a case that destroys its account cannot affect
    subsequent cases.
    """

    def get(self, case, context):
        base_url = (
            getattr(getattr(case.operation, "schema", None), "base_url", None)
            or os.environ.get("SCHEMATHESIS_BASE_URL", "http://localhost:15001")
        )
        return _register_ephemeral(base_url)

    def set(self, case, data, context):
        if not data:
            return
        case.headers = case.headers or {}
        case.headers["Authorization"] = f"Bearer {data}"
