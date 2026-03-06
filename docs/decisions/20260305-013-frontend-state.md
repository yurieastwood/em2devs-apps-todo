# Frontend State Management — Svelte 5 Runes + Stores + SvelteKit Load Functions

- Status: Accepted
- Date: 2026-03-05

## Context and Problem Statement

EM2Devs.Todo is a gamified todo application with three distinct categories of state: component-local UI state (form inputs, modal visibility, animation triggers), cross-cutting shared state that multiple unrelated components must read and write (authenticated user identity, XP/level display, notification queue), and server-loaded page data (todo lists, quest details, leaderboard rankings). The chosen SvelteKit / Svelte 5 framework ([ADR-002](20260305-002-frontend-framework.md)) ships multiple built-in mechanisms for handling each category. The question is whether those built-in tools are sufficient, or whether an external state management library should be added. What state management strategy should the EM2Devs.Todo SvelteKit frontend follow?

## Decision Drivers

- Svelte 5 ecosystem alignment: prefer patterns the Svelte 5 compiler and SvelteKit runtime are designed to support natively
- Simplicity: avoid external dependencies that add cognitive overhead, bundle weight, or fight against Svelte's reactive model
- Reactivity performance: gamification state (XP counters, streak timers, achievement progress) changes frequently and must propagate efficiently
- SSR compatibility: server-side rendering must work without client-only state assumptions
- Developer experience: patterns should be discoverable, composable, and easy to reason about for a small team
- Minimal boilerplate: the overhead of any pattern must be proportional to the complexity it manages

## Considered Options

- Svelte 5 built-in tools (runes + stores + SvelteKit load functions + context API)
- External state management library (Zustand/Jotai-style adapter for Svelte)
- Redux-style global store (svelte-redux or hand-rolled flux pattern)

## Decision Outcome

Chosen option: "Svelte 5 built-in tools", because the four built-in mechanisms together cover every state category this application has without requiring an external dependency. Each mechanism is scoped to the exact problem it solves, which keeps patterns predictable and avoids the "put everything in the global store" anti-pattern that external libraries tend to encourage.

The four mechanisms and their designated roles are:

- **Runes (`$state`, `$derived`, `$effect`)** — component-local state and derived values. This is the primary reactive primitive for anything scoped to a single component or a small composable function. Replaces Svelte 4's `let` + `$:` reactive declarations with an explicit, compiler-checked API.
- **SvelteKit `load` functions** — server-side and universal data loading for route and layout data. Handles SSR, streaming, cache invalidation (`invalidate()`), and dependent data waterfalls. Used for fetching todos, quests, leaderboard data, and user profiles. Data returned from `load` is available as the `data` prop on the corresponding page component.
- **Svelte stores (`writable`, `readable`, `derived`)** — cross-cutting shared state that unrelated components need to access. Used for auth state (current user session), the notification/toast queue, and the XP/level values rendered in the global header. Stores are compatible with SSR when initialised correctly.
- **Context API (`setContext` / `getContext`)** — scoped dependency injection within a component subtree. Used to pass a "current quest" or "active collection" object down a deep component tree without threading props through every intermediate component.

This layered approach means no single mechanism becomes a dumping ground for all state. Engineers follow a clear decision rule: is this state local to one component? Use runes. Is it page or layout data loaded from the server? Use a `load` function. Do multiple unrelated components across the tree need it? Use a store. Does a parent need to share something with deep children in a bounded subtree? Use context.

### Positive Consequences

- Zero additional dependencies — the state management layer has no npm packages to update, audit, or keep compatible with SvelteKit upgrades
- Each pattern matches the Svelte 5 compiler's expectations; runes, stores, and load functions all integrate with Svelte's devtools and HMR without special adapters
- SSR is safe by design: `load` functions run on the server, stores can be seeded from server data, and runes are component-scoped and therefore not shared across requests
- The decision rule (local / server-loaded / cross-cutting / scoped tree) is teachable and eliminates bikeshedding about where to put new state
- Bundle size is not increased by any external library

### Negative Consequences

- There is no single place to inspect all application state; developers accustomed to Redux DevTools will find per-component rune state and distributed stores harder to trace across the whole app
- Svelte 5 runes are a significant shift from Svelte 4 patterns; developers with Svelte 4 experience must unlearn `$:` reactive declarations and `export let` for shared state
- Large-scale data normalisation (e.g., caching and deduplicating nested todo/quest relationships) is not handled out of the box — if the data model grows complex, a query-cache library (like TanStack Query for Svelte) may be needed as a targeted addition

### Neutral

- Svelte stores use the subscriber pattern and are not tied to Svelte's component lifecycle, making them usable in plain TypeScript modules (e.g., service classes) as well as components
- The context API is not global — it does not leak across unrelated component trees, which is intentional but means it cannot replace stores for app-wide state

## Pros and Cons of the Options

### Svelte 5 built-in tools (runes + stores + load functions + context API)

Four complementary mechanisms shipped with Svelte 5 and SvelteKit, each optimised for a different state scope.

- Good, because no external dependency is needed — the installed framework already provides all required primitives
- Good, because runes are fine-grained and compile-time checked; `$derived` values are memoised automatically without manual dependency arrays
- Good, because SvelteKit `load` functions handle SSR, streaming, and cache invalidation as first-class concerns — server state does not need a separate client-side caching layer for this app's scale
- Good, because Svelte stores are reactive to the auto-subscription shorthand (`$storeName`) in templates, making cross-component state feel as natural as local state
- Good, because the context API prevents prop drilling in deep trees without polluting a global namespace
- Bad, because there is no unified devtools view of all state — debugging requires checking component state, stores, and load data separately
- Bad, because the decision rule requires discipline; without code review enforcement, engineers may reach for stores when runes suffice, or bypass `load` functions in favour of client-side fetching

### External state management library (Zustand/Jotai-style adapter for Svelte)

Libraries like `svelte-zustand` or custom atom-based stores that mirror patterns popular in the React ecosystem.

- Good, because developers with React/Zustand experience can transfer mental models directly
- Good, because atom-based libraries provide fine-grained subscriptions that avoid over-rendering
- Bad, because Svelte stores already provide a subscribable primitive — an external atom library largely duplicates it with extra indirection
- Bad, because any external library must be maintained for Svelte 5 compatibility; the Svelte 5 runes model is new and many React-ecosystem adapters have not caught up
- Bad, because adds a dependency and onboarding overhead for a problem the built-in tools already solve at this app's scale

### Redux-style global store (svelte-redux or hand-rolled flux)

A single global state tree with explicit actions and reducers, following the Flux/Redux pattern.

- Good, because all state transitions are explicit and traceable through a reducer function — good for complex workflows with many actors
- Good, because Redux DevTools can be wired in to provide a time-travel debugging experience
- Bad, because this is significant over-engineering for a single-frontend app; the todos/quests/XP domain does not have the multi-actor, multi-system state synchronisation problems that Redux was designed to solve
- Bad, because the ceremony of defining action types, action creators, and reducers for every state change is disproportionate to the complexity of the state being managed
- Bad, because Redux's synchronous reducer model does not integrate naturally with SvelteKit `load` functions, creating a seam where server-loaded data must be manually pushed into the store on page navigation

## More Information

- [Svelte 5 Runes — $state, $derived, $effect](https://svelte.dev/docs/svelte/what-are-runes)
- [Svelte stores (writable, readable, derived)](https://svelte.dev/docs/svelte/stores)
- [SvelteKit load functions](https://svelte.dev/docs/kit/load)
- [Svelte context API (setContext / getContext)](https://svelte.dev/docs/svelte/context)
- Related: [ADR-002](20260305-002-frontend-framework.md) — SvelteKit / Svelte 5 as the frontend framework
- Related: [ADR-004](20260305-004-api-style.md) — API integration and how `load` functions consume REST endpoints and SignalR events
