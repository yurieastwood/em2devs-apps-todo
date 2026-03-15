# Waypoint — BDD Feature Specifications

Behaviour-Driven Development (BDD) specifications for Waypoint, written in Gherkin syntax.
These files serve as the single source of truth for product behaviour and are intended for
use with any Gherkin-compatible test runner (SpecFlow, Cucumber, Reqnroll, etc.).

## Directory Structure

```
features/
├── core/                          # Foundation: task management and organisation
│   ├── task-management.feature        Tasks: CRUD, views, filtering, sorting
│   ├── quest-hierarchy.feature        Quests, epics, sagas, and hierarchy navigation
│   ├── recurring-tasks.feature        Recurring tasks and auto-generating quest chains
│   ├── boss-tasks.feature             Procrastinated-task detection, intervention, and rewards
│   └── notifications.feature          Reminders, achievement alerts, and notification preferences
│
├── progression/                   # Gamification engine: XP, levels, identity
│   ├── experience-points.feature      XP calculation, weighting, display, and anti-gaming
│   ├── levelling.feature              Level thresholds, progressive feature unlocks
│   ├── skill-trees.feature            Behaviour-driven skill trees, tiers, and perks
│   ├── titles-and-ranks.feature       Sustained-behaviour titles, display, and retention
│   ├── streaks.feature                Streak tracking, grace days, streak freeze
│   └── seasons.feature                Quarterly seasons, quest lines, leaderboards, cosmetics
│
├── intelligence/                  # Productivity engine: smart scheduling and insights
│   ├── energy-scheduling.feature      Energy check-in, pattern learning, task surfacing
│   ├── capacity-modelling.feature     Throughput learning, overcommitment warnings
│   ├── time-estimation.feature        Estimation tracking, bias detection, corrected suggestions
│   ├── daily-brief.feature            Morning plan generation, interaction, learning
│   └── procrastination-detection.feature  Avoidance signals, intervention flows, insights
│
├── social/                        # Multiplayer: collaboration and competition
│   ├── guilds.feature                 Guild CRUD, quest boards, progression
│   ├── accountability-partners.feature  Pairing, shared summaries, check-in messages
│   ├── leaderboards.feature           Cohort-based ranking, types, privacy
│   ├── shared-quests.feature          Multi-contributor quests
│   └── challenge-mode.feature         Time-limited competitions, integrity
│
├── reflection/                    # Retrospection: reviews, timeline, insights
│   ├── weekly-review.feature          Guided review ritual, streaks, basic and advanced flows
│   ├── journey-timeline.feature       Event timeline, filtering, annotations
│   ├── insight-cards.feature          Pattern-based personalised observations
│   └── annual-wrapped.feature         Year-end summary, sharing
│
├── monetisation/                  # Business model
│   └── subscription-tiers.feature     Free, Premium, Team tiers, cosmetics, no pay-to-win
│
├── onboarding/                    # First-run experience
│   └── progressive-disclosure.feature Gradual feature reveal, retroactive activation, tutorials
│
└── data/                          # Data ownership and privacy
    └── local-first-data.feature       Offline-first, sync, export, deletion
```

## Tag Taxonomy

| Tag                      | Purpose                                   |
|--------------------------|-------------------------------------------|
| `@core`                  | Foundation task management features        |
| `@tasks`                 | Task CRUD operations                       |
| `@quests`                | Quest hierarchy management                 |
| `@recurring`             | Recurring tasks and quest chains           |
| `@boss-tasks`            | Boss Task mechanics                        |
| `@notifications`         | Notification system                        |
| `@progression`           | Gamification engine features               |
| `@xp`                    | Experience points                          |
| `@levels`                | Levelling system                           |
| `@skill-trees`           | Skill tree mechanics                       |
| `@titles`                | Titles and ranks                           |
| `@streaks`               | Streak and grace day mechanics             |
| `@seasons`               | Seasonal content                           |
| `@intelligence`          | Productivity intelligence features         |
| `@energy`                | Energy-aware scheduling                    |
| `@capacity`              | Capacity modelling                         |
| `@estimation`            | Time estimation learning                   |
| `@daily-brief`           | Smart daily brief                          |
| `@procrastination`       | Procrastination detection                  |
| `@social`                | Social and multiplayer features            |
| `@guilds`                | Guild mechanics                            |
| `@accountability`        | Accountability partners                    |
| `@leaderboards`          | Leaderboard system                         |
| `@shared-quests`         | Collaborative quests                       |
| `@challenges`            | Challenge mode                             |
| `@reflection`            | Retrospection features                     |
| `@weekly-review`         | Weekly review ritual                       |
| `@timeline`              | Journey timeline                           |
| `@insights`              | Insight cards                              |
| `@wrapped`               | Annual wrapped                             |
| `@monetisation`          | Subscription and payment features          |
| `@tiers`                 | Tier-specific behaviour                    |
| `@onboarding`            | First-run and progressive disclosure       |
| `@progressive-disclosure`| Feature revelation mechanics               |
| `@data`                  | Data management features                   |
| `@local-first`           | Offline and local storage                  |
| `@premium`               | Requires premium subscription              |

## Running By Phase

The phased rollout from the product vision maps to tags as follows:

| Phase                    | Tags to include                                                    |
|--------------------------|--------------------------------------------------------------------|
| Phase 1: Foundation      | `@core`, `@data`, `@onboarding` (partial)                         |
| Phase 2: Progression     | `@progression`, `@onboarding` (retroactive activation)            |
| Phase 3: Social          | `@social`                                                          |
| Phase 4: Intelligence    | `@intelligence`, `@reflection` (insight cards, wrapped)           |
| Phase 5: Teams           | `@guilds` (team tier), `@monetisation` (team tier)                |

## Conventions

- **Background steps** establish common preconditions per feature.
- **Rules** group scenarios by business rule within a feature.
- **Scenario Outlines** with Examples are used where behaviour varies by parameter.
- **`@premium`** tag marks scenarios that require a premium subscription.
- All scenarios are written from the user's perspective; no implementation detail is specified.
