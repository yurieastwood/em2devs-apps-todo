# Authentication — Auth0 with Social Logins

- Status: Accepted
- Date: 2026-03-05

## Context and Problem Statement

EM2Devs.Todo is a gamified productivity app where user motivation is the core product. Friction at the signup gate directly undermines that motivation — a user who abandons registration never earns their first XP. The app must allow any new user to be authenticated and playing within seconds, using an account they already have, with no password to create or forget. At the same time, the backend needs a stable local user identity to attach todos, XP totals, achievements, and quest progress to. How should authentication and user identity be handled to satisfy both zero-friction onboarding and reliable backend-side user records?

## Decision Drivers

- Frictionless onboarding: users must be able to sign in with one click using an existing social account — no registration form, no password creation
- Social provider coverage: Apple, Google, Microsoft, and Meta are required at launch; additional providers should be addable without code changes to the backend
- Password-less by default: storing and managing passwords introduces credential risk and operational overhead that is not justified for this app
- MFA support: Auth0's built-in MFA satisfies security requirements without custom implementation
- Free tier generosity: the solution must be cost-free through early adoption (target: 25 000 MAU threshold)
- Developer experience: social OAuth2 flows are complex to implement correctly from scratch; a managed service removes that risk
- Local user record: the backend must maintain its own user entity (internal ID, display name, email, avatar URL, linked provider ID) that domain logic can reference — the identity provider is not the source of truth for application data

## Considered Options

- Auth0 (managed identity provider with social login support)
- Keycloak (self-hosted open-source identity provider)
- ASP.NET Identity + manual OAuth2 (self-managed social login flows)
- Entra ID / Azure AD (Microsoft managed identity platform)

## Decision Outcome

Chosen option: "Auth0 with social logins", because it delivers all required social providers out of the box, is free for the first 25 000 MAU, and requires roughly five minutes of configuration to wire up Apple, Google, Microsoft, and Meta — work that would take weeks to replicate safely in a self-managed solution. The approach preserves a clean separation of concerns: Auth0 handles credential verification and token issuance, while the backend maintains its own user record for all application domain data.

The sign-in flow is:

1. User taps "Sign in with Google" in the Flutter or SvelteKit client.
2. The client redirects to the Auth0-hosted login page, which proxies to Google's OAuth2 consent screen.
3. Google authenticates the user and returns an ID token (email, name, avatar URL) to Auth0.
4. Auth0 issues a token to the client, which forwards it to the EM2Devs.Todo backend.
5. The backend validates the token against Auth0's JWKS endpoint.
6. The backend checks whether a local `User` record already exists for the provider + provider user ID combination.
   - If no record exists: one is auto-created (internal UUID, display name from the social profile, email, avatar URL, linked provider ID). No form, no password, no manual step.
   - If a record exists: the user is logged in.
7. The backend issues its own short-lived JWT ([ADR-004](20260305-004-api-style.md)) for all subsequent API calls. Auth0 is not consulted again until the session expires.

The local `User` entity is essential — it is the root aggregate that todos, XP, achievements, and quest progress attach to. Auth0 owns the credential; the backend owns the application identity.

### Positive Consequences

- Any new user is authenticated and inside the app in under ten seconds, with no form to fill
- All required social providers (Apple, Google, Microsoft, Meta) are available through Auth0's connection library without writing OAuth2 flows
- Adding a new social provider (e.g., GitHub, LinkedIn) requires one Auth0 configuration change and no backend code changes
- Built-in MFA satisfies the security baseline ([ADR-021](20260305-021-security-baseline.md)) without custom implementation
- The backend JWT layer means downstream services never call Auth0 on the hot path — Auth0 is only touched at login
- Auth0 free tier (25 000 MAU) covers the expected user base through early production

### Negative Consequences

- Auth0 is a third-party dependency; an outage or pricing change affects the login flow (mitigated by the backend's own JWT layer, which keeps authenticated sessions alive without re-hitting Auth0)
- If the Auth0 free tier is exceeded, costs scale per MAU — the team must plan for this threshold as a growth milestone
- Account linking across providers (e.g., a user who signs in with Google and later wants to use Apple) requires explicit handling; Auth0 supports it but the backend must also reconcile local records

### Neutral

- The backend JWT can be issued with any claims needed for authorisation (user ID, roles, feature flags), keeping the API layer independent of Auth0's token format
- Social profile data (avatar URL, display name) is captured at first login and stored locally; it is not re-fetched from the provider on every request
- Auth0's Universal Login page is hosted and managed by Auth0, which means it can be styled to match the app brand but is subject to Auth0's rendering constraints

## Pros and Cons of the Options

### Auth0 (managed identity provider)

Auth0 is a cloud-hosted identity platform that manages the full OAuth2/OIDC flow for social providers. The developer configures connections (Google, Apple, etc.) in the Auth0 dashboard; Auth0 handles the redirect dance, token exchange, and user storage. The backend validates Auth0-issued tokens and syncs a local user record.

- Good, because all major social providers are pre-built connections requiring only a dashboard toggle
- Good, because 25 000 MAU are free — sufficient for launch and early growth without billing concern
- Good, because MFA, anomaly detection, and bot protection are available without additional code
- Good, because JWKS-based token validation is straightforward to implement in ASP.NET Core with `Microsoft.AspNetCore.Authentication.JwtBearer`
- Good, because the hosted login UI removes the need to build and maintain a login screen
- Bad, because it introduces a vendor dependency for the login path; Auth0 outages affect sign-in (not authenticated sessions)
- Bad, because exceeding the free tier introduces per-MAU costs that must be budgeted as the app grows

### Keycloak (self-hosted)

Keycloak is an open-source identity provider that can be hosted on the team's own infrastructure. It supports social login adapters and issues standard OIDC tokens.

- Good, because self-hosted — no vendor dependency, no per-MAU costs at scale
- Good, because open-source with a large community and extensive documentation
- Bad, because the team must provision, operate, and secure a Keycloak instance (or cluster), adding significant DevOps overhead at project inception
- Bad, because social login adapters require more configuration than Auth0's built-in connections
- Bad, because the operational burden is not justified when Auth0 is free within the target MAU range

### ASP.NET Identity + Manual OAuth2

Implement user management with ASP.NET Core Identity and manually implement OAuth2 authorization code flows for each social provider using a library such as `AspNet.Security.OAuth.Providers`.

- Good, because full ownership of the identity stack — no third-party dependency
- Good, because maximum flexibility in user data modelling and token format
- Bad, because implementing OAuth2 flows correctly (PKCE, state parameter, token refresh, error handling) is complex and error-prone
- Bad, because each social provider must be individually configured and tested; adding a new provider requires code changes
- Bad, because password management, credential storage, and breach response become team responsibilities
- Bad, because development time spent on auth plumbing is time not spent on gamification features

### Entra ID / Azure AD

Microsoft's managed identity platform, offering social login via B2C or External Identities and tight integration with the Microsoft ecosystem.

- Good, because first-class support for Microsoft (and GitHub, via enterprise directory) accounts
- Good, because deeply integrated with Azure services if Azure is the deployment target
- Bad, because configuring non-Microsoft social providers (Apple, Meta) in Entra is significantly more complex than in Auth0
- Bad, because Entra ID B2C pricing and setup complexity is higher than Auth0 for the initial use case
- Bad, because the mental model (tenants, policies, user flows) adds overhead compared to Auth0's simpler configuration surface

## More Information

- [Auth0 social connections documentation](https://auth0.com/docs/authenticate/identity-providers/social-identity-providers)
- [Auth0 pricing (free tier: 25 000 MAU)](https://auth0.com/pricing)
- [Microsoft.AspNetCore.Authentication.JwtBearer (.NET 9)](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [Sign In with Apple requirements for apps](https://developer.apple.com/sign-in-with-apple/get-started/)
- Related: [ADR-004](20260305-004-api-style.md) — JWT used for backend API authentication after the Auth0 login flow
- Related: [ADR-021](20260305-021-security-baseline.md) — Security baseline (CORS, rate limiting, HTTPS, security headers)
