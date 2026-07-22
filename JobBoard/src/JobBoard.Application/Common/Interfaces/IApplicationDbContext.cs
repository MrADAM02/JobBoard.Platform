using JobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Common.Interfaces;

// The Application layer depends only on this abstraction, never on EF Core's
// DbContext directly or on Infrastructure. Keeps Application testable/persistence-agnostic.
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<CandidateProfile> CandidateProfiles { get; }
    DbSet<Company> Companies { get; }
    DbSet<JobListing> JobListings { get; }
    DbSet<JobApplication> JobApplications { get; }
    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
