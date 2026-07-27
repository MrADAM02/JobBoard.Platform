using JobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBoard.Infrastructure.Persistence.Configurations;

public class SavedJobConfiguration : IEntityTypeConfiguration<SavedJob>
{
    public void Configure(EntityTypeBuilder<SavedJob> builder)
    {
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.JobListing)
            .WithMany()
            .HasForeignKey(s => s.JobListingId)
            .OnDelete(DeleteBehavior.Cascade);

        // A user can only save a given job once - enforced at the DB level.
        builder.HasIndex(s => new { s.UserId, s.JobListingId }).IsUnique();
    }
}
