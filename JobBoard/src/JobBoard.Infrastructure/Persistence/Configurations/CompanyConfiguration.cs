using JobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBoard.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Website).HasMaxLength(500);
        builder.Property(c => c.LogoUrl).HasMaxLength(500);
        builder.Property(c => c.Location).HasMaxLength(200);

        // One employer owns exactly one company (see User -> Company one-to-one in UserConfiguration).
        builder.HasIndex(c => c.OwnerUserId).IsUnique();
    }
}
