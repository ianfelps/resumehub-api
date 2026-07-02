using ResumeHub.Api;
using ResumeHub.Api.Common;
using ResumeHub.Infrastructure;
using Scalar.AspNetCore;

// Load .env (searching up from the working dir) into the process environment before
// the host reads configuration. Real secrets stay out of source control; see .env.example.
DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// PaaS platforms (Render, Railway, …) inject the listening port via PORT. Bind to it when
// present; otherwise fall back to ASPNETCORE_URLS (defaults to :8080 in the Docker image).
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApi(builder.Configuration);

var app = builder.Build();

// Must run first: rewrites scheme/remote IP from X-Forwarded-* so everything downstream
// (HTTPS redirect, rate limiting, HSTS) sees the real client request behind a proxy.
app.UseForwardedHeaders();

// Liveness probe for the platform health check. Registered before HTTPS redirection so an
// internal HTTP probe returns 200 instead of a 307, and it is not subject to rate limiting.
app.UseHealthChecks("/health");

app.UseExceptionHandler();
app.UseSecurityHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // UI at /scalar
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors(ResumeHub.Api.DependencyInjection.WebCorsPolicy);

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
