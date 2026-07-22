using JobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBoard.Infrastructure.Persistence.Configurations;

public class JobListingConfiguration : IEntityTypeConfiguration<JobListing>
{
    public void Configure(EntityTypeBuilder<JobListing> builder)
    {
        builder.Property(j => j.Title).IsRequired().HasMaxLength(200);
        builder.Property(j => j.SalaryMin).HasColumnType("decimal(12,2)");
        builder.Property(j => j.SalaryMax).HasColumnType("decimal(12,2)");

        builder.HasOne(j => j.Company)
            .WithMany(c => c.JobListings)
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Speeds up the public listings query (status + recency are filtered/sorted on every request).
        builder.HasIndex(j => new { j.Status, j.PublishedAt });
    }
}
