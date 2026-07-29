# jobboard-web

Nuxt 3 frontend for JobBoard, consuming the ASP.NET Core API in [`../JobBoard`](../JobBoard).

## Why Nuxt (SSR), not a plain SPA

The public job listing pages (`/jobs`, `/jobs/[id]`) are server-rendered on every
request — that's the reason this project uses Nuxt instead of a client-only SPA:

- **A client-rendered SPA ships an empty root `<div>`.** Content only appears after
  the browser downloads and executes JavaScript, then makes an API call. Googlebot
  can eventually render JS, but it's slower and less reliable than plain HTML, and
  most other crawlers (social-preview bots for Slack/Twitter/LinkedIn, some search
  engines) don't execute JS at all — they see nothing.
- **SSR guarantees content and meta tags are in the first response.** With Nuxt's
  `useAsyncData`/`useFetch` running server-side, the job title, description, and
  `<title>`/`<meta description>`/Open Graph tags are already in the HTML the server
  sends back. `curl` or "view source" show the real content, not a loading spinner.
- **Search-driven URLs stay crawlable.** Filters on `/jobs` (`?keyword=`, `?location=`,
  etc.) are read from the URL query string, not client-only component state. A direct
  request to a filtered URL — which is exactly what a crawler or a shared link does —
  server-renders the same filtered results as an in-app navigation would.
- **`/jobs/[id]` emits [`JobPosting`](https://developers.google.com/search/docs/appearance/structured-data/job-posting)
  JSON-LD**, Google's structured-data schema for job listings, which enables rich
  search results (salary, location, remote badge, etc.). This only has any effect
  because the JSON is present in the server-rendered response — a client-injected
  version wouldn't reliably reach the crawler.

Authenticated pages (dashboards, apply flow) don't need any of this — there's no SEO
value in a page behind a login wall — so those are free to fetch client-side after
hydration, same as a normal SPA.

## Project layout

**Public, SSR pages** — real content in the first response, no login required:
- `pages/index.vue` — home/hero
- `pages/jobs/index.vue` — job search/listing, filters synced to the URL query string, real numbered pagination (`components/Pagination.vue`)
- `pages/jobs/[id].vue` — job detail, `useSeoMeta` + `JobPosting` JSON-LD
- `pages/companies/index.vue`, `pages/companies/[id].vue` — public company directory + profile
- `pages/login.vue`, `pages/register.vue`

**Authenticated dashboards** (`ssr: false` — no SEO value behind a login wall, so these fetch client-side after hydration):
- `pages/dashboard/candidate/` — overview, applications, saved jobs, profile (resume upload)
- `pages/dashboard/employer/` — overview, job listings (create/edit/publish/close/delete), applicants (status + private notes), company profile (logo upload), analytics
- `pages/dashboard/admin/` — platform stats, user management, cross-company job moderation

**Shared design system** (`components/`) — a small component library rather than every page hand-rolling Tailwind strings:
- `BaseButton`, `BaseInput`, `BaseTextarea`, `BaseSelect`, `Card`, `Badge`, `Alert`, `EmptyState`, `Spinner`
- `ToastContainer` + `composables/useToast.ts` — global success/error feedback for mutations
- `Pagination`, `BookmarkButton`, `NotificationBell`, `ApplyToJobBox`, `ThemeToggle`, `LocaleSwitcher`
- `ViewsLineChart`, `ApplicationStatusChart` — hand-rolled SVG/CSS charts (no charting library) for the employer analytics page

**API layer** (`composables/use*Api.ts`) — one typed `$fetch` wrapper per backend feature area (`useJobsApi`, `useApplicationsApi`, `useCompaniesApi`, `useCandidatesApi`, `useNotificationsApi`, `useSavedJobsApi`, `useAdminApi`, `useAuthApi`), backed by `useAuthFetch` (attaches the JWT, retries once through refresh-token rotation on a 401) and mirrored `types/*.ts` DTOs.

**i18n / theming**:
- `i18n/locales/{en,ar}.json`, `i18n.config.ts` (custom 6-form Arabic plural rule) — full English/Arabic UI with real RTL layout (Tailwind logical properties, not just a text-direction flip), `hreflang` alternates via `useLocaleHead()`
- `@nuxtjs/color-mode` — cookie-persisted dark/light theme, correct in the very first SSR response (no flash), toggled via `ThemeToggle.vue`

**Auth**: `stores/auth.ts` (Pinia, persisted to `localStorage`, hydrated client-side) + `middleware/auth.ts` + `composables/useRequireRole.ts` — deliberately client-only, unlike the cookie-backed theme/locale state, since dashboard pages have no SEO value to protect.

`error.vue` is the custom error page (used for the job-not-found 404 case).

## Setup

```bash
pnpm install
```

Copy `.env.example` to `.env` if you need to point the app at anything other
than the defaults (`http://localhost:5000/api` for the API, matching its
`http` launch profile — this sidesteps the API's self-signed HTTPS dev
certificate, which server-side `fetch` calls during SSR won't trust by
default). Both variables already fall back to sane local defaults, so this
step is optional:

```bash
cp .env.example .env
```

## Development

```bash
pnpm run dev
```

Runs on `http://localhost:3000`. The API's CORS policy (`Cors:AllowedOrigins` in
`JobBoard.Api`'s `appsettings.json`) already allows this origin.

## Production

```bash
pnpm run build
pnpm run preview   # or: node .output/server/index.mjs
```

## Testing

```bash
pnpm run test
```

Vitest, via `@nuxt/test-utils` (auto-imports and the i18n plugin resolve
correctly inside tests, not just the running app). `tests/` covers the
non-trivial pure logic rather than every file - the Arabic CLDR plural rule,
the toast queue's push/auto-dismiss behavior, and `Pagination.vue`'s
ellipsis-collapsing window. Runs in CI on every push/PR alongside the build.

## Tooling notes

- **pnpm, not npm.** Faster installs and a shared content-addressable store instead
  of npm's flat `node_modules` duplication — noticeably lighter on disk across
  multiple Node projects. `onlyBuiltDependencies` in `package.json` explicitly
  allow-lists `esbuild`'s postinstall script, since pnpm blocks arbitrary package
  postinstall scripts by default (a supply-chain safety feature npm doesn't have).
- **Nuxt 3, not 4.** `nuxi init` defaults to the latest Nuxt (4.5 at the time this
  was scaffolded), whose newest patch requires Node `^22.19.0 || ^24.11.0 || >=26.0.0`.
  The dev environment runs Node 22.16, below that floor. Pinning to the latest
  Nuxt **3** (`^3.21.0`) avoided either forcing a Node upgrade or pinning to an
  older, less battle-tested Nuxt 4 patch — Nuxt 3 needs only Node `>=22.12.0` and
  has everything this project uses (SSR, `useAsyncData`, `useSeoMeta`). Moving to
  Nuxt 4 later is a well-documented, largely mechanical migration.
- **`@nuxtjs/i18n` pinned to `9.5.6`, not the latest major.** The default install
  (v10) pulls in `vue-router@5`, which conflicts with this project's Pinia 2 /
  Nuxt 3 stack (built against `vue-router@4`). v9.5.6 depends on `vue-router@^4`,
  matching Nuxt 3's own router.
- **`@nuxt/fonts`** self-hosts the Google Font declared in `tailwind.config.ts`
  at build time — real `.woff2` files ship from this app's own origin, no
  runtime request to Google's CDN.
