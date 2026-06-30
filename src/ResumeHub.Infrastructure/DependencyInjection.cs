using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ResumeHub.Application.Abstractions;
using ResumeHub.Domain.Entities;
using ResumeHub.Infrastructure.Auth;
using ResumeHub.Infrastructure.Persistence;

namespace ResumeHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not configured.");

        services.AddDbContext<ResumeHubDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Expose the concrete DbContext through the Application port.
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ResumeHubDbContext>());

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<ResumeHubDbContext>();

        // JWT token signing port.
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
