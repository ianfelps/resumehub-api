using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using ResumeHub.Application.Account;
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
        QuestPDF.Settings.License = LicenseType.Community;

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();

        services.AddScoped<IExperienceService, ExperienceService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IEducationService, EducationService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IProfileService, ProfileService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Default FluentValidation messages (NotEmpty, EmailAddress, …) in PT-BR.
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("pt-BR");

        return services;
    }
}
