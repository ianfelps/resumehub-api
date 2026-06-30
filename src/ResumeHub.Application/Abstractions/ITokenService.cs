using ResumeHub.Domain.Entities;

namespace ResumeHub.Application.Abstractions;

/// <summary>
/// JWT/token port. The concrete signing implementation lives in Infrastructure.
/// </summary>
public interface ITokenService
{
    string CreateAccessToken(ApplicationUser user);
    string CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
