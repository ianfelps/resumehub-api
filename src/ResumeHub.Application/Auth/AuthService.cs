using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ResumeHub.Application.Abstractions;
using ResumeHub.Application.Common;
using ResumeHub.Domain.Entities;

namespace ResumeHub.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(RefreshRequest request);
}

public class AuthService(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new ConflictException("E-mail já cadastrado.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ConflictException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        // Uniform failure for unknown email or wrong password (no user enumeration).
        if (user is null)
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (userManager.SupportsUserLockout && await userManager.IsLockedOutAsync(user))
            throw new UnauthorizedAccessException(
                "Conta temporariamente bloqueada por excesso de tentativas. Tente novamente mais tarde.");

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            // Count the failure toward lockout (UserManager doesn't do this automatically,
            // unlike SignInManager which we intentionally avoid to stay cookie-free).
            if (userManager.SupportsUserLockout)
                await userManager.AccessFailedAsync(user);
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        if (userManager.SupportsUserLockout && await userManager.GetAccessFailedCountAsync(user) > 0)
            await userManager.ResetAccessFailedCountAsync(user);

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request)
    {
        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var user = userManager.Users.FirstOrDefault(u => u.RefreshTokenHash == hash);

        if (user is null || user.RefreshTokenExpiresAt is null
            || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Token de atualização inválido ou expirado.");

        return await IssueTokensAsync(user);
    }

    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user)
    {
        var accessToken = tokenService.CreateAccessToken(user);
        var refreshToken = tokenService.CreateRefreshToken();

        user.RefreshTokenHash = tokenService.HashRefreshToken(refreshToken);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);
        await userManager.UpdateAsync(user);

        return new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes));
    }
}
