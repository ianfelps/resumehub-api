using System.Text;
using System.Threading.RateLimiting;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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

    /// <summary>Stricter rate-limit policy for unauthenticated auth endpoints (brute-force defense).</summary>
    public const string AuthRateLimitPolicy = "auth";

    /// <summary>Max request body size (bytes). The API only accepts JSON — no uploads — so 1 MB is generous.</summary>
    private const long MaxRequestBodyBytes = 1 * 1024 * 1024;

    public static IServiceCollection AddApi(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddHealthChecks();
        services.AddFluentValidationAutoValidation();

        // JSON-only API: cap the request body so a single oversized payload can't exhaust memory.
        services.Configure<KestrelServerOptions>(options =>
            options.Limits.MaxRequestBodySize = MaxRequestBodyBytes);

        // Behind a PaaS/reverse proxy that terminates TLS, honor X-Forwarded-* so the app sees the
        // real client IP (used by rate limiting) and scheme (avoids HTTPS-redirect loops). The proxy
        // is untrusted-by-default in ASP.NET Core; clearing the known lists trusts the platform edge.
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        AddRateLimiting(services);

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

    private static void AddRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global fallback: per-client-IP fixed window. Protects every endpoint from floods.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ClientKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            // Tight window for auth endpoints (login/register/refresh) to blunt brute-force.
            options.AddPolicy(AuthRateLimitPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ClientKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new { error = "Muitas requisições. Tente novamente em instantes." }, token);
            };
        });

        // Partition by the real client IP (ForwardedHeaders middleware populates RemoteIpAddress
        // from X-Forwarded-For when running behind a proxy). Falls back to a shared bucket.
        static string ClientKey(HttpContext context) =>
            context.Connection.RemoteIpAddress?.ToString()
            ?? context.Request.Headers.Host.ToString()
            ?? "unknown";
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
