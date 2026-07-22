using JobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBoard.Infrastructure.Persistence.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.HasOne(a => a.JobListing)
            .WithMany(j => j.Applications)
            .HasForeignKey(a => a.JobListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.CandidateProfile)
            .WithMany(c => c.Applications)
            .HasForeignKey(a => a.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // A candidate can only apply once per listing - enforced at the DB level, not just in the handler.
        builder.HasIndex(a => new { a.JobListingId, a.CandidateProfileId }).IsUnique();
    }
}
