# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository structure

Two independent halves living in one repo, each with its own detailed README (setup, architecture, full API/page breakdown, and the reasoning behind key decisions):

```
JobBoard.Platform/
├── JobBoard/         ASP.NET Core 8 backend  → JobBoard/README.md
└── jobboard-web/     Nuxt 4 frontend         → jobboard-web/README.md
```

This is a portfolio project: the backend demonstrates clean architecture/CQRS, the frontend demonstrates SSR/SEO discipline. Not deployed anywhere — built and verified entirely locally.

## Commands

### Backend (`JobBoard/`)

```bash
cd JobBoard
dotnet restore
dotnet build JobBoard.sln --configuration Release
dotnet test JobBoard.sln --configuration Release      # both test projects
dotnet test tests/JobBoard.Application.UnitTests --filter "FullyQualifiedName~GetSimilarJobs"   # single test/class
cd src/JobBoard.Api && dotnet run                      # http://localhost:5000, Swagger at /swagger
```

First-time setup needs JWT secret + connection string via user-secrets (see `JobBoard/README.md`) and `dotnet ef database update --project ../JobBoard.Infrastructure --startup-project .`.

### Frontend (`jobboard-web/`)

```bash
cd jobboard-web
pnpm install               # pnpm, not npm — see "Tooling notes" in jobboard-web/README.md
pnpm run dev                # http://localhost:3000
pnpm run test                # vitest run — all tests
pnpm exec vitest run tests/Pagination.test.ts   # single file
pnpm run build && pnpm run preview
```

Requires Node `^22.19.0 || ^24.11.0 || >=26.0.0` (Nuxt 4.5's floor) — `pnpm env use --global 24.18.1` (or `pnpm runtime set node 24.18.1 -g` on newer pnpm) fetches and switches a compatible version without nvm. On Windows specifically, a machine-wide Node install under `C:\Program Files\nodejs` in the **System** PATH silently wins over pnpm's managed version in the **User** PATH — if `node --version` doesn't reflect the version you just switched to, that's why; uninstalling the system-level Node (or removing it from System PATH) fixes it, and needs an elevated/admin shell.

Copy `jobboard-web/.env.example` to `.env` to point at a non-default API URL; both vars already fall back to sane local defaults.

### Docker

```bash
docker compose up --build   # Postgres + API (:5000) + web (:3000)
```

`JobBoard/Dockerfile` and `jobboard-web/Dockerfile` are each multi-stage; `docker-compose.yml` (repo root) wires them together. The API container runs with `ASPNETCORE_ENVIRONMENT=Development` on purpose, so it reuses the existing auto-migrate + dev-admin-seed block in `Program.cs` (`admin@jobboard.local` / `Admin123!`) — no separate migration step needed. `JWT_SECRET` has a dev-only fallback baked into `docker-compose.yml`; override via a root `.env` (see `.env.example`) for anything beyond trying the app locally.

### CI (`.github/workflows/ci.yml`)

Three independent jobs on every push/PR to `main`: `backend` (`dotnet restore` → `build` → `test`), `frontend` (`pnpm install --frozen-lockfile` → `pnpm run test` → `pnpm run build`, Node 24/pnpm 10), `docker` (build both images, no push). Mirror these commands locally before assuming a change is CI-clean.

## Backend architecture

Clean/layered architecture, dependencies point inward:

```
JobBoard.Api            <- controllers, Program.cs, JWT config, Swagger
   |
JobBoard.Infrastructure <- EF Core, Postgres, JWT/password/email/file services
   |
JobBoard.Application    <- CQRS (MediatR), validation (FluentValidation), DTOs
   |
JobBoard.Domain         <- entities, enums. No dependencies on anything.
```

- **CQRS via MediatR**: every use case (register, create job listing, apply to job, ...) is one file under `src/JobBoard.Application/Features/<Area>/{Commands,Queries}/<UseCase>/<UseCase>Command.cs` — the record, its `IRequestHandler`, and a co-located `AbstractValidator<T>` when input needs validating all live together. Follow this exact shape for new use cases rather than adding logic to a shared service class.
- **`Application` never touches EF Core's `DbContext` directly** — only `IApplicationDbContext`, so handlers are testable against `EF Core InMemory` without a real database. Don't inject `ApplicationDbContext` into a handler; inject the interface.
- `ValidationBehavior<TRequest, TResponse>` (`Common/Behaviors/`) is a MediatR pipeline behavior that runs every registered `IValidator<TRequest>` before the handler executes and throws `FluentValidation.ValidationException` on failure — handlers themselves never validate.
- `ExceptionHandlingMiddleware` (`JobBoard.Api/Middleware/`) is the single place mapping exceptions → HTTP responses: `ValidationException` → 400, `NotFoundException` → 404, `ForbiddenAccessException` → 403, `UnauthorizedAccessException` → 401, anything else → 500 with a generic `{"errors":{"message":"An unexpected error occurred."}}` body (the real exception is logged server-side only). When adding a new failure mode, prefer throwing one of the existing `Common/Exceptions/*` types over a raw exception.
- Controllers stay thin: parse the route/query into a command/query record, `await _mediator.Send(...)`, done. No business logic in controllers.
- **User-supplied `DateTime` values need `DateTime.SpecifyKind(..., DateTimeKind.Utc)` before persisting.** JSON deserialization produces `DateTimeKind.Unspecified`, which Npgsql rejects for `timestamp with time zone` columns (this bit `ExpiresAt` in both `CreateJobListingCommand` and `UpdateJobListingCommand` — see their handlers for the pattern to copy for any new user-supplied date field).
- `Directory.Build.props` (this project's root, next to `JobBoard.sln`) sets `TargetFramework`/`ImplicitUsings`/`Nullable` once for all 6 projects via MSBuild's directory-walk-up convention — don't re-add those to a `.csproj`, only project-specific properties (`UserSecretsId`, `IsPackable`) belong there.
- Background jobs run through Hangfire (Postgres-backed, dashboard at `/hangfire`, localhost-only): emails are enqueued (`BackgroundJob.Enqueue<IEmailService>`) rather than awaited inline, and an hourly recurring job auto-closes listings past `ExpiresAt`.
- Output caching (`[OutputCache(PolicyName = "PublicReads")]`) is the established pattern for public, unauthenticated GET endpoints (job/company reads) — apply it to new public read endpoints too.
- Full endpoint-by-endpoint API surface (auth requirements, notes) is documented in `JobBoard/README.md` — check there before assuming an endpoint doesn't exist.

### Backend testing

- `tests/JobBoard.Application.UnitTests` — handlers run against `TestHelpers.TestDbContextFactory` (real `ApplicationDbContext`, EF Core InMemory provider, isolated per-test database). Prioritizes ownership/authorization checks and non-obvious logic (analytics zero-fill, expiry bulk-update, similar-jobs ranking) over blanket coverage.
- `tests/JobBoard.Api.IntegrationTests` — `WebApplicationFactory` over real HTTP, Hangfire storage swapped to in-memory so tests never touch a real Postgres instance.

## Frontend architecture

- **`app/` is the `srcDir`** (Nuxt 4's default) — `~/` resolves there. `app/pages`, `app/components`, `app/composables`, `app/layouts`, `app/middleware`, `app/plugins`, `app/stores`, `app/types`, `app/assets`, `app/app.vue`, `app/error.vue`. `i18n/` (locale JSON + `i18n.config.ts`) deliberately stays at the repo root, outside `app/` — that's `@nuxtjs/i18n`'s own convention (`experimental.restructureDir`) since translations are needed both server- and client-side. `public/`, `tests/`, and every config file (`nuxt.config.ts`, `tailwind.config.ts`, `vitest.config.ts`) also stay at root. `tailwind.config.ts`'s hand-written `content` globs and any relative import in `tests/` that reaches into app code (e.g. `tests/Pagination.test.ts`) need the `app/` prefix — they don't auto-resolve.
- ⚠️ **`nuxt.config.ts` currently has a top-level `ssr: false`**, added 2026-08-01 outside of a specific tracked change, which disables server-rendering for the *entire* app. This directly contradicts the "SSR is deliberate" reasoning below and `jobboard-web/README.md#why-nuxt-ssr-not-a-plain-spa` — right now none of that is actually happening (`curl`-ing a page returns the SPA loading shell, not real content). Flagged, not fixed, per explicit instruction when last discussed. Don't assume SSR is live without checking this line first.
- **SSR is deliberate** (see caveat above), not default Nuxt behavior left alone. The public pages (`/jobs`, `/jobs/[id]`, `/companies*`) must be crawlable/indexable — see `jobboard-web/README.md#why-nuxt-ssr-not-a-plain-spa` for the full reasoning (empty-shell SPA problem, `JobPosting` JSON-LD, URL-driven filters). Authenticated dashboard pages (`app/pages/dashboard/**`) are `ssr: false` per-page and fetch client-side after hydration — there's no SEO value behind a login wall. When adding a new public-facing page, default to SSR (`useAsyncData` for the fetch); when adding a new dashboard page, follow the existing `ssr: false` + `definePageMeta({ middleware: 'auth' })` pattern instead.
- **Hydration-mismatch discipline**: anything that reads client-only state (the Pinia auth store, before it hydrates from `localStorage`) must be wrapped in `<ClientOnly>` — see every `auth.isAuthenticated` branch in `app/layouts/default.vue` for the pattern. Conversely, anything that must render identically on server and client but whose *value* can differ (e.g. `useColorMode()`'s resolved preference vs. its SSR fallback) should keep both branches in the DOM and toggle visibility via a CSS class (e.g. `dark:block`/`dark:hidden`) rather than a `v-if` — see `app/components/ThemeToggle.vue`. Don't reach for `v-if` on state that isn't guaranteed identical between server and client render.
- **API layer**: one composable per backend feature area under `app/composables/use*Api.ts` (`useJobsApi`, `useApplicationsApi`, etc.), each a thin typed `$fetch` wrapper. Public/anonymous calls use `$fetch` directly against `runtimeConfig.public.apiBase`; authenticated calls go through `useAuthFetch` (attaches the JWT, retries once through refresh-token rotation on a 401). Add new endpoints as a function on the relevant `use*Api.ts`, not as inline `$fetch` calls in a page.
- **Design system**: `app/components/Base*` (Button/Input/Textarea/Select), `Card`, `Badge`, `Alert`, `EmptyState`, `Spinner` — reuse these instead of hand-rolling Tailwind strings in a page. `ToastContainer` + `composables/useToast.ts` is the shared success/error feedback mechanism for mutations (module-singleton reactive queue — see `tests/useToast.test.ts` for why tests need `vi.resetModules()` to isolate state).
- **i18n/RTL**: `i18n/locales/{en,ar}.json` are the only translatable strings — job/company content typed by users is never auto-translated. `i18n.config.ts` has a hand-rolled 6-form Arabic CLDR plural rule (`arabicPluralRule`, exported for testing). RTL uses Tailwind logical properties (`ps-`/`pe-`/`ms-`/`me-`), not a blanket direction flip.
- Types mirror the backend DTOs under `app/types/*.ts`, kept in sync by hand — when a backend DTO shape changes, update the matching frontend type.

### Frontend testing

Vitest via `@nuxt/test-utils`. Two setups depending on what a test needs:
- Plain `happy-dom` environment (default, from `vitest.config.ts`) for pure logic — e.g. `tests/i18n.config.test.ts`, `tests/useToast.test.ts`.
- Full Nuxt test environment via a `// @vitest-environment nuxt` docblock at the top of the file, plus `mountSuspended` from `@nuxt/test-utils/runtime` (not `@vue/test-utils`'s plain `mount`), for components that call `useI18n()` or rely on auto-imports — e.g. `tests/Pagination.test.ts`.

Tests target non-trivial/risk logic (CLDR plural boundaries, toast auto-dismiss timing, pagination ellipsis-collapsing), not blanket coverage — match that bar for new tests rather than testing every component.
