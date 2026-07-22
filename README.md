# JobBoard.Platform

A full-stack job board: employers post listings, candidates search and apply.
Built as a portfolio project pairing an ASP.NET Core backend with a Nuxt 3
frontend — the two halves are deliberately chosen to demonstrate different
things (clean architecture/CQRS on the backend, SSR/SEO on the frontend), not
just "a REST API plus some Vue pages."

## Structure

```
JobBoard.Platform/
├── JobBoard/         ASP.NET Core backend  → see JobBoard/README.md
└── jobboard-web/      Nuxt 3 frontend       → see jobboard-web/README.md
```

Each half has its own detailed README (setup, architecture, API surface, SSR
rationale). This file is the front door — quick start and how the two connect.

## Quick start

Requires: .NET 8 SDK, PostgreSQL, Node.js ≥22.12, [pnpm](https://pnpm.io).

**Backend** (`JobBoard/`) — full detail in [`JobBoard/README.md`](JobBoard/README.md):

```bash
cd JobBoard
dotnet restore
cd src/JobBoard.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 48)"
dotnet ef database update --project ../JobBoard.Infrastructure --startup-project .
dotnet run
```

API runs at `http://localhost:5000` (Swagger at `/swagger` in dev).

**Frontend** (`jobboard-web/`) — full detail in [`jobboard-web/README.md`](jobboard-web/README.md):

```bash
cd jobboard-web
pnpm install
pnpm run dev
```

Runs at `http://localhost:3000`, pointed at the API above via `NUXT_PUBLIC_API_BASE`.

## Why this stack

- **ASP.NET Core, clean architecture (Domain → Application → Infrastructure → Api),
  CQRS via MediatR** — each use case (register, post a job, apply, ...) is one
  file, testable in isolation against an abstraction (`IApplicationDbContext`)
  rather than a real database.
- **Nuxt 3, server-side rendering the public job pages** — `/jobs` and
  `/jobs/[id]` need to be crawlable and indexable; a client-only SPA can't
  guarantee that. See [`jobboard-web/README.md`](jobboard-web/README.md#why-nuxt-ssr-not-a-plain-spa)
  for the full reasoning, including why Nuxt 3 (not the newer Nuxt 4) and pnpm
  (not npm) specifically.

## Status

Backend: auth, job listings, companies, candidate profiles, and applications
are fully implemented with unit + integration test coverage. Frontend: public
SSR job search/detail pages are live; authenticated flows (login, employer
dashboard, apply) are the next milestone. See each project's README for the
detailed "what's done vs. scaffolded" breakdown.
