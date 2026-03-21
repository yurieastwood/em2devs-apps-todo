# EM2Devs.Todo.Web

SvelteKit frontend for the EM2Devs Todo application.

## Prerequisites

- Node.js (see `.nvmrc` for pinned version)
- Backend API running at `http://localhost:5001` (or set `API_BASE_URL`)

## Development

```sh
npm install
npm run dev
```

Or use .NET Aspire from the repo root to launch both frontend and backend:

```sh
dotnet run --project src/EM2Devs.Todo.AppHost
```

## Scripts

| Script                 | Description               |
| ---------------------- | ------------------------- |
| `npm run dev`          | Start dev server          |
| `npm run build`        | Production build          |
| `npm run check`        | Type check (svelte-check) |
| `npm run lint`         | Lint (ESLint)             |
| `npm run format`       | Format (Prettier)         |
| `npm run format:check` | Verify formatting         |
| `npm run test`         | Run unit tests (Vitest)   |

## Tech Stack

- SvelteKit with Svelte 5 (runes)
- TypeScript
- Vitest + @testing-library/svelte
- ESLint + Prettier
