"""Schemathesis hooks + auth provider.

Two responsibilities:

1. Disable automatic stateful link inference so only explicit OpenAPI Links
   are followed (prevents Schemathesis from reusing parent IDs as child IDs
   in compound endpoints like quest->tasks, epic->quests).

2. Authenticate all requests with a JWT obtained by calling
   ``POST /api/auth/login`` with the seeded demo credentials. Implemented via
   Schemathesis' ``@auth`` provider, which integrates with the framework's
   auth pipeline so the Coverage phase can still probe negative-auth behaviour
   (``ignored_auth`` check) while Stateful and Examples phases get a valid
   bearer token attached.
"""

from __future__ import annotations

import json
import os
import urllib.error
import urllib.request
from typing import Optional

import schemathesis

# Seeded dev credentials — see InMemoryUserRepository / AddUsersAndSeed migration.
DEMO_EMAIL = os.environ.get("SCHEMATHESIS_AUTH_EMAIL", "demo@waypoint.dev")
DEMO_PASSWORD = os.environ.get("SCHEMATHESIS_AUTH_PASSWORD", "demo1234")


def _login(base_url: str) -> Optional[str]:
    """POST seeded credentials to /api/auth/login and return the JWT."""
    data = json.dumps({"email": DEMO_EMAIL, "password": DEMO_PASSWORD}).encode("utf-8")
    req = urllib.request.Request(
        base_url.rstrip("/") + "/api/auth/login",
        data=data,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=10) as resp:  # noqa: S310 — local test target
            body = json.loads(resp.read().decode("utf-8"))
            return body.get("token")
    except (urllib.error.URLError, urllib.error.HTTPError, ValueError) as exc:
        print(f"[schemathesis_hooks] Login failed: {exc}")
        return None


@schemathesis.hook
def after_load_schema(context, schema):
    # Keep link inference disabled so only explicit OpenAPI Links drive stateful tests.
    schema.config.phases.stateful.inference.algorithms = []


@schemathesis.auth()
class DemoBearerAuth:
    """Fetch a JWT once per refresh interval and attach it as a bearer token.

    Schemathesis uses ``get`` to retrieve the credential and ``set`` to apply it
    to each generated case. Because this is registered via ``@auth``, the
    framework knows about our auth requirement and can still run its
    ``ignored_auth`` negative-auth probe correctly (it suppresses our provider
    for that specific check).
    """

    def get(self, case, context):
        # Resolve the base URL from the schema — Schemathesis sets this from --url.
        base_url = (
            getattr(getattr(case.operation, "schema", None), "base_url", None)
            or os.environ.get("SCHEMATHESIS_BASE_URL", "http://localhost:15001")
        )
        return _login(base_url)

    def set(self, case, data, context):
        if not data:
            return
        case.headers = case.headers or {}
        case.headers["Authorization"] = f"Bearer {data}"
