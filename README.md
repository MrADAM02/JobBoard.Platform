# JobBoard.Platform

[![CI](https://github.com/MrADAM02/JobBoard.Platform/actions/workflows/ci.yml/badge.svg)](https://github.com/MrADAM02/JobBoard.Platform/actions/workflows/ci.yml)

A full-stack job board: employers post listings, candidates search and apply.
Built as a portfolio project pairing an ASP.NET Core backend with a Nuxt 4
frontend — the two halves are deliberately chosen to demonstrate different
things (clean architecture/CQRS on the backend, SSR/SEO on the frontend), not
just "a REST API plus some Vue pages."

## Structure

```
JobBoard.Platform/
├── JobBoard/         ASP.NET Core backend  → see JobBoard/README.md
└── jobboard-web/      Nuxt 4 frontend       → see jobboard-web/README.md
```

Each half has its own detailed README (setup, architecture, API surface, SSR
rationale). This file is the front door — quick start and how the two connect.

## Quick start

Requires: .NET 8 SDK, PostgreSQL, Node.js `^22.19.0 || ^24.11.0 || >=26.0.0`
(Nuxt 4.5's floor), [pnpm](https://pnpm.io).

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
- **Nuxt 4, server-side rendering the public job pages** — `/jobs` and
  `/jobs/[id]` need to be crawlable and indexable; a client-only SPA can't
  guarantee that. See [`jobboard-web/README.md`](jobboard-web/README.md#why-nuxt-ssr-not-a-plain-spa)
  for the full reasoning, including why pnpm (not npm) specifically.

## Features

- **Core marketplace** — employer job postings (draft/publish/close/expire),
  candidate search + apply, resume/logo upload, company profiles, a
  "similar jobs" section on each listing (same type/location, ranked by
  best match then recency)
- **Candidate tools** — saved/bookmarked jobs, in-app notifications
- **Employer tools** — applicant tracking with private notes, an analytics
  dashboard (views over time, application-status breakdown, top listings),
  auto-closing listings past their expiry date
- **Admin panel** — platform stats, user management, cross-company listing moderation
- **i18n/RTL** — full English/Arabic UI with real right-to-left layout, not
  just a text-direction flip
- **Dark/light theme**, mobile-responsive, a small shared design system
  (buttons/inputs/cards/toasts) instead of per-page ad-hoc styling
- **Background jobs** (Hangfire) for outbound email and the job-expiry sweep;
  output caching, rate limiting, and a health check on the API; CI running
  build + test on every push (backend `dotnet test`, frontend Vitest)

## Status

Both halves are fully built out: auth, job listings, companies, candidate
profiles, applications, notifications, saved jobs, an admin panel, and an
employer analytics dashboard, backed by unit + integration test coverage on
the API and a Vitest suite on the frontend, plus SSR on every public-facing
page. See each project's README for the detailed feature/API breakdown. Not
deployed anywhere yet — this repo is built and verified entirely locally
(see each README's "what's left" notes for what deployment would still need).
