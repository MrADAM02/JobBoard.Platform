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

## Docker

No local toolchain needed — Postgres, the API, and the frontend all run in containers:

```bash
cp .env.example .env   # set JWT_SECRET and POSTGRES_PASSWORD - compose won't start without them
docker compose up --build
```

API at `http://localhost:5000`, frontend at `http://localhost:3000`. The API
container auto-migrates the database and seeds a dev admin account on startup
(`admin@jobboard.local` / `Admin123!`) — same convenience behavior as running
the API locally in Development mode. Postgres data and uploaded files persist
across restarts via named volumes. Neither secret has a fallback, on purpose —
a known credential baked into the compose file is exactly the kind of thing
that ends up running for real by accident.

## Deploying to a server

Every push to `main` builds both images, publishes them to GitHub Container
Registry (`ghcr.io/mradam02/jobboard-api` / `-web`), then SSHes into the
server and redeploys — see the `publish` and `deploy` jobs in
[`.github/workflows/ci.yml`](.github/workflows/ci.yml). The server runs in
`Production` mode: no Swagger, no auto-seeded admin account (unlike the local
`docker-compose.yml` above) — migrations still apply automatically on every
boot, but you create the first admin yourself, once, as below.

**One-time server setup:**

```bash
git clone https://github.com/MrADAM02/JobBoard.Platform.git
cd JobBoard.Platform
cp .env.production.example .env
# edit .env: set PUBLIC_HOST to this server's IP, POSTGRES_PASSWORD, and
# JWT_SECRET (openssl rand -base64 48)
docker compose -f docker-compose.prod.yml up -d
```

If `docker compose version` errors with `unknown command: docker compose`, the
Compose v2 CLI plugin isn't installed — this bites Ubuntu servers where Docker
came from the `docker.io` apt package rather than Docker's own repo, since
`docker.io` doesn't bundle it. Fix with `sudo apt-get install docker-compose-v2`
(Ubuntu's package name for the plugin) or `docker-compose-plugin` if you
installed Docker from `download.docker.com` instead.

**One-time GitHub setup** — add these as repo secrets (Settings → Secrets and
variables → Actions):
- `DEPLOY_HOST` — the server's IP
- `DEPLOY_USER` — the SSH user
- `DEPLOY_SSH_KEY` — the private key for that user

After the first successful `publish` run, the two GHCR packages are created
as private by default — flip them to Public once under the repo's Packages
tab (sidebar → Packages → each package → Package settings) so the server can
pull them without authenticating.

**Creating the first admin account** — register a normal account through the
running app at `http://<server-ip>:3000`, then promote it directly in
Postgres (self-registering as Admin is intentionally blocked — see
[`JobBoard/README.md`](JobBoard/README.md)):

```bash
docker compose -f docker-compose.prod.yml exec postgres \
  psql -U postgres -d jobboard \
  -c "UPDATE \"Users\" SET \"Role\" = 2 WHERE \"Email\" = 'you@example.com';"
```

No domain/HTTPS yet — the app is reachable over plain HTTP by IP
(`http://<server-ip>:3000` and `:5000`). Worth adding a reverse proxy
(Caddy is the simplest option — automatic Let's Encrypt certs) once a domain
is pointed at the server.

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
page. See each project's README for the detailed feature/API breakdown.

Deployed via the CI `publish`/`deploy` pipeline (see "Deploying to a server"
above) — pushes to `main` build and publish both images to GHCR, then
SSH-redeploy the remote server automatically. No domain/HTTPS in front of it
yet (see the note at the end of that section).
