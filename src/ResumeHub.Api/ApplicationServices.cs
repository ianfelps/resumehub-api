using ResumeHub.Api.Services;

namespace ResumeHub.Api;

/// <summary>
/// Registration of inventory + profile application services.
/// Kept separate so feature services are added in one obvious place.
/// </summary>
public static class ApplicationServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IExperienceService, ExperienceService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IEducationService, EducationService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IProfileService, ProfileService>();

        return services;
    }
}
