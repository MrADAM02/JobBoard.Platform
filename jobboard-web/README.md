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

- `pages/jobs/index.vue` — SSR job search/listing, filters synced to the URL query string
- `pages/jobs/[id].vue` — SSR job detail, `useSeoMeta` + `JobPosting` JSON-LD
- `composables/useJobsApi.ts` — typed `$fetch` wrapper around `JobBoard.Api`
- `types/job.ts` — TypeScript types mirroring the API's DTO shapes
- `error.vue` — custom error page (used for the job-not-found 404 case)

## Setup

```bash
npm install
```

Point the app at your running `JobBoard.Api` instance (defaults to
`http://localhost:5000/api`, matching the API's `http` launch profile — this
sidesteps the API's self-signed HTTPS dev certificate, which server-side `fetch`
calls during SSR won't trust by default):

```bash
# .env (optional - only needed to override the default)
NUXT_PUBLIC_API_BASE=http://localhost:5000/api
```

## Development

```bash
npm run dev
```

Runs on `http://localhost:3000`. The API's CORS policy (`Cors:AllowedOrigins` in
`JobBoard.Api`'s `appsettings.json`) already allows this origin.

## Production

```bash
npm run build
npm run preview   # or: node .output/server/index.mjs
```
