# Waypoint — Delivery Roadmap

Status tags: no tag = not started, `wip` = in progress, `done` = delivered and merged.

## Phase 1: PoC — "It works end-to-end"

A user can create tasks, complete them, earn XP, and see their level — backed by a real database, orchestrated by Aspire, tested with Testcontainers.

### 1.1 Aspire Orchestration — `done` (PR #26, #31)

- [x] AppHost project with PostgreSQL and Redis resources
- [x] ServiceDefaults project (OpenTelemetry, health checks, resilience)

### 1.2 Data Access — PostgreSQL + EF Core — `done` (PR #26, #30)

- [x] EF Core DbContext with TodoTask entity configuration
- [x] Initial PostgreSQL migration (tasks table)
- [x] Real TaskRepository: EF Core for writes
- [x] Integration tests with Testcontainers (PostgreSQL)
- [ ] Dapper for reads (deferred — EF Core handling both for now)

### 1.3 Application Layer — CQRS + Mediator — `done` (PR #25, #29)

- [x] Custom lightweight mediator (per ADR-010)
- [x] Commands: CreateTask, UpdateTaskStatus, DeleteTask
- [x] Queries: GetTask, ListTasks (with filtering)
- [x] Domain events infrastructure (publish on task completion)

### 1.4 API Refactor — Controller → Mediator — `done` (PR #25)

- [x] Refactor TasksController to dispatch commands/queries via mediator
- [x] Wire DI with real infrastructure (EF Core, PostgreSQL)
- [x] Conditional DI: PostgreSQL when connection string present, in-memory fallback for tests

### 1.5 Progression Integration — `done` (PR #28, #29)

- [x] XP award on task completion (domain event → handler)
- [x] Player profile aggregate (XP total, current level, streak)
- [x] API endpoint: GET /api/profile (XP, level, streak)
- [x] Update OpenAPI contract for profile endpoint (human approved)

### PoC Exit Criteria — `done`

- [x] `dotnet run --project src/EM2Devs.Todo.AppHost` starts the full stack
- [x] Task CRUD via API with PostgreSQL persistence
- [x] XP earned on task completion, level calculated, visible on profile
- [x] Data persists across restarts
- [x] All 7 gates pass
- [x] Integration tests with Testcontainers green

---

## Phase 2: MVP — "A user can use it daily"

Authentication, a basic frontend, and the core gamification loop (tasks → XP → levels → streaks) working as a real application.

### 2.1 Authentication — `wip` (PR #50, #65)

- [x] Demo auth stub with ICurrentUser abstraction (PR #50)
- [x] Route guards, conditional nav, login redirect (PR #65)
- [ ] Auth0 integration with social logins (per ADR-007) — deferred, demo stub in place
- [ ] JWT validation middleware — deferred
- [ ] User entity and repository — deferred
- [ ] Protected API endpoints (require authentication) — deferred (demo mode, no enforcement)

### 2.2 Frontend — SvelteKit Scaffold — `done` (PR #41)

- [x] SvelteKit project setup (per ADR-002) (PR #41)
- [x] Svelte 5 runes + stores + SvelteKit load functions (per ADR-013) (PR #41)
- [x] API client layer (PR #41)
- [ ] Auth0 login/logout flow — deferred (demo login in place)

### 2.3 Frontend — Task Management — `wip` (PR #47)

- [x] Task list screen with create (PR #47)
- [x] Task completion flow (PR #47)
- [x] Task deletion (PR #47)
- [ ] Task detail/edit screen — not started
- [ ] Filter, sort — not started
- [ ] XP animation on completion — not started

### 2.4 Frontend — Progression Dashboard — `done` (PR #51, #55)

- [x] XP bar and level display (PR #51)
- [x] Streak counter (PR #51)
- [x] XP breakdown display (PR #55)
- [ ] Achievement notifications — not started
- [ ] Streak history — not started

### 2.5 Caching

- [ ] Redis caching for read-heavy endpoints (per ADR-008)
- [ ] Cache invalidation on writes

### 2.6 Quest Management — `done` (PR #52)

- [x] Quest API endpoints (CRUD, add/remove tasks, progress) (PR #52)
- [x] Frontend: quest list and detail screens (PR #52)
- [x] Epic API endpoints and frontend (PR #52)

### 2.7 Background Jobs

- [ ] Quartz.NET setup (per ADR-019)
- [ ] Recurring task instance generation
- [ ] Streak evaluation (daily)
- [ ] Notification scheduling

### 2.8 Real-Time

- [ ] SignalR hub for notifications (per ADR-004)
- [ ] Frontend: real-time notification display

### 2.9 Error Handling + Security — `wip` (PR #37, #45, #46)

- [x] Result pattern + Problem Details RFC 9457 (per ADR-018) (PR #45)
- [x] API versioning /api/v1/ prefix (per ADR-017) (PR #46)
- [x] CORS (per ADR-021) (PR #37)
- [ ] Rate limiting — not started
- [ ] HTTPS enforcement — not started
- [ ] Security headers — not started

### 2.10 E2E Testing — `done` (PR #53)

- [x] Playwright test setup (per ADR-014) (PR #53)
- [x] E2E tests for task management flow (PR #53)
- [x] E2E tests for login/logout (PR #53)

### MVP Exit Criteria

- [ ] User can sign in with Google/GitHub via Auth0
- [x] Full task and quest management via the frontend
- [x] XP, level, and streak visible on progression dashboard
- [ ] Real-time notifications on achievements
- [x] PostgreSQL + Redis via Aspire
- [ ] Security baseline met (ADR-021)
- [x] All 7 gates + E2E tests pass

---

## Phase 3: Feature Complete — "Full gamification"

All BDD feature specs implemented across all layers.

### 3.1 Social

- [ ] Leaderboards: cohort-based ranking, types, privacy (leaderboards.feature)
- [ ] Shared quests: multi-contributor quests (shared-quests.feature)
- [ ] Challenge mode: time-limited competitions (challenge-mode.feature)
- [ ] Frontend for all social features

### 3.2 Intelligence

- [ ] API endpoints for energy scheduling, capacity modelling, time estimation, daily brief
- [ ] Procrastination detection domain model + API (procrastination-detection.feature)
- [ ] Frontend for intelligence features

### 3.3 Reflection

- [ ] Weekly review: guided review ritual (weekly-review.feature)
- [ ] Journey timeline: event timeline and filtering (journey-timeline.feature)
- [ ] Insight cards: personalised observations (insight-cards.feature)
- [ ] Annual wrapped: year-end summary (annual-wrapped.feature)
- [ ] Frontend for all reflection features

### 3.4 Onboarding

- [ ] Progressive disclosure: gradual feature reveal (progressive-disclosure.feature)
- [ ] Contextual tutorials
- [ ] Retroactive activation

### 3.5 Monetisation

- [ ] Subscription tiers: Free, Premium, Team (subscription-tiers.feature)
- [ ] Premium feature gating
- [ ] Cosmetic purchases

### 3.6 Data

- [ ] Local-first storage with sync (local-first-data.feature)
- [ ] Data export (JSON)
- [ ] Account deletion with holding period

### 3.7 Remaining Core

- [ ] Saga management (quest-hierarchy.feature)
- [ ] Hierarchy navigation (breadcrumbs)

### Feature Complete Exit Criteria

- [ ] All BDD scenarios tagged @done
- [ ] All feature areas have API endpoints, frontend screens, and tests
- [ ] All 7 gates + E2E tests pass across the full feature set
