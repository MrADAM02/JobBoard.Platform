using JobBoard.Api.Middleware;
using JobBoard.Application;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using JobBoard.Infrastructure;
using JobBoard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Convenience only: applies pending migrations automatically in dev.
    // Never do this in production - use `dotnet ef database update` in your deploy pipeline instead.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Dev-only admin seed: Admin can't be created via /register (see RegisterCommandValidator),
    // so this is the only way to get one. Never do this in production.
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Needed for WebApplicationFactory<Program> in JobBoard.Api.IntegrationTests
// (top-level statements generate an internal Program class by default).
public partial class Program { }
