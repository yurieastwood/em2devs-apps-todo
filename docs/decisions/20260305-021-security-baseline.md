# Security Baseline — CORS, Rate Limiting, HTTPS, Input Validation, Security Headers

- Status: Accepted
- Date: 2026-03-05

## Context and Problem Statement

Authentication ([ADR-007](20260305-007-authentication.md)) establishes who a user is. But authentication alone does not protect the API against a wide class of common attacks: cross-origin resource abuse, brute-force credential stuffing, insecure transport, malformed or malicious input, and missing HTTP security headers that harden the browser-side attack surface. These controls are standard practice for any web-facing API, and retrofitting them after an incident is more costly and error-prone than building them in from day one. What security baseline should the EM2Devs.Todo backend implement from the start, and which .NET 9 built-in capabilities should be used to enforce it?

## Decision Drivers

- OWASP compliance: the baseline should address the most common web API vulnerabilities without requiring heavy third-party frameworks
- .NET 9 built-in capabilities: prefer middleware and libraries that ship with or are directly supported by .NET 9, minimising third-party dependency surface
- Defense in depth: no single control is sufficient; layering multiple controls reduces the blast radius of any individual bypass
- Minimal operational overhead: controls should be configurable per environment (development, staging, production) without requiring separate infrastructure
- Input validation consistency: validation logic should live in one place in the CQRS pipeline, not scattered across endpoint handlers
- CI enforcement: security controls should be verifiable in automated checks ([ADR-011](20260305-011-ci-cd.md)) so regressions are caught before they reach production

## Considered Options

- Comprehensive baseline (CORS + rate limiting + HTTPS + input validation + security headers)
- Minimal baseline (HTTPS + CORS only — defer other controls)
- Defer all security hardening to the deployment/infrastructure layer

## Decision Outcome

Chosen option: "Comprehensive baseline", because security controls are significantly cheaper to build in at inception than to retrofit after a vulnerability is discovered or exploited. All five control areas use either .NET 9 built-in middleware or well-established, actively maintained open-source libraries — none require bespoke implementation.

The five control areas and their implementation:

### 1. CORS Policy

Restrict cross-origin requests to the SvelteKit frontend origin only. Configured via `builder.Services.AddCors()` and `app.UseCors()` with a named policy.

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(builder.Configuration["Cors:AllowedOrigin"]!)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});
```

The allowed origin is environment-specific (`http://localhost:5173` in development, the production SvelteKit URL in staging/production). Wildcard origins (`*`) are never used in production.

### 2. Rate Limiting

Use the .NET 9 built-in `RateLimiter` middleware (`System.Threading.RateLimiting`). Apply stricter limits to sensitive endpoints:

- **Auth-related endpoints** (token exchange, user creation): fixed-window policy, 10 requests per minute per IP.
- **General API endpoints**: sliding-window policy, 100 requests per minute per authenticated user ID.
- **Leaderboard endpoints** (read-only, cacheable): more permissive or bypassed when Redis cache is warm.

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", cfg =>
    {
        cfg.Window = TimeSpan.FromMinutes(1);
        cfg.PermitLimit = 10;
    });
    options.AddSlidingWindowLimiter("api", cfg =>
    {
        cfg.Window = TimeSpan.FromMinutes(1);
        cfg.SegmentsPerWindow = 6;
        cfg.PermitLimit = 100;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
```

### 3. HTTPS Enforcement

Redirect all HTTP requests to HTTPS and add HSTS headers for browser-side enforcement.

```csharp
app.UseHttpsRedirection();
app.UseHsts(); // only in production; Aspire handles certs in development
```

HTTPS is required for Auth0 redirect callbacks and for ensuring JWTs are never transmitted over plain HTTP. In development, Aspire and `dotnet dev-certs` handle the local certificate.

### 4. Input Validation (FluentValidation in CQRS Pipeline)

All write commands pass through a `ValidationBehavior<TRequest, TResponse>` registered in the MediatR pipeline. `FluentValidation` validators are discovered automatically from the assembly containing the command.

```csharp
// ValidationBehavior (registered once in DI)
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TRequest, TResponse> next, CancellationToken ct)
    {
        var failures = validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

Validation errors are caught by a global exception handler that maps `ValidationException` to HTTP 422 with a structured problem details body. Endpoint handlers contain no validation logic.

### 5. Security Headers

Add HTTP security headers to every response. Implemented via `NetEscapades.AspNetCore.SecurityHeaders` (a lightweight, actively maintained package) or via a custom middleware if the package is deemed unnecessary overhead.

Headers applied:

| Header | Value |
|--------|-------|
| `Content-Security-Policy` | `default-src 'self'` (tightened per environment) |
| `X-Content-Type-Options` | `nosniff` |
| `X-Frame-Options` | `DENY` |
| `Referrer-Policy` | `strict-origin-when-cross-origin` |
| `Permissions-Policy` | `camera=(), microphone=(), geolocation=()` |

The API is consumed by a native Flutter app and a SvelteKit frontend; the Flutter client ignores browser-style headers, but the SvelteKit frontend and any future web client benefit from them.

### Positive Consequences

- CORS policy prevents arbitrary web pages from making authenticated requests to the API using a victim's browser session
- Rate limiting limits the damage of credential stuffing, brute-force, and scraping attempts without requiring a WAF
- HTTPS enforcement ensures tokens, session data, and user content are never transmitted in plaintext
- FluentValidation in the CQRS pipeline provides a single, consistent place to define input rules; validators are unit-testable independently of HTTP concerns
- Security headers harden the browser attack surface for SvelteKit frontend users at minimal cost
- All controls are configurable per environment — development is not unnecessarily locked down

### Negative Consequences

- Rate limiting with in-memory counters does not work correctly across multiple API instances; a Redis-backed rate limit store is required for consistent enforcement in multi-instance deployments (see [ADR-008](20260305-008-caching.md))
- FluentValidation is a third-party dependency; .NET 9's own data annotations validation is an alternative, but FluentValidation's composable, testable rule sets are significantly more maintainable for complex command objects
- Security header tuning (especially CSP) requires iteration as the frontend evolves; an overly strict CSP can break legitimate frontend behaviour

### Neutral

- The `ValidationBehavior` applies to all MediatR requests; pure query handlers that do not mutate state can be excluded via a marker interface or convention if the overhead is measurable
- HSTS headers should not be applied in development or staging environments where the HTTPS certificate may be self-signed or short-lived
- The security baseline does not include an API gateway or WAF; those are infrastructure-layer concerns that can be added independently without changing application code

## Pros and Cons of the Options

### Comprehensive Baseline (Recommended)

Implement all five control areas (CORS, rate limiting, HTTPS, input validation, security headers) from the project's first production-targeted sprint.

- Good, because security is built into the application from inception — no "we'll add it later" risk
- Good, because all five areas use .NET 9 built-in capabilities or well-established libraries; no bespoke security code is written
- Good, because defense in depth: an attacker who bypasses one control still faces the others
- Good, because FluentValidation in the CQRS pipeline is a pattern the team already adopts for domain consistency ([ADR-010](20260305-010-cqrs-mediator.md)); adding validators is low-friction
- Bad, because requires upfront configuration time for each control area
- Bad, because in-memory rate limiting counters require a Redis-backed store for multi-instance accuracy (additional coordination with [ADR-008](20260305-008-caching.md))

### Minimal Baseline (HTTPS + CORS Only)

Apply only HTTPS redirection and a CORS policy; defer rate limiting, input validation at the pipeline level, and security headers.

- Good, because faster to implement — fewer moving parts in the initial sprint
- Good, because the two highest-impact controls (HTTPS and CORS) are still in place
- Bad, because leaves the API open to brute-force and scraping attacks until rate limiting is added
- Bad, because input validation scattered across handlers becomes inconsistent and harder to test
- Bad, because security headers are low-cost to add and protect real users of the SvelteKit frontend
- Bad, because "defer" rarely means "later" — these controls tend to remain unimplemented until an incident forces the issue

### Defer to Deployment / Infrastructure Layer

Rely on the hosting environment (reverse proxy, API gateway, cloud WAF) to enforce security controls rather than implementing them in the application.

- Good, because application code stays minimal — no security middleware in the pipeline
- Good, because infrastructure-level controls (WAF rules, gateway rate limiting) can be updated without redeploying the application
- Bad, because the application is unprotected in any environment that does not have the infrastructure controls in place (local dev, staging, internal previews)
- Bad, because input validation cannot be delegated to infrastructure — the application must validate its own inputs
- Bad, because CQRS pipeline validation is an application architecture concern, not an infrastructure concern
- Bad, because deferring creates a gap between development behaviour (no security middleware) and production behaviour (all security middleware), making bugs harder to reproduce

## More Information

- [ASP.NET Core rate limiting middleware (.NET 7+)](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [ASP.NET Core CORS documentation](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
- [HSTS in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl)
- [FluentValidation documentation](https://docs.fluentvalidation.net/en/latest/)
- [NetEscapades.AspNetCore.SecurityHeaders](https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders)
- [OWASP REST Security Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/REST_Security_Cheat_Sheet.html)
- Related: [ADR-007](20260305-007-authentication.md) — Auth0 authentication; HTTPS required for Auth0 redirect callbacks
- Related: [ADR-008](20260305-008-caching.md) — Redis provides the backplane needed for distributed rate limiting counters
- Related: [ADR-010](20260305-010-cqrs-mediator.md) — CQRS mediator pipeline where `ValidationBehavior` is registered
- Related: [ADR-011](20260305-011-ci-cd.md) — CI pipeline that enforces security baseline checks
