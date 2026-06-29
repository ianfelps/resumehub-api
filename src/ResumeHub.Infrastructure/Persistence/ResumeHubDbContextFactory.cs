using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ResumeHub.Infrastructure.Persistence;

/// <summary>
/// Design-time factory so `dotnet ef` can build the context without running the API host.
/// Reads the connection string from the API project's appsettings / user-secrets.
/// </summary>
public class ResumeHubDbContextFactory : IDesignTimeDbContextFactory<ResumeHubDbContext>
{
    public ResumeHubDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "ResumeHub.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetFullPath(basePath))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<ResumeHubDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' not found for design-time context.");

        var options = new DbContextOptionsBuilder<ResumeHubDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ResumeHubDbContext(options);
    }
}
