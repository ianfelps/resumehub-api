using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResumeHub.Domain.Entities;

namespace ResumeHub.Infrastructure.Persistence.Configurations;

public class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Company).HasMaxLength(200).IsRequired();
        b.Property(x => x.Role).HasMaxLength(200).IsRequired();
        b.Property(x => x.Location).HasMaxLength(200);
        b.Property(x => x.Description).HasMaxLength(4000);
        b.HasIndex(x => x.UserId);
        b.HasOne(x => x.User).WithMany(u => u.Experiences)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.Url).HasMaxLength(500);
        b.Property(x => x.RepoUrl).HasMaxLength(500);
        b.HasIndex(x => x.UserId);
        b.HasOne(x => x.User).WithMany(u => u.Projects)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Category).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Level).HasConversion<string>().HasMaxLength(30);
        b.HasIndex(x => x.UserId);
        b.HasOne(x => x.User).WithMany(u => u.Skills)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Proficiency).HasConversion<string>().HasMaxLength(30);
        b.HasIndex(x => x.UserId);
        b.HasOne(x => x.User).WithMany(u => u.Languages)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Institution).HasMaxLength(200).IsRequired();
        b.Property(x => x.Degree).HasMaxLength(200).IsRequired();
        b.Property(x => x.Field).HasMaxLength(200);
        b.HasIndex(x => x.UserId);
        b.HasOne(x => x.User).WithMany(u => u.Education)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Provider).HasMaxLength(200);
        b.Property(x => x.CertificateUrl).HasMaxLength(500);
        b.HasIndex(x => x.UserId);
        b.HasOne(x => x.User).WithMany(u => u.Courses)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
