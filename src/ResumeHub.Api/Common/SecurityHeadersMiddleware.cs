namespace ResumeHub.Api.Common;

/// <summary>
/// Adds baseline security response headers. This is a JSON API (no server-rendered HTML),
/// so the CSP is locked all the way down and framing is denied outright.
///
/// The one exception is the Scalar API reference UI (dev only): it is real HTML that pulls
/// its script bundle/fonts from a CDN, so <c>default-src 'none'</c> would blank the page.
/// Those paths get a scoped, permissive CSP instead.
/// </summary>
public class SecurityHeadersMiddleware(RequestDelegate next, IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "no-referrer";

        if (environment.IsDevelopment() && IsScalarUi(context.Request.Path))
        {
            headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "img-src 'self' data: https:; " +
                "connect-src 'self';";
        }
        else
        {
            headers["X-Frame-Options"] = "DENY";
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        }

        await next(context);
    }

    private static bool IsScalarUi(PathString path)
        => path.StartsWithSegments("/scalar") || path.StartsWithSegments("/openapi");
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();
}
