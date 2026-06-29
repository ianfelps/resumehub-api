using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ResumeHub.Api.Auth;
using ResumeHub.Api.Common;

namespace ResumeHub.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddFluentValidationAutoValidation();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // Application services (inventory + profiles) are registered here.
        services.AddApplicationServices();

        AddJwtAuth(services, configuration);
        AddOpenApiWithBearer(services);

        return services;
    }

    private static void AddJwtAuth(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(section);
        var settings = section.Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt settings not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();
    }

    private static void AddOpenApiWithBearer(IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the JWT access token (without the 'Bearer ' prefix)."
                };
                document.Security ??= [];
                document.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
                return Task.CompletedTask;
            });
        });
    }
}
