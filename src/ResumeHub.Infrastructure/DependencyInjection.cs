using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ResumeHub.Domain.Entities;
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

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ResumeHubDbContext>();

        return services;
    }
}
