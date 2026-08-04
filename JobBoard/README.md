# JobBoard API (ASP.NET Core backend)

Backend for a job board / marketplace: employers post listings, candidates
apply, both sides get a dashboard. Built as the ASP.NET Core half of a
full-stack portfolio project paired with a Nuxt.js frontend.

## Architecture

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

- **Domain** has zero package dependencies - just POCOs and enums.
- **Application** depends only on `IApplicationDbContext`, never on EF Core's
  `DbContext` directly, so business logic is persistence-agnostic and testable
  without a real database.
- **Infrastructure** implements those interfaces (Postgres via EF Core, JWT via
  `System.IdentityModel.Tokens.Jwt`, password hashing via BCrypt).
- Every use case (register, create job listing, apply to job, ...) is a MediatR
  command/query + handler under `Features/`, with a co-located FluentValidation
  validator where input needs validating. This is the CQRS pattern - it keeps
  each use case in one file instead of spreading logic across a fat service class.
- `Directory.Build.props` (repo root of this project) sets `TargetFramework`/
  `ImplicitUsings`/`Nullable` once for all 6 projects via MSBuild's directory-walk-up
  convention, instead of repeating them in every `.csproj`.

## Getting started

Requires the .NET 8 SDK and PostgreSQL (or swap the Npgsql package reference
for `Microsoft.EntityFrameworkCore.SqlServer` if you'd rather use SQL Server).

```bash
# from the repo root
dotnet restore

# set the JWT secret + connection string via user-secrets instead of committing them
cd src/JobBoard.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 48)"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=jobboard;Username=postgres;Password=postgres"

# create the initial migration and database
dotnet tool install --global dotnet-ef   # first time only
dotnet ef migrations add InitialCreate --project ../JobBoard.Infrastructure --startup-project .
dotnet ef database update --project ../JobBoard.Infrastructure --startup-project .

dotnet run
```

Swagger UI opens automatically at `/swagger` in development.

## What's fully implemented vs. scaffolded

**Fully implemented** (real handlers, validators, EF configs):
- Auth: register, login, refresh token rotation, BCrypt hashing, JWT issuing,
  a dev-only seeded Admin account (self-registering as Admin is blocked)
- Job listings: create, update, close, publish-a-draft, soft-delete, public
  search/filter/pagination (output-cached), detail page, distinct-locations
  list (backs the frontend's location filter), "my listings" for the owning
  employer, view tracking (a cheap `ViewCount` counter plus a timestamped
  `JobView` log for the analytics chart), and an hourly Hangfire recurring job
  that auto-closes listings once their optional `ExpiresAt` date passes
- Companies: create, update, fetch by id, public list/search, logo upload,
  employer analytics (30-day view trend, application-status breakdown, top
  listings) scoped to the caller's own company
- Candidate profiles: fetch/update the profile auto-created at registration,
  resume upload (5 MB cap, PDF/DOCX)
- Applications: apply, list mine (candidate), list for a listing (employer),
  update status, private employer-only notes on an application (never
  returned to the candidate)
- Notifications: in-app feed + mark-as-read, created when a candidate applies
  (notifies the employer) and when an employer changes an application's
  status (notifies the candidate)
- Saved jobs: candidates can bookmark/unbookmark a listing and list their
  saved jobs; save/unsave are idempotent by design
- Admin panel: platform-wide stats, paginated user list with
  activate/deactivate (an admin can't deactivate their own account), and
  cross-company job listing moderation (soft-delete any listing, not just
  employer-owned ones)
- Background jobs via Hangfire (Postgres-backed, dashboard at `/hangfire`,
  gated to localhost-only requests): outbound emails are queued
  (`BackgroundJob.Enqueue<IEmailService>`) instead of awaited inline so a slow
  mail provider can't fail a request, plus the job-expiry recurring job above
- Cross-cutting: per-IP rate limiting on the auth endpoints, a `/health`
  check (Postgres connectivity), and short-lived output caching on the public
  job/company reads
- Test coverage: `tests/JobBoard.Application.UnitTests` (handlers against EF
  Core InMemory, prioritizing ownership/authorization checks and the trickier
  logic like the analytics zero-fill, the expiry bulk-update, and the
  similar-jobs ranking) and `tests/JobBoard.Api.IntegrationTests`
  (`WebApplicationFactory` over real HTTP, Hangfire storage swapped to
  in-memory so tests never touch a real Postgres instance) - run both with
  `dotnet test` from the repo root

**Intentionally left as a next step**:
- Refresh tokens are single-slot per user (one active token). For multi-device
  login, add a `RefreshToken` table instead of two columns on `User`.
- `IEmailService` just logs (queued via Hangfire, but the provider itself is a
  stub). Wire it to a real provider (SendGrid/SES) before deploying.
- `IFileStorageService` writes to local disk. Add an S3/Azure Blob
  implementation behind the same interface before deploying to an ephemeral host.
- No frontend linting (ESLint/Prettier) - not there yet, doesn't block local
  development. (Docker is covered — see the root README's "Docker" section.)

## API surface

| Endpoint | Auth | Notes |
|---|---|---|
| `POST /api/auth/register` | - | candidate or employer |
| `POST /api/auth/login` | - | rate-limited per IP |
| `POST /api/auth/refresh` | - | rotates the refresh token, rate-limited per IP |
| `GET /api/jobs` | - | keyword/location/type/remote/salary filters + pagination, output-cached |
| `GET /api/jobs/{id}` | - | output-cached |
| `GET /api/jobs/locations` | - | distinct locations across published listings |
| `GET /api/jobs/{id}/similar` | - | up to 4 other published listings matching type/location, output-cached |
| `GET /api/jobs/mine` | Employer | includes Draft/Closed, excludes soft-deleted |
| `POST /api/jobs` | Employer | optional `ExpiresAt` |
| `PUT /api/jobs/{id}` | Employer | |
| `POST /api/jobs/{id}/close` | Employer | |
| `POST /api/jobs/{id}/publish` | Employer | publishes a draft listing |
| `DELETE /api/jobs/{id}` | Employer, Admin | soft-delete (`JobStatus.Deleted`); admins can delete listings they don't own |
| `POST /api/jobs/{id}/view` | - | fire-and-forget view ping, fired client-side only |
| `GET /api/companies` | - | keyword search + pagination |
| `POST /api/companies` | Employer | |
| `GET /api/companies/{id}` | - | |
| `PUT /api/companies/{id}` | Employer | |
| `GET /api/companies/mine` | Employer | |
| `GET /api/companies/mine/analytics` | Employer | 30-day view trend, status breakdown, top listings |
| `POST /api/companies/{id}/logo` | Employer | 2 MB cap, PNG/JPEG/SVG |
| `GET /api/candidates/me` | Candidate | |
| `PUT /api/candidates/me` | Candidate | |
| `POST /api/candidates/me/resume` | Candidate | 5 MB cap, PDF/DOCX |
| `POST /api/applications` | Candidate | |
| `GET /api/applications/mine` | Candidate | |
| `GET /api/applications/job/{jobListingId}` | Employer | |
| `PUT /api/applications/{id}/status` | Employer | |
| `PUT /api/applications/{id}/note` | Employer | private note, never exposed to the candidate |
| `GET /api/notifications` | Any authenticated | most recent 20 |
| `PUT /api/notifications/{id}/read` | Any authenticated | |
| `GET /api/saved-jobs` | Candidate | |
| `GET /api/saved-jobs/ids` | Candidate | lightweight id list for bookmark-toggle state |
| `POST /api/saved-jobs/{jobId}` | Candidate | idempotent |
| `DELETE /api/saved-jobs/{jobId}` | Candidate | idempotent |
| `GET /api/admin/stats` | Admin | |
| `GET /api/admin/users` | Admin | |
| `PUT /api/admin/users/{id}/active` | Admin | can't deactivate your own account |
| `GET /api/admin/jobs` | Admin | cross-company, optional status filter |
| `GET /health` | - | Postgres connectivity |
| `/hangfire` | localhost only | background job dashboard |
