using System.Reflection;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<JobListing> JobListings => Set<JobListing>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Picks up every IEntityTypeConfiguration<T> in Persistence/Configurations automatically.
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}
