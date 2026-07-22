using JobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBoard.Infrastructure.Persistence.Configurations;

public class CandidateProfileConfiguration : IEntityTypeConfiguration<CandidateProfile>
{
    public void Configure(EntityTypeBuilder<CandidateProfile> builder)
    {
        builder.Property(c => c.FullName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Headline).HasMaxLength(200);
        builder.Property(c => c.ResumeUrl).HasMaxLength(500);

        // One candidate has exactly one profile (see User -> CandidateProfile one-to-one in UserConfiguration).
        builder.HasIndex(c => c.UserId).IsUnique();
    }
}
