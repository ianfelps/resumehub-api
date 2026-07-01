using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResumeHub.Domain.Entities;

namespace ResumeHub.Infrastructure.Persistence.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(120).IsRequired();
        b.Property(x => x.Headline).HasMaxLength(200);
        b.Property(x => x.Summary).HasMaxLength(4000);
        b.HasIndex(x => x.Slug).IsUnique();
        b.HasIndex(x => x.UserId);
        b.HasOne(x => x.User).WithMany(u => u.Profiles)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProfileExperienceConfiguration : IEntityTypeConfiguration<ProfileExperience>
{
    public void Configure(EntityTypeBuilder<ProfileExperience> b)
    {
        b.HasKey(x => new { x.ProfileId, x.ExperienceId });
        b.HasOne(x => x.Profile).WithMany(p => p.Experiences)
            .HasForeignKey(x => x.ProfileId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Experience).WithMany()
            .HasForeignKey(x => x.ExperienceId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProfileProjectConfiguration : IEntityTypeConfiguration<ProfileProject>
{
    public void Configure(EntityTypeBuilder<ProfileProject> b)
    {
        b.HasKey(x => new { x.ProfileId, x.ProjectId });
        b.HasOne(x => x.Profile).WithMany(p => p.Projects)
            .HasForeignKey(x => x.ProfileId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Project).WithMany()
            .HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProfileSkillConfiguration : IEntityTypeConfiguration<ProfileSkill>
{
    public void Configure(EntityTypeBuilder<ProfileSkill> b)
    {
        b.HasKey(x => new { x.ProfileId, x.SkillId });
        b.HasOne(x => x.Profile).WithMany(p => p.Skills)
            .HasForeignKey(x => x.ProfileId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Skill).WithMany()
            .HasForeignKey(x => x.SkillId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProfileLanguageConfiguration : IEntityTypeConfiguration<ProfileLanguage>
{
    public void Configure(EntityTypeBuilder<ProfileLanguage> b)
    {
        b.HasKey(x => new { x.ProfileId, x.LanguageId });
        b.HasOne(x => x.Profile).WithMany(p => p.Languages)
            .HasForeignKey(x => x.ProfileId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Language).WithMany()
            .HasForeignKey(x => x.LanguageId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProfileEducationConfiguration : IEntityTypeConfiguration<ProfileEducation>
{
    public void Configure(EntityTypeBuilder<ProfileEducation> b)
    {
        b.HasKey(x => new { x.ProfileId, x.EducationId });
        b.HasOne(x => x.Profile).WithMany(p => p.Education)
            .HasForeignKey(x => x.ProfileId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Education).WithMany()
            .HasForeignKey(x => x.EducationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProfileCourseConfiguration : IEntityTypeConfiguration<ProfileCourse>
{
    public void Configure(EntityTypeBuilder<ProfileCourse> b)
    {
        b.HasKey(x => new { x.ProfileId, x.CourseId });
        b.HasOne(x => x.Profile).WithMany(p => p.Courses)
            .HasForeignKey(x => x.ProfileId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Course).WithMany()
            .HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
    }
}
