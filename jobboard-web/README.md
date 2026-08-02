# jobboard-web

Nuxt 4 frontend for JobBoard, consuming the ASP.NET Core API in [`../JobBoard`](../JobBoard).

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

App code lives under `app/` (Nuxt 4's default `srcDir`), so `~/` resolves there — `app/components`, `app/composables`, `app/stores`, `app/types`, etc. `i18n/` stays at the repo root (outside `app/`, per `@nuxtjs/i18n`'s own convention, since translations are used both server- and client-side), alongside `public/`, `tests/`, and the config files (`nuxt.config.ts`, `tailwind.config.ts`, `vitest.config.ts`).

**Public, SSR pages** — real content in the first response, no login required:
- `app/pages/index.vue` — home/hero
- `app/pages/jobs/index.vue` — job search/listing, filters synced to the URL query string, real numbered pagination (`app/components/Pagination.vue`)
- `app/pages/jobs/[id].vue` — job detail, `useSeoMeta` + `JobPosting` JSON-LD, an SSR-fetched "similar jobs" section (same type or location, ranked by best match then recency)
- `app/pages/companies/index.vue`, `app/pages/companies/[id].vue` — public company directory + profile
- `app/pages/login.vue`, `app/pages/register.vue`

**Authenticated dashboards** (`ssr: false` — no SEO value behind a login wall, so these fetch client-side after hydration):
- `app/pages/dashboard/candidate/` — overview, applications, saved jobs, profile (resume upload)
- `app/pages/dashboard/employer/` — overview, job listings (create/edit/publish/close/delete), applicants (status + private notes), company profile (logo upload), analytics
- `app/pages/dashboard/admin/` — platform stats, user management, cross-company job moderation

**Shared design system** (`app/components/`) — a small component library rather than every page hand-rolling Tailwind strings:
- `BaseButton`, `BaseInput`, `BaseTextarea`, `BaseSelect`, `Card`, `Badge`, `Alert`, `EmptyState`, `Spinner`
- `ToastContainer` + `composables/useToast.ts` — global success/error feedback for mutations
- `Pagination`, `BookmarkButton`, `NotificationBell`, `ApplyToJobBox`, `ThemeToggle`, `LocaleSwitcher`
- `ViewsLineChart`, `ApplicationStatusChart` — hand-rolled SVG/CSS charts (no charting library) for the employer analytics page

**API layer** (`app/composables/use*Api.ts`) — one typed `$fetch` wrapper per backend feature area (`useJobsApi`, `useApplicationsApi`, `useCompaniesApi`, `useCandidatesApi`, `useNotificationsApi`, `useSavedJobsApi`, `useAdminApi`, `useAuthApi`), backed by `useAuthFetch` (attaches the JWT, retries once through refresh-token rotation on a 401) and mirrored `app/types/*.ts` DTOs.

**i18n / theming**:
- `i18n/locales/{en,ar}.json`, `i18n.config.ts` (custom 6-form Arabic plural rule) — full English/Arabic UI with real RTL layout (Tailwind logical properties, not just a text-direction flip), `hreflang` alternates via `useLocaleHead()`
- `@nuxtjs/color-mode` — cookie-persisted dark/light theme, correct in the very first SSR response (no flash), toggled via `ThemeToggle.vue`

**Auth**: `app/stores/auth.ts` (Pinia, persisted to `localStorage`, hydrated client-side) + `app/middleware/auth.ts` + `composables/useRequireRole.ts` — deliberately client-only, unlike the cookie-backed theme/locale state, since dashboard pages have no SEO value to protect.

`app/error.vue` is the custom error page (used for the job-not-found 404 case).

## Setup

Requires Node `^22.19.0 || ^24.11.0 || >=26.0.0` (Nuxt 4.5's floor). If you're on an older Node and have pnpm installed, its built-in runtime manager can fetch and switch to a compatible version without needing nvm:

```bash
pnpm env use --global 24.18.1   # or: pnpm runtime set node 24.18.1 -g, on newer pnpm
node --version
```

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
- **Nuxt 4** (`^4.5.1`), on the `app/` `srcDir` convention — moved off the initial
  Nuxt 3 scaffold once Node was upgraded to satisfy Nuxt 4.5's `^22.19.0 ||
  ^24.11.0 || >=26.0.0` floor. The module ecosystem had already started requiring
  it anyway (`@nuxt/fonts`, `@nuxtjs/i18n`'s latest majors both depend on
  `@nuxt/kit ^4.x`), so staying on Nuxt 3 was starting to mean losing the ability
  to update anything.
- **Pinia `^4.0.2`** (up from 2.x) and **`@nuxtjs/i18n` `^10.6.0`** (up from the
  `9.5.6` pin) came along with the Nuxt 4 move — `@pinia/nuxt@1.x` requires
  Pinia 4, and `@nuxtjs/i18n@10.x` is built against `vue-router@^5`, which Nuxt 4
  itself now uses (the old pin to `9.5.6` existed specifically to avoid v10's
  `vue-router@5` conflicting with the Nuxt 3 stack's `vue-router@4` — no longer
  an issue once both moved to Nuxt 4 together).
- **`@nuxt/fonts`** self-hosts the Google Font declared in `tailwind.config.ts`
  at build time — real `.woff2` files ship from this app's own origin, no
  runtime request to Google's CDN.
