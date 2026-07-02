using Microsoft.EntityFrameworkCore;
using ResumeHub.Application.Abstractions;
using ResumeHub.Application.Common;
using ResumeHub.Application.Dtos;
using ResumeHub.Domain.Entities;

namespace ResumeHub.Application.Services;

public interface IExperienceService : IOwnedCrudService<ExperienceRequest, ExperienceRequest, ExperienceResponse>;
public interface IProjectService : IOwnedCrudService<ProjectRequest, ProjectRequest, ProjectResponse>;
public interface ISkillService : IOwnedCrudService<SkillRequest, SkillRequest, SkillResponse>;
public interface ILanguageService : IOwnedCrudService<LanguageRequest, LanguageRequest, LanguageResponse>;
public interface IEducationService : IOwnedCrudService<EducationRequest, EducationRequest, EducationResponse>;
public interface ICourseService : IOwnedCrudService<CourseRequest, CourseRequest, CourseResponse>;

public class ExperienceService(IApplicationDbContext db, ICurrentUser user)
    : OwnedCrudService<Experience, ExperienceRequest, ExperienceRequest, ExperienceResponse>(db, user),
      IExperienceService
{
    protected override DbSet<Experience> Set => Db.Experiences;

    protected override IQueryable<Experience> OrderListing(IQueryable<Experience> q)
        => q.OrderByDescending(e => e.StartDate);

    protected override Experience FromCreate(ExperienceRequest d) => new()
    {
        Company = d.Company, Role = d.Role, Location = d.Location,
        StartDate = d.StartDate, EndDate = d.EndDate, Description = d.Description
    };

    protected override void ApplyUpdate(ExperienceRequest d, Experience e)
    {
        e.Company = d.Company; e.Role = d.Role; e.Location = d.Location;
        e.StartDate = d.StartDate; e.EndDate = d.EndDate; e.Description = d.Description;
    }

    protected override ExperienceResponse ToResponse(Experience e) =>
        new(e.Id, e.Company, e.Role, e.Location, e.StartDate, e.EndDate, e.Description);
}

public class ProjectService(IApplicationDbContext db, ICurrentUser user)
    : OwnedCrudService<Project, ProjectRequest, ProjectRequest, ProjectResponse>(db, user),
      IProjectService
{
    protected override DbSet<Project> Set => Db.Projects;

    protected override IQueryable<Project> OrderListing(IQueryable<Project> q)
        => q.OrderByDescending(e => e.Date);

    protected override Project FromCreate(ProjectRequest d) => new()
    {
        Name = d.Name, Description = d.Description, Url = d.Url,
        RepoUrl = d.RepoUrl, Date = d.Date
    };

    protected override void ApplyUpdate(ProjectRequest d, Project e)
    {
        e.Name = d.Name; e.Description = d.Description; e.Url = d.Url;
        e.RepoUrl = d.RepoUrl; e.Date = d.Date;
    }

    protected override ProjectResponse ToResponse(Project e) =>
        new(e.Id, e.Name, e.Description, e.Url, e.RepoUrl, e.Date);
}

public class SkillService(IApplicationDbContext db, ICurrentUser user)
    : OwnedCrudService<Skill, SkillRequest, SkillRequest, SkillResponse>(db, user),
      ISkillService
{
    protected override DbSet<Skill> Set => Db.Skills;

    protected override IQueryable<Skill> OrderListing(IQueryable<Skill> q)
        => q.OrderBy(e => e.Category).ThenBy(e => e.Name);

    protected override Skill FromCreate(SkillRequest d) => new()
    {
        Name = d.Name, Category = d.Category, Level = d.Level
    };

    protected override void ApplyUpdate(SkillRequest d, Skill e)
    {
        e.Name = d.Name; e.Category = d.Category; e.Level = d.Level;
    }

    protected override SkillResponse ToResponse(Skill e) =>
        new(e.Id, e.Name, e.Category, e.Level);
}

public class LanguageService(IApplicationDbContext db, ICurrentUser user)
    : OwnedCrudService<Language, LanguageRequest, LanguageRequest, LanguageResponse>(db, user),
      ILanguageService
{
    protected override DbSet<Language> Set => Db.Languages;

    protected override IQueryable<Language> OrderListing(IQueryable<Language> q)
        => q.OrderBy(e => e.Name);

    protected override Language FromCreate(LanguageRequest d) => new()
    {
        Name = d.Name, Proficiency = d.Proficiency
    };

    protected override void ApplyUpdate(LanguageRequest d, Language e)
    {
        e.Name = d.Name; e.Proficiency = d.Proficiency;
    }

    protected override LanguageResponse ToResponse(Language e) =>
        new(e.Id, e.Name, e.Proficiency);
}

public class EducationService(IApplicationDbContext db, ICurrentUser user)
    : OwnedCrudService<Education, EducationRequest, EducationRequest, EducationResponse>(db, user),
      IEducationService
{
    protected override DbSet<Education> Set => Db.Education;

    protected override IQueryable<Education> OrderListing(IQueryable<Education> q)
        => q.OrderByDescending(e => e.StartDate);

    protected override Education FromCreate(EducationRequest d) => new()
    {
        Institution = d.Institution, Degree = d.Degree, Field = d.Field,
        StartDate = d.StartDate, EndDate = d.EndDate
    };

    protected override void ApplyUpdate(EducationRequest d, Education e)
    {
        e.Institution = d.Institution; e.Degree = d.Degree; e.Field = d.Field;
        e.StartDate = d.StartDate; e.EndDate = d.EndDate;
    }

    protected override EducationResponse ToResponse(Education e) =>
        new(e.Id, e.Institution, e.Degree, e.Field, e.StartDate, e.EndDate);
}

public class CourseService(IApplicationDbContext db, ICurrentUser user)
    : OwnedCrudService<Course, CourseRequest, CourseRequest, CourseResponse>(db, user),
      ICourseService
{
    protected override DbSet<Course> Set => Db.Courses;

    protected override IQueryable<Course> OrderListing(IQueryable<Course> q)
        => q.OrderByDescending(e => e.CompletionDate);

    protected override Course FromCreate(CourseRequest d) => new()
    {
        Name = d.Name, Provider = d.Provider,
        CompletionDate = d.CompletionDate, CertificateUrl = d.CertificateUrl
    };

    protected override void ApplyUpdate(CourseRequest d, Course e)
    {
        e.Name = d.Name; e.Provider = d.Provider;
        e.CompletionDate = d.CompletionDate; e.CertificateUrl = d.CertificateUrl;
    }

    protected override CourseResponse ToResponse(Course e) =>
        new(e.Id, e.Name, e.Provider, e.CompletionDate, e.CertificateUrl);
}
