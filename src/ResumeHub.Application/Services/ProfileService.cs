using Microsoft.EntityFrameworkCore;
using ResumeHub.Application.Abstractions;
using ResumeHub.Application.Common;
using ResumeHub.Application.Dtos;
using ResumeHub.Domain.Entities;

namespace ResumeHub.Application.Services;

public interface IProfileService
{
    Task<IReadOnlyList<ProfileResponse>> GetAllAsync();
    Task<ProfileResponse> GetByIdAsync(Guid id);
    Task<ProfileResponse> CreateAsync(ProfileRequest dto);
    Task<ProfileResponse> UpdateAsync(Guid id, ProfileRequest dto);
    Task DeleteAsync(Guid id);
    Task SetItemsAsync(Guid id, ProfileItemsRequest dto);
    Task<ProfileItemsResponse> GetItemsAsync(Guid id);
    Task<PublicResumeResponse> GetPublicBySlugAsync(string slug);
}

public class ProfileService(IApplicationDbContext db, ICurrentUser currentUser) : IProfileService
{
    public async Task<IReadOnlyList<ProfileResponse>> GetAllAsync()
    {
        var profiles = await db.Profiles
            .Where(p => p.UserId == currentUser.Id)
            .OrderByDescending(p => p.UpdatedAt)
            .AsNoTracking()
            .ToListAsync();
        return profiles.Select(ToResponse).ToList();
    }

    public async Task<ProfileResponse> GetByIdAsync(Guid id)
        => ToResponse(await FindOwnedAsync(id));

    public async Task<ProfileResponse> CreateAsync(ProfileRequest dto)
    {
        var slug = await EnsureUniqueSlugAsync(
            SlugGenerator.Slugify(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Name : dto.Slug),
            excludeProfileId: null);

        var profile = new Profile
        {
            UserId = currentUser.Id,
            Name = dto.Name,
            Slug = slug,
            Headline = dto.Headline,
            Summary = dto.Summary,
            IsPublic = dto.IsPublic
        };

        db.Profiles.Add(profile);
        await db.SaveChangesAsync();
        return ToResponse(profile);
    }

    public async Task<ProfileResponse> UpdateAsync(Guid id, ProfileRequest dto)
    {
        var profile = await FindOwnedAsync(id);

        var desiredSlug = SlugGenerator.Slugify(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Name : dto.Slug);
        if (desiredSlug != profile.Slug)
            profile.Slug = await EnsureUniqueSlugAsync(desiredSlug, excludeProfileId: profile.Id);

        profile.Name = dto.Name;
        profile.Headline = dto.Headline;
        profile.Summary = dto.Summary;
        profile.IsPublic = dto.IsPublic;

        await db.SaveChangesAsync();
        return ToResponse(profile);
    }

    public async Task DeleteAsync(Guid id)
    {
        var profile = await FindOwnedAsync(id);
        db.Profiles.Remove(profile);
        await db.SaveChangesAsync();
    }

    public async Task SetItemsAsync(Guid id, ProfileItemsRequest dto)
    {
        var profile = await db.Profiles
            .Include(p => p.Experiences)
            .Include(p => p.Projects)
            .Include(p => p.Skills)
            .Include(p => p.Languages)
            .Include(p => p.Education)
            .Include(p => p.Courses)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUser.Id)
            ?? throw new NotFoundException($"Perfil '{id}' não encontrado.");

        await ValidateOwnedIdsAsync(dto);

        profile.Experiences = Map(dto.Experiences, s => new ProfileExperience
        { ProfileId = id, ExperienceId = s.Id, DisplayOrder = s.DisplayOrder });
        profile.Projects = Map(dto.Projects, s => new ProfileProject
        { ProfileId = id, ProjectId = s.Id, DisplayOrder = s.DisplayOrder });
        profile.Skills = Map(dto.Skills, s => new ProfileSkill
        { ProfileId = id, SkillId = s.Id, DisplayOrder = s.DisplayOrder });
        profile.Languages = Map(dto.Languages, s => new ProfileLanguage
        { ProfileId = id, LanguageId = s.Id, DisplayOrder = s.DisplayOrder });
        profile.Education = Map(dto.Education, s => new ProfileEducation
        { ProfileId = id, EducationId = s.Id, DisplayOrder = s.DisplayOrder });
        profile.Courses = Map(dto.Courses, s => new ProfileCourse
        { ProfileId = id, CourseId = s.Id, DisplayOrder = s.DisplayOrder });

        await db.SaveChangesAsync();
    }

    public async Task<ProfileItemsResponse> GetItemsAsync(Guid id)
    {
        var profile = await db.Profiles
            .AsNoTracking()
            .Include(p => p.Experiences)
            .Include(p => p.Projects)
            .Include(p => p.Skills)
            .Include(p => p.Languages)
            .Include(p => p.Education)
            .Include(p => p.Courses)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUser.Id)
            ?? throw new NotFoundException($"Perfil '{id}' não encontrado.");

        return new ProfileItemsResponse(
            profile.Experiences.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProfileItemSelection(x.ExperienceId, x.DisplayOrder)).ToList(),
            profile.Projects.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProfileItemSelection(x.ProjectId, x.DisplayOrder)).ToList(),
            profile.Skills.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProfileItemSelection(x.SkillId, x.DisplayOrder)).ToList(),
            profile.Languages.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProfileItemSelection(x.LanguageId, x.DisplayOrder)).ToList(),
            profile.Education.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProfileItemSelection(x.EducationId, x.DisplayOrder)).ToList(),
            profile.Courses.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProfileItemSelection(x.CourseId, x.DisplayOrder)).ToList());
    }

    public async Task<PublicResumeResponse> GetPublicBySlugAsync(string slug)
    {
        var profile = await db.Profiles
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Experiences).ThenInclude(x => x.Experience)
            .Include(p => p.Projects).ThenInclude(x => x.Project)
            .Include(p => p.Skills).ThenInclude(x => x.Skill)
            .Include(p => p.Languages).ThenInclude(x => x.Language)
            .Include(p => p.Education).ThenInclude(x => x.Education)
            .Include(p => p.Courses).ThenInclude(x => x.Course)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublic)
            ?? throw new NotFoundException($"Perfil público '{slug}' não encontrado.");

        return new PublicResumeResponse(
            profile.Name,
            profile.Summary,
            new PublicOwner(
                profile.User?.FullName,
                profile.Headline ?? profile.User?.Headline,
                profile.User?.Location,
                profile.User?.LinkedInUrl,
                profile.User?.GitHubUrl,
                profile.User?.WebsiteUrl),
            profile.Experiences.OrderBy(x => x.DisplayOrder)
                .Select(x => new ExperienceResponse(x.Experience!.Id, x.Experience.Company,
                    x.Experience.Role, x.Experience.Location, x.Experience.StartDate,
                    x.Experience.EndDate, x.Experience.Description)).ToList(),
            profile.Projects.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProjectResponse(x.Project!.Id, x.Project.Name,
                    x.Project.Description, x.Project.Url, x.Project.RepoUrl, x.Project.Highlights)).ToList(),
            profile.Skills.OrderBy(x => x.DisplayOrder)
                .Select(x => new SkillResponse(x.Skill!.Id, x.Skill.Name, x.Skill.Category, x.Skill.Level)).ToList(),
            profile.Languages.OrderBy(x => x.DisplayOrder)
                .Select(x => new LanguageResponse(x.Language!.Id, x.Language.Name, x.Language.Proficiency)).ToList(),
            profile.Education.OrderBy(x => x.DisplayOrder)
                .Select(x => new EducationResponse(x.Education!.Id, x.Education.Institution,
                    x.Education.Degree, x.Education.Field, x.Education.StartDate, x.Education.EndDate)).ToList(),
            profile.Courses.OrderBy(x => x.DisplayOrder)
                .Select(x => new CourseResponse(x.Course!.Id, x.Course.Name, x.Course.Provider,
                    x.Course.CompletionDate, x.Course.CertificateUrl)).ToList());
    }

    // ---- helpers ----

    private static List<T> Map<T>(List<ProfileItemSelection>? items, Func<ProfileItemSelection, T> map)
        => items?.Select(map).ToList() ?? [];

    private async Task ValidateOwnedIdsAsync(ProfileItemsRequest dto)
    {
        await AssertOwnedAsync(db.Experiences, dto.Experiences, "experiência");
        await AssertOwnedAsync(db.Projects, dto.Projects, "projetos");
        await AssertOwnedAsync(db.Skills, dto.Skills, "habilidades");
        await AssertOwnedAsync(db.Languages, dto.Languages, "idiomas");
        await AssertOwnedAsync(db.Education, dto.Education, "formação");
        await AssertOwnedAsync(db.Courses, dto.Courses, "cursos");
    }

    private async Task AssertOwnedAsync<TEntity>(
        DbSet<TEntity> set, List<ProfileItemSelection>? selections, string label)
        where TEntity : OwnedEntity
    {
        if (selections is null || selections.Count == 0) return;

        var ids = selections.Select(s => s.Id).Distinct().ToList();
        var ownedCount = await set.CountAsync(e => ids.Contains(e.Id) && e.UserId == currentUser.Id);
        if (ownedCount != ids.Count)
            throw new NotFoundException(
                $"Um ou mais itens de {label} não foram encontrados no seu inventário.");
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug, Guid? excludeProfileId)
    {
        var slug = baseSlug;
        var suffix = 1;
        while (await db.Profiles.AnyAsync(p =>
            p.Slug == slug && (excludeProfileId == null || p.Id != excludeProfileId)))
        {
            slug = $"{baseSlug}-{++suffix}";
        }
        return slug;
    }

    private async Task<Profile> FindOwnedAsync(Guid id)
        => await db.Profiles.FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUser.Id)
            ?? throw new NotFoundException($"Perfil '{id}' não encontrado.");

    private static ProfileResponse ToResponse(Profile p) =>
        new(p.Id, p.Name, p.Slug, p.Headline, p.Summary, p.IsPublic, p.CreatedAt, p.UpdatedAt);
}
