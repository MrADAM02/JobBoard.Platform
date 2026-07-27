using JobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBoard.Infrastructure.Persistence.Configurations;

public class JobViewConfiguration : IEntityTypeConfiguration<JobView>
{
    public void Configure(EntityTypeBuilder<JobView> builder)
    {
        builder.HasOne(v => v.JobListing)
            .WithMany()
            .HasForeignKey(v => v.JobListingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Backs the "views in the last 30 days, grouped by job and day" analytics query.
        builder.HasIndex(v => new { v.JobListingId, v.ViewedAt });
    }
}
