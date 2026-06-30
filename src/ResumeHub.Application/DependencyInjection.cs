using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ResumeHub.Application.Auth;
using ResumeHub.Application.Services;

namespace ResumeHub.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers use-case services, the auth service and FluentValidation validators.
    /// Ports (IApplicationDbContext, ITokenService, IStorageService, ICurrentUser) are
    /// wired up by the Infrastructure and Api layers.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IExperienceService, ExperienceService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IEducationService, EducationService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IProfileService, ProfileService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
