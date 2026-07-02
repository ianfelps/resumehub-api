using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResumeHub.Domain.Entities;

namespace ResumeHub.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");
        b.HasKey(x => x.Id);

        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();
        b.HasIndex(x => x.NormalizedEmail).IsUnique();

        b.Property(x => x.PasswordHash).IsRequired();
        b.Property(x => x.PhoneNumber).HasMaxLength(40);

        b.Property(x => x.FullName).HasMaxLength(200);
        b.Property(x => x.Headline).HasMaxLength(200);
        b.Property(x => x.Location).HasMaxLength(200);
        b.Property(x => x.LinkedInUrl).HasMaxLength(300);
        b.Property(x => x.GitHubUrl).HasMaxLength(300);
        b.Property(x => x.WebsiteUrl).HasMaxLength(300);

        b.Property(x => x.RefreshTokenHash).HasMaxLength(64);
    }
}
