using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Infrastructure.Services;

public class JobExpiryService : IJobExpiryService
{
    private readonly IApplicationDbContext _db;

    public JobExpiryService(IApplicationDbContext db)
    {
        _db = db;
    }

    // ExecuteUpdateAsync issues a single SQL UPDATE - no need to load rows into
    // memory just to flip one column on what could be a large batch of listings.
    public async Task CloseExpiredJobsAsync()
    {
        var now = DateTime.UtcNow;
        await _db.JobListings
            .Where(j => j.Status == JobStatus.Published && j.ExpiresAt != null && j.ExpiresAt <= now)
            .ExecuteUpdateAsync(setters => setters.SetProperty(j => j.Status, JobStatus.Closed));
    }
}
