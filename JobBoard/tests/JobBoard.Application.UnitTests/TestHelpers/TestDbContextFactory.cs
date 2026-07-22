using JobBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.UnitTests.TestHelpers;

// Reuses the real ApplicationDbContext (entity configs and all) against EF Core's
// InMemory provider, so handler tests exercise the same model as production - just
// swapping Npgsql for InMemory. Each call gets its own isolated database.
public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
