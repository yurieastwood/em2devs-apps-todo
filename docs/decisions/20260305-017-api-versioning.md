# API Versioning — URL Path Versioning with Asp.Versioning

- Status: Accepted
- Date: 2026-03-05

## Context and Problem Statement

The EM2Devs.Todo REST API will evolve as the product grows. Breaking changes — removed fields, renamed endpoints, changed response shapes — are inevitable. Without a versioning strategy established from day one, any breaking change forces all consumers to update simultaneously or accept downtime. How should the REST API expose versions to clients in a way that is explicit, easy to route, and compatible with the Minimal APIs pattern chosen in [ADR-004](20260305-004-api-style.md)?

## Decision Drivers

- Explicitness: the version a client is using must be unambiguous from the request alone
- Ease of understanding: developers and testers should be able to reason about which version they are calling without reading documentation first
- Routing simplicity: the versioning mechanism must work cleanly with .NET 9 Minimal API route groups
- Tooling support: the mechanism should integrate with the OpenAPI generation pipeline so each version gets its own documented spec
- Browser and curl testability: testers must be able to target a specific version without custom headers or query string manipulation

## Considered Options

- URL path versioning (`/api/v1/todos`, `/api/v2/todos`)
- Header versioning (`Api-Version: 1.0` request header)
- Query string versioning (`/api/todos?api-version=1.0`)

## Decision Outcome

Chosen option: "URL path versioning with `Asp.Versioning`", because it is the most explicit and universally understood versioning mechanism, works directly with route-based dispatch in Minimal APIs, is trivially testable in any HTTP client, and the `Asp.Versioning` NuGet package provides first-class Minimal API support with automatic OpenAPI integration.

The concrete application of this decision:

- All routes are prefixed `/api/v{version}/`, starting with `/api/v1/`
- The `Asp.Versioning.Http` package is registered at startup; versioning middleware and conventions are configured once
- Each Minimal API route group declares the version(s) it supports via `WithApiVersionSet`
- The OpenAPI pipeline generates a separate document per declared version, so Scalar serves `/openapi/v1.json` and `/openapi/v2.json` independently
- **Version introduction policy**: only create v2 routes when a genuine breaking change occurs. Non-breaking additions (new optional fields, new endpoints) are made to the existing version. This keeps the version surface minimal.
- Infrastructure is wired from day one at zero functional cost; the discipline of never introducing breaking changes without a new version is established as a team norm from the start.

### Positive Consequences

- Version is visible in every log line, trace, and browser network panel — no ambiguity when debugging
- Route groups per version make it straightforward to deprecate and eventually remove old versions by removing the group registration
- `Asp.Versioning` generates version-aware OpenAPI documents automatically; no manual spec maintenance
- Clients (SvelteKit frontend, mobile apps, third-party integrations) can pin to a version and upgrade on their own schedule
- Consistent with the versioning convention developers encounter most frequently across public APIs

### Negative Consequences

- URL duplication: when a v2 version of an endpoint is introduced, both the v1 and v2 handlers must be maintained until v1 is sunset
- Puristic REST theory argues that the URL should identify a resource, not a version; in practice this is a widely accepted pragmatic tradeoff

### Neutral

- Starting at v1 means no immediate migration cost; the infrastructure cost of adding versioning upfront is a one-time setup of a few lines of startup code
- Deprecated versions can be marked with `[ApiVersion("1.0", Deprecated = true)]`; Scalar surfaces this visually, giving consumers advance notice before removal

## Pros and Cons of the Options

### URL Path Versioning (`/api/v1/todos`)

The version is encoded directly in the URL path. Industry convention for most publicly documented REST APIs (Stripe, GitHub, Twilio, etc.).

- Good, because it is the most universally understood versioning convention across the industry
- Good, because the version is visible in every log line, browser address bar, curl command, and network trace without any extra tooling
- Good, because route-based dispatch in .NET Minimal APIs maps directly to path segments — no middleware inspection of headers or query strings required
- Good, because different versions can be deployed to different route groups or even different services if the architecture ever warrants it
- Good, because `Asp.Versioning` has first-class support for Minimal APIs and generates per-version OpenAPI documents with minimal configuration
- Bad, because the URL is technically no longer a pure resource identifier; version is mixed with the resource path
- Bad, because each breaking version requires maintaining parallel route handlers until the old version is retired

### Header Versioning (`Api-Version: 1.0`)

The version is communicated via a custom or standardised request header.

- Good, because the URL remains a clean resource identifier, satisfying strict REST constraints
- Good, because a single URL can serve multiple versions without URL duplication
- Bad, because the version is hidden — not visible in browser address bar, logs, or curl output without explicit inspection
- Bad, because testing in a browser or simple HTTP client requires setting a custom header, which adds friction
- Bad, because proxy layers and CDNs may not cache or route header-versioned requests correctly without custom configuration
- Bad, because less discoverable; a developer new to the API does not know which header to set or what values are valid without reading documentation

### Query String Versioning (`/api/todos?api-version=1.0`)

The version is appended as a query parameter.

- Good, because the URL path remains clean and the version is at least visible in the request URL
- Good, because it requires no custom headers and is testable from any browser
- Bad, because mixing versioning concerns into the query string pollutes the parameter space used for filtering and pagination
- Bad, because query parameters are semantically optional in HTTP; a missing `api-version` parameter requires a default version policy that can surprise callers
- Bad, because it looks non-standard and inconsistent with the conventions most developers encounter in production APIs
- Bad, because caching by CDNs and reverse proxies treats query-string variants as separate cache entries, which can cause cache fragmentation

## More Information

- [Asp.Versioning for Minimal APIs](https://github.com/dotnet/aspnet-api-versioning/wiki/New-Services-Quick-Start#aspnet-core-with-minimal-apis)
- [Microsoft API versioning guidance](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design#versioning-a-restful-web-api)
- [Asp.Versioning OpenAPI integration](https://github.com/dotnet/aspnet-api-versioning/wiki/API-Documentation#openapi)
- Related: [ADR-004](20260305-004-api-style.md) — REST Minimal APIs as the primary API pattern
