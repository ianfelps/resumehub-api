using System.Security.Claims;

namespace ResumeHub.Api.Common;

public interface ICurrentUser
{
    Guid Id { get; }
}

/// <summary>Reads the authenticated user's id from the JWT claims on the current request.</summary>
public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid Id
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? accessor.HttpContext?.User.FindFirstValue("sub");
            return Guid.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedAccessException("No authenticated user in context.");
        }
    }
}
