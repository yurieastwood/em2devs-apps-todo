# OpenAPI Contract Changes Proposal

**Status**: Awaiting human approval (ADR-025)
**Scope**: API surface for all 323 now-`@done` BDD scenarios
**Current contract**: `docs/contracts/openapi.yaml` (35 endpoints)
**Proposed**: +92 new endpoints across 11 feature categories

The domain layer is complete and verified (100% mutation score, 2,638 domain tests). These endpoints are the HTTP surface that would expose that domain to clients. Each endpoint maps to domain methods that already exist and are tested.

---

## Core — Task Management Extensions (9 endpoints)

| Method | Path | Operation | Maps to |
|--------|------|-----------|---------|
| GET | `/api/tasks?view=inbox` | listTasksInInbox | `TaskViewFilter.Inbox` |
| GET | `/api/tasks?view=today` | listTasksForToday | `TaskViewFilter.Today` |
| GET | `/api/tasks?view=upcoming` | listTasksUpcoming | `TaskViewFilter.Upcoming` |
| GET | `/api/tasks?view=completed` | listCompletedTasks | `TaskViewFilter.Completed` |
| GET | `/api/tasks?tag={tag}` | filterTasksByTag | `TodoTask.Tags` |
| GET | `/api/tasks?search={keyword}` | searchTasks | `TodoTask.MatchesKeyword` |
| POST | `/api/tasks:quick-add` | quickAddTask | `QuickAddParser.Parse` |
| POST | `/api/tasks/{taskId}/tags` | addTagToTask | `TodoTask.AddTag` |
| POST | `/api/tasks/{taskId}/actual-time` | recordActualTime | `TodoTask.RecordActualTime` |

## Core — Sagas & Hierarchy (5 endpoints)

| Method | Path | Operation |
|--------|------|-----------|
| POST | `/api/sagas` | createSaga (requires Pro tier) |
| GET | `/api/sagas/{sagaId}` | getSaga |
| GET | `/api/sagas/{sagaId}/timeline` | getSagaTimeline |
| POST | `/api/sagas/{sagaId}/epics` | assignEpicToSaga |
| GET | `/api/tasks/{taskId}/hierarchy` | getTaskHierarchyPath |

## Core — Quest Chains (5 endpoints)

| Method | Path | Operation |
|--------|------|-----------|
| GET | `/api/quest-chains/suggestions` | getSuggestedPatterns |
| POST | `/api/quest-chains` | createFromTemplate |
| GET | `/api/quest-chains/{chainId}` | getQuestChain |
| GET | `/api/quest-chains/{chainId}/history` | getChainHistory |
| POST | `/api/quest-chains/{chainId}/adapt` | applyAdaptation |

## Core — Notifications (7 endpoints)

| Method | Path | Operation |
|--------|------|-----------|
| GET | `/api/notifications` | listNotifications |
| POST | `/api/notifications/{id}/read` | markNotificationRead |
| POST | `/api/notifications/{id}/dismiss` | dismissNotification |
| GET | `/api/notifications/settings` | getNotificationSettings |
| PUT | `/api/notifications/settings` | updateNotificationSettings |
| POST | `/api/notifications/push/register` | registerPushDevice |
| DELETE | `/api/notifications/push/register` | unregisterPushDevice |

## Progression — XP, Levels, Titles, Skill Trees, Seasons (13 endpoints)

| Method | Path | Operation |
|--------|------|-----------|
| GET | `/api/profile/xp-history` | getXpHistory |
| GET | `/api/profile/skill-trees` | listSkillTrees |
| GET | `/api/profile/skill-trees/{type}` | getSkillTreeDetail |
| GET | `/api/profile/titles` | listTitles (earned + progress) |
| PUT | `/api/profile/titles/active` | setActiveTitle |
| POST | `/api/profile/streak/freeze` | activateStreakFreeze |
| DELETE | `/api/profile/streak/freeze` | unfreezeStreak |
| GET | `/api/seasons/current` | getCurrentSeason |
| GET | `/api/seasons/history` | getSeasonHistory |
| GET | `/api/seasons/current/quest-line` | getSeasonalQuestLine |
| GET | `/api/seasons/current/leaderboard` | getSeasonalLeaderboard |
| GET | `/api/seasons/current/cosmetics` | listSeasonalCosmetics |
| GET | `/api/profile/cosmetics` | listEarnedCosmetics |

## Intelligence — Energy, Capacity, Estimation, Daily Brief, Procrastination (14 endpoints)

| Method | Path | Operation |
|--------|------|-----------|
| POST | `/api/energy/check-in` | recordEnergyLevel |
| GET | `/api/energy/current` | getCurrentEnergy |
| GET | `/api/energy/profile` | getEnergyProfile |
| GET | `/api/capacity/model` | getCapacityModel |
| GET | `/api/capacity/overview` | getWeeklyCapacityOverview |
| GET | `/api/capacity/insights` | getCapacityInsights |
| POST | `/api/capacity/reprioritise` | applyReprioritisation |
| POST | `/api/tasks/{taskId}/timer:start` | startTaskTimer |
| POST | `/api/tasks/{taskId}/timer:stop` | stopTaskTimer |
| GET | `/api/estimation/dashboard` | getEstimationDashboard |
| GET | `/api/estimation/suggestion` | getCorrectedEstimate |
| GET | `/api/daily-brief` | getDailyBrief |
| POST | `/api/daily-brief/accept` | acceptBrief |
| POST | `/api/daily-brief/modify` | modifyBrief |
| POST | `/api/daily-brief/dismiss` | dismissBrief |
| GET | `/api/procrastination/candidates` | listProcrastinationCandidates |
| GET | `/api/procrastination/insights` | getProcrastinationPatterns (Pro) |
| POST | `/api/tasks/{taskId}/intervention/{type}` | applyIntervention |

## Social — Guilds, Partners, Leaderboards, Shared Quests, Challenges (22 endpoints)

### Guilds (10)

| Method | Path | Operation |
|--------|------|-----------|
| POST | `/api/guilds` | createGuild |
| GET | `/api/guilds` | listUserGuilds |
| GET | `/api/guilds/{guildId}` | getGuild |
| PUT | `/api/guilds/{guildId}` | updateGuildDetails |
| POST | `/api/guilds/{guildId}/invite` | generateGuildInvite |
| POST | `/api/guilds/{guildId}/join` | acceptGuildInvite |
| DELETE | `/api/guilds/{guildId}/members/{userId}` | removeGuildMember |
| POST | `/api/guilds/{guildId}/leave` | leaveGuild |
| POST | `/api/guilds/{guildId}/disband` | disbandGuild |
| POST | `/api/guilds/{guildId}/quests` | createGuildQuest |
| GET | `/api/guilds/{guildId}/quests` | listGuildQuests |
| GET | `/api/guilds/{guildId}/feed` | getGuildFeed |
| GET | `/api/guilds/{guildId}/profile` | getGuildProfile (XP, level) |

### Accountability Partners (5)

| Method | Path | Operation |
|--------|------|-----------|
| POST | `/api/partners/request` | sendPartnerRequest (requires level 7) |
| POST | `/api/partners/{id}/accept` | acceptPartnerRequest |
| POST | `/api/partners/{id}/decline` | declinePartnerRequest |
| DELETE | `/api/partners/{id}` | endPartnership |
| GET | `/api/partners/{id}/summary` | getPartnerDailySummary |
| POST | `/api/partners/{id}/messages` | sendCheckInMessage |

### Leaderboards (3)

| Method | Path | Operation |
|--------|------|-----------|
| GET | `/api/leaderboards?type={type}` | getLeaderboard |
| GET | `/api/leaderboards/history` | getLeaderboardHistory |
| PUT | `/api/profile/leaderboard-settings` | updateLeaderboardPrivacy |

### Shared Quests (5)

| Method | Path | Operation |
|--------|------|-----------|
| POST | `/api/shared-quests` | createSharedQuest |
| GET | `/api/shared-quests/{id}` | getSharedQuest |
| POST | `/api/shared-quests/{id}/invite` | inviteToSharedQuest |
| POST | `/api/shared-quests/{id}/accept` | acceptSharedQuestInvite |
| POST | `/api/shared-quests/{id}/leave` | leaveSharedQuest |

### Challenges (4)

| Method | Path | Operation |
|--------|------|-----------|
| GET | `/api/challenges` | listChallenges |
| POST | `/api/challenges/{id}/join` | joinChallenge |
| POST | `/api/challenges/{id}/withdraw` | withdrawFromChallenge |
| POST | `/api/guilds/{guildId}/challenges` | createGuildChallenge |

## Reflection — Weekly Review, Timeline, Insights, Wrapped (12 endpoints)

### Weekly Review (6)

| Method | Path | Operation |
|--------|------|-----------|
| POST | `/api/reviews` | startWeeklyReview |
| PATCH | `/api/reviews/{id}` | updateReview (save draft / add notes) |
| POST | `/api/reviews/{id}/complete` | completeReview (awards XP) |
| GET | `/api/reviews` | listPastReviews |
| GET | `/api/reviews/draft` | getDraftReview |
| PUT | `/api/reviews/settings` | configureReviewSchedule |

### Journey Timeline (3)

| Method | Path | Operation |
|--------|------|-----------|
| GET | `/api/timeline` | listTimelineEvents (paginated, filterable) |
| GET | `/api/timeline/{eventId}` | getTimelineEvent |
| PATCH | `/api/timeline/{eventId}` | updatePersonalNote |

### Insight Cards (3, Pro tier)

| Method | Path | Operation |
|--------|------|-----------|
| GET | `/api/insights` | listInsightCards |
| PATCH | `/api/insights/{id}` | updateInsightStatus (read/save/dismiss) |
| GET | `/api/insights/saved` | listSavedInsights |

### Annual Wrapped (3, Pro tier)

| Method | Path | Operation |
|--------|------|-----------|
| GET | `/api/wrapped/{year}` | getAnnualWrapped |
| GET | `/api/wrapped` | listWrappedYears |
| POST | `/api/wrapped/{year}/slides/{index}/share` | generateShareableSlide |

## Onboarding (4 endpoints)

| Method | Path | Operation |
|--------|------|-----------|
| GET | `/api/onboarding/state` | getOnboardingState |
| POST | `/api/onboarding/first-task` | createFirstTask |
| POST | `/api/onboarding/first-task/skip` | skipFirstTaskPrompt |
| GET | `/api/features/discover` | discoverFeatures (locked previews) |

## Data (5 endpoints)

| Method | Path | Operation |
|--------|------|-----------|
| POST | `/api/data/export` | requestDataExport (JSON or CSV) |
| POST | `/api/data/import` | importData |
| PUT | `/api/settings/sync` | updateSyncSettings (Pro) |
| DELETE | `/api/data` | deleteAllData |
| DELETE | `/api/account` | deleteAccount (30-day hold) |
| POST | `/api/account/recover` | recoverAccount |

## Monetisation (7 endpoints)

| Method | Path | Operation |
|--------|------|-----------|
| GET | `/api/subscription` | getSubscription |
| POST | `/api/subscription` | subscribe (Pro or Team) |
| DELETE | `/api/subscription` | cancelSubscription |
| GET | `/api/team` | getTeamWorkspace (Team tier) |
| POST | `/api/team/members` | inviteTeamMember |
| DELETE | `/api/team/members/{id}` | removeTeamMember |
| GET | `/api/cosmetics` | listCosmeticShop |
| POST | `/api/cosmetics/{itemId}/purchase` | purchaseCosmetic |

---

## Cross-Cutting Concerns

**Authentication**: All endpoints require Auth0 bearer token (already in contract via `/api/auth`).

**Premium gating**: Endpoints marked "Pro" or "Team" return 402 Payment Required when accessed by Free tier.

**Pagination**: List endpoints use cursor-based pagination (`cursor` + `limit` query params).

**Versioning**: Proposed to stay on v1 under `/api/`; breaking changes would introduce `/api/v2/`.

**Error responses**: All use the existing `ErrorResponse` schema (RFC 7807 problem details).

**Content types**: All endpoints use `application/json` for requests and responses.

## Required Before Implementation

1. **Human approval** of this endpoint list (ADR-025)
2. **Pagination convention** decision (cursor-based vs offset)
3. **Premium tier enforcement** decision (middleware vs per-controller)
4. **Bulk operations** decision (should tag operations be POST with array or one-per-call?)
5. **Deep-link URL scheme** for notifications (`waypoint://tasks/{id}` vs `https://app.waypoint.io/tasks/{id}`)

## Implementation Plan

Once approved, implementation would be one pass per feature category:
- Add OpenAPI path, request/response schemas
- Add ApplicationCommand/Query classes
- Add Controller class
- Add infrastructure repository interface
- Add example payloads for Schemathesis coverage
- Pass `pre-merge-commit` hook (Spectral + Schemathesis + coverage check)

Estimated footprint: ~3,000 lines of OpenAPI YAML additions, ~5,000 lines of C# controllers/DTOs.
