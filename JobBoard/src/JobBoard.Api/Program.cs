using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.Dashboard;
using JobBoard.Api.Middleware;
using JobBoard.Application;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using JobBoard.Infrastructure;
using JobBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Each layer registers its own services - Program.cs just wires them together.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "JobBoard API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    // Named policy so the Nuxt frontend (localhost:3000 in dev) can call the API
    // with credentials while everything else stays locked out.
    options.AddPolicy("NuxtFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Fixed-window limiter on auth endpoints (login/register/refresh) - these are
// anonymous and credential-guessing/registration-spam targets, unlike the rest
// of the API which is already gated by JWT auth or read-only. Partitioned by
// caller IP via AddPolicy (not AddFixedWindowLimiter, which would create a
// single global window shared by every caller - one abusive client would lock
// out everyone else, confirmed by testing: hammering /login from one client
// immediately 429'd a completely unrelated /register call).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        }));
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

// Short-lived cache on the public, unauthenticated job/company reads - pairs
// with the SSR story as a layered-caching talking point (SSR for crawlers,
// output cache for repeat API hits). Never applied to authenticated actions
// like GetMyJobListings - opt-in per action via [OutputCache], not global.
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("PublicReads", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .SetVaryByQuery("keyword", "location", "jobType", "remoteOnly", "minSalary", "pageNumber", "pageSize"));
});

var app = builder.Build();

// Applies pending migrations on every boot, in every environment - idempotent
// (EF only applies what's not already applied), and without this a fresh
// Production database has no schema-provisioning path at all. IsRelational()
// guards this against JobBoard.Api.IntegrationTests' CustomWebApplicationFactory,
// which swaps in the EF Core InMemory provider - Migrate() throws on anything non-relational.
using (var migrationScope = app.Services.CreateScope())
{
    var migrationDb = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (migrationDb.Database.IsRelational())
    {
        migrationDb.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Dev-only admin seed: Admin can't be created via /register (see RegisterCommandValidator),
    // so this is the only way to get one. Never do this in production - a
    // well-known admin password has no business existing on a public deployment;
    // promote a real registered account to Admin manually instead (see
    // README.md#deploying-to-a-server).
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!db.Users.Any(u => u.Role == UserRole.Admin))
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        db.Users.Add(new User
        {
            Email = "admin@jobboard.local",
            PasswordHash = passwordHasher.Hash("Admin123!"),
            Role = UserRole.Admin
        });
        db.SaveChanges();
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Skipped in dev: the frontend deliberately calls the API over plain HTTP
// (see jobboard-web/README.md) to avoid the browser rejecting the self-signed
// HTTPS dev certificate. Redirecting here - which happens whenever the HTTPS
// endpoint is bound, e.g. the "https" launch profile or Visual Studio's default -
// would silently break every request: the http->https redirect either gets
// blocked by CORS (cross-origin redirect) or hits the untrusted cert.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles(); // serves /uploads for locally-stored resumes/logos

app.UseCors("NuxtFrontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapControllers();
app.MapHealthChecks("/health");

// Dashboard has its own auth model (not JWT-based, since it's opened as a plain
// browser tab) - gated to localhost-only requests, Hangfire's documented pattern
// for securing the dashboard without building a separate cookie-auth flow.
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new LocalRequestsOnlyAuthorizationFilter()]
});

// JobListing.ExpiresAt existed on the entity long before anything set or read
// it - this is what finally makes it do something, closing published listings
// once they pass their expiry date so they drop off the public /jobs listing.
RecurringJob.AddOrUpdate<IJobExpiryService>(
    "close-expired-jobs", s => s.CloseExpiredJobsAsync(), Cron.Hourly);

app.Run();

// Needed for WebApplicationFactory<Program> in JobBoard.Api.IntegrationTests
// (top-level statements generate an internal Program class by default).
public partial class Program { }
