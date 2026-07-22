using JobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBoard.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired();

        builder.HasOne(u => u.CandidateProfile)
            .WithOne(c => c.User)
            .HasForeignKey<CandidateProfile>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Company)
            .WithOne(c => c.OwnerUser)
            .HasForeignKey<Company>(c => c.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
