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
- Job listings: create, update, close, soft-delete, public search/filter/pagination,
  detail page, "my listings" for the owning employer
- Companies: create, update, fetch by id
- Candidate profiles: fetch/update the profile auto-created at registration
- Applications: apply, list mine (candidate), list for a listing (employer),
  update status with an email notification stub
- Test coverage: `tests/JobBoard.Application.UnitTests` (handlers against EF Core
  InMemory) and `tests/JobBoard.Api.IntegrationTests` (`WebApplicationFactory`
  over real HTTP) - run both with `dotnet test` from the repo root

**Intentionally left as a next step** (the pattern is there to copy):
- Refresh tokens are single-slot per user (one active token). For multi-device
  login, add a `RefreshToken` table instead of two columns on `User`.
- `ViewCount` on `JobListing` exists but isn't incremented - that's a side
  effect and belongs in a command, not the `GetJobListingByIdQuery` (queries
  should stay read-only). Add a small `RecordJobViewCommand` fired from the
  frontend on page load.
- `IEmailService` just logs. Wire it to SendGrid/SES, and call it through
  Hangfire (`BackgroundJob.Enqueue<IEmailService>(s => s.SendAsync(...))`)
  instead of `await`-ing it inline, so a slow mail provider can't fail an
  application submission.
- `IFileStorageService` writes to local disk and exists but nothing calls it
  yet - resume/logo upload commands are the next slice. Add an S3/Azure Blob
  implementation behind the same interface before deploying to an ephemeral host.
- The `Notification` entity exists (DbSet included) but has no feature slice
  built on top of it yet - no commands/queries/controller.

## API surface

| Endpoint | Auth | Notes |
|---|---|---|
| `POST /api/auth/register` | - | candidate or employer |
| `POST /api/auth/login` | - | |
| `POST /api/auth/refresh` | - | rotates the refresh token |
| `GET /api/jobs` | - | keyword/location/type/remote/salary filters + pagination |
| `GET /api/jobs/{id}` | - | |
| `GET /api/jobs/mine` | Employer | includes Draft/Closed, excludes soft-deleted |
| `POST /api/jobs` | Employer | |
| `PUT /api/jobs/{id}` | Employer | |
| `POST /api/jobs/{id}/close` | Employer | |
| `DELETE /api/jobs/{id}` | Employer | soft-delete (`JobStatus.Deleted`) |
| `POST /api/companies` | Employer | |
| `GET /api/companies/{id}` | - | |
| `PUT /api/companies/{id}` | Employer | |
| `GET /api/candidates/me` | Candidate | |
| `PUT /api/candidates/me` | Candidate | |
| `POST /api/applications` | Candidate | |
| `GET /api/applications/mine` | Candidate | |
| `GET /api/applications/job/{jobListingId}` | Employer | |
| `PUT /api/applications/{id}/status` | Employer | |
