using Microsoft.EntityFrameworkCore;
using ResumeHub.Domain.Entities;

namespace ResumeHub.Application.Abstractions;

/// <summary>
/// Persistence port exposed to the Application layer. The concrete EF Core
/// <c>ResumeHubDbContext</c> (Infrastructure) implements it, so use cases depend
/// on this abstraction instead of on Infrastructure directly.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Experience> Experiences { get; }
    DbSet<Project> Projects { get; }
    DbSet<Skill> Skills { get; }
    DbSet<Language> Languages { get; }
    DbSet<Education> Education { get; }
    DbSet<Course> Courses { get; }

    DbSet<Profile> Profiles { get; }
    DbSet<ProfileExperience> ProfileExperiences { get; }
    DbSet<ProfileProject> ProfileProjects { get; }
    DbSet<ProfileSkill> ProfileSkills { get; }
    DbSet<ProfileLanguage> ProfileLanguages { get; }
    DbSet<ProfileEducation> ProfileEducations { get; }
    DbSet<ProfileCourse> ProfileCourses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
