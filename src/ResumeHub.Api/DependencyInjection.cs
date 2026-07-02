using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ResumeHub.Api.Common;
using ResumeHub.Application;
using ResumeHub.Application.Auth;
using ResumeHub.Application.Common;

namespace ResumeHub.Api;

public static class DependencyInjection
{
    /// <summary>CORS policy that allows the SPA frontend (direct browser calls).</summary>
    public const string WebCorsPolicy = "web";

    public static IServiceCollection AddApi(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddFluentValidationAutoValidation();

        // The Next.js client calls this API directly from the browser. Allow its
        // origin (configurable via Cors__WebOrigin) so preflight/CORS succeeds.
        var webOrigin = configuration["Cors:WebOrigin"] ?? "http://localhost:3000";
        services.AddCors(options =>
            options.AddPolicy(WebCorsPolicy, policy => policy
                .WithOrigins(webOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("X-Page-Count")));

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // ICurrentUser is an Application port implemented here (reads JWT claims).
        services.AddScoped<ICurrentUser, CurrentUser>();

        // Use cases, auth service and validators.
        services.AddApplication();

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
