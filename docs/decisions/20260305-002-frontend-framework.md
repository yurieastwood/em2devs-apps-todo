# Frontend Framework — SvelteKit / Svelte 5

- Status: Accepted
- Date: 2026-03-05

## Context and Problem Statement

EM2Devs.Todo is a gamified todo application that requires an interactive frontend with animations, real-time progress updates, achievement notifications, and a dashboard-style UI. The framework choice will determine the long-term developer experience, bundle size, and how naturally the gamification layer can be expressed in component code. Which frontend framework and meta-framework should be used?

## Decision Drivers

- Performance: runtime overhead, bundle size, and rendering efficiency matter for an interactive, animation-heavy UI
- Reactivity model: gamification state (XP, streaks, achievements) changes frequently and must propagate to the UI with minimal boilerplate
- Developer experience: how quickly can a developer write, understand, and maintain a component?
- Routing, SSR, and data loading: the meta-framework must handle these without bespoke configuration
- Ecosystem: availability of component libraries and integrations
- Bundle size: smaller output means faster initial load, especially on mobile networks

## Considered Options

- SvelteKit with Svelte 5 (runes reactivity model)
- Next.js with React (React 18 / 19, App Router)
- Nuxt with Vue 3 (Composition API)

## Decision Outcome

Chosen option: "SvelteKit with Svelte 5", because Svelte compiles components to vanilla JavaScript at build time — there is no virtual DOM and no framework runtime shipped to the browser. This produces the leanest possible output for an interactive, gamified UI. Svelte 5's runes (`$state`, `$derived`, `$effect`) provide fine-grained, explicit reactivity that maps cleanly to gamification state (XP counters, streak timers, achievement progress). SvelteKit handles routing, SSR, data loading (`load` functions), and form actions with minimal ceremony.

### Positive Consequences

- No virtual DOM diffing overhead; animations and frequent state updates are cheap
- Compiled output is small — faster initial load and better Lighthouse scores
- Svelte 5 runes make reactive state explicit and easy to trace (no hidden proxies or compiler magic for component state)
- SvelteKit `load` functions co-locate data fetching with routes, reducing boilerplate
- Built-in transitions and animation primitives reduce reliance on additional animation libraries
- Straightforward TypeScript integration with full type safety in templates

### Negative Consequences

- Smaller component library ecosystem compared to React; developers may need to build more components from scratch or adapt headless libraries
- Svelte 5 runes are a significant syntax change from Svelte 4; team must familiarise themselves with the new model
- Fewer Svelte-specific hiring candidates compared to React; onboarding React-only developers requires a short learning curve

### Neutral

- SvelteKit supports both SSR and SPA/CSR modes per route; the gamification dashboard may ship as a CSR route while landing pages use SSR
- The Svelte ecosystem is growing rapidly; major UI libraries (shadcn-svelte, Melt UI) have adopted Svelte 5

## Pros and Cons of the Options

### SvelteKit with Svelte 5

A compiler-first framework that ships no runtime to the browser. Svelte 5 replaces the legacy reactive declarations with an explicit runes API.

- Good, because compiled output has minimal runtime overhead — ideal for animation-heavy gamified UIs
- Good, because runes (`$state`, `$derived`, `$effect`) provide clear, composable reactivity without context boilerplate
- Good, because SvelteKit covers routing, SSR, data loading, and API routes in one cohesive package
- Good, because built-in `animate:`, `transition:`, and `use:` directives handle most gamification animation needs natively
- Good, because bundle sizes are consistently smaller than equivalent React or Vue apps
- Bad, because component library ecosystem is smaller; some React-ecosystem libraries have no Svelte equivalent
- Bad, because Svelte 5 runes are still relatively new; community patterns and best practices are still stabilising

### Next.js with React

The dominant React meta-framework. App Router (introduced in Next.js 13) uses React Server Components for server-side rendering.

- Good, because React has the largest component library ecosystem (shadcn/ui, Radix, Headless UI, etc.)
- Good, because hiring pool of React developers is the largest of any frontend framework
- Good, because Next.js App Router is mature and production-proven at scale
- Bad, because React ships a runtime to the browser; bundle size is larger by default
- Bad, because React Server Components add conceptual complexity (server vs. client component boundaries) that requires careful management in an interactive app
- Bad, because frequent re-renders in complex gamification state require explicit memoisation (`useMemo`, `useCallback`, `React.memo`)

### Nuxt with Vue 3

Vue's official meta-framework. Vue 3 Composition API provides a clean reactivity model.

- Good, because Vue 3's `ref`/`reactive` Composition API is intuitive and well-documented
- Good, because Nuxt's auto-imports and conventions reduce boilerplate significantly
- Good, because strong ecosystem in Europe and Asia-Pacific; good hiring pool in those regions
- Bad, because Vue's component library ecosystem, while solid (Vuetify, PrimeVue), is smaller than React's
- Bad, because Vue ships a runtime; bundle size advantage over React is marginal compared to Svelte
- Bad, because the team has no existing Vue experience; learning curve is similar to Svelte without the bundle size benefit

## More Information

- [Svelte 5 Runes documentation](https://svelte.dev/docs/svelte/what-are-runes)
- [SvelteKit documentation](https://svelte.dev/docs/kit/introduction)
- [Svelte 5 migration guide from Svelte 4](https://svelte.dev/docs/svelte/v5-migration-guide)
- Related: [ADR-013](20260305-013-frontend-state.md) — Frontend state management strategy
