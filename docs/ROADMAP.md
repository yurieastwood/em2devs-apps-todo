# Waypoint — Delivery Roadmap

Status tags: no tag = not started, `wip` = in progress, `done` = delivered and merged.

## Phase 1: PoC — "It works end-to-end"

A user can create tasks, complete them, earn XP, and see their level — backed by a real database, orchestrated by Aspire, tested with Testcontainers.

### 1.1 Aspire Orchestration

- [ ] AppHost project with PostgreSQL and Redis resources
- [ ] ServiceDefaults project (OpenTelemetry, health checks, resilience)

### 1.2 Data Access — PostgreSQL + EF Core

- [ ] EF Core DbContext with TodoTask entity configuration
- [ ] Initial PostgreSQL migration (tasks table)
- [ ] Real TaskRepository: EF Core for writes, Dapper for reads
- [ ] Integration tests with Testcontainers (PostgreSQL)

### 1.3 Application Layer — CQRS + Mediator

- [ ] Custom lightweight mediator (per ADR-010)
- [ ] Commands: CreateTask, UpdateTaskStatus, DeleteTask
- [ ] Queries: GetTask, ListTasks (with filtering)
- [ ] Domain events infrastructure (publish on task completion)

### 1.4 API Refactor — Controller → Mediator

- [ ] Refactor TasksController to dispatch commands/queries via mediator
- [ ] Wire DI with real infrastructure (EF Core, Dapper, PostgreSQL)
- [ ] Update OpenAPI contract for any endpoint changes (requires human approval)

### 1.5 Progression Integration

- [ ] XP award on task completion (domain event → handler)
- [ ] Player profile aggregate (XP total, current level, streak)
- [ ] API endpoint: GET /api/profile (XP, level, streak)
- [ ] Update OpenAPI contract for profile endpoint (requires human approval)

### PoC Exit Criteria

- [ ] `dotnet run --project src/EM2Devs.Todo.AppHost` starts the full stack
- [ ] Task CRUD via API with PostgreSQL persistence
- [ ] XP earned on task completion, level calculated, visible on profile
- [ ] Data persists across restarts
- [ ] All 7 gates pass
- [ ] Integration tests with Testcontainers green

---

## Phase 2: MVP — "A user can use it daily"

Authentication, a basic frontend, and the core gamification loop (tasks → XP → levels → streaks) working as a real application.

### 2.1 Authentication

- [ ] Auth0 integration with social logins (per ADR-007)
- [ ] JWT validation middleware
- [ ] User entity and repository
- [ ] Protected API endpoints (require authentication)

### 2.2 Frontend — SvelteKit Scaffold

- [ ] SvelteKit project setup (per ADR-002)
- [ ] Svelte 5 runes + stores + SvelteKit load functions (per ADR-013)
- [ ] Auth0 login/logout flow
- [ ] API client layer

### 2.3 Frontend — Task Management

- [ ] Task list screen (create, filter, sort)
- [ ] Task detail/edit screen
- [ ] Task completion flow with XP animation
- [ ] Task deletion with confirmation

### 2.4 Frontend — Progression Dashboard

- [ ] XP bar and level display
- [ ] Streak counter and history
- [ ] Achievement notifications

### 2.5 Caching

- [ ] Redis caching for read-heavy endpoints (per ADR-008)
- [ ] Cache invalidation on writes

### 2.6 Quest Management

- [ ] Quest API endpoints (CRUD, add/remove tasks, progress)
- [ ] Frontend: quest list and detail screens
- [ ] Epic API endpoints and frontend

### 2.7 Background Jobs

- [ ] Quartz.NET setup (per ADR-019)
- [ ] Recurring task instance generation
- [ ] Streak evaluation (daily)
- [ ] Notification scheduling

### 2.8 Real-Time

- [ ] SignalR hub for notifications (per ADR-004)
- [ ] Frontend: real-time notification display

### 2.9 Error Handling + Security

- [ ] Result pattern + Problem Details RFC 9457 (per ADR-018)
- [ ] API versioning /api/v1/ prefix (per ADR-017)
- [ ] CORS, rate limiting, HTTPS, security headers (per ADR-021)

### 2.10 E2E Testing

- [ ] Playwright test setup (per ADR-014)
- [ ] E2E tests for task management flow
- [ ] E2E tests for login/logout

### MVP Exit Criteria

- [ ] User can sign in with Google/GitHub via Auth0
- [ ] Full task and quest management via the frontend
- [ ] XP, level, and streak visible on progression dashboard
- [ ] Real-time notifications on achievements
- [ ] PostgreSQL + Redis via Aspire
- [ ] Security baseline met (ADR-021)
- [ ] All 7 gates + E2E tests pass

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
