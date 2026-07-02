using Microsoft.EntityFrameworkCore;
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
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    // Brute-force lockout: lock an account after repeated failed logins.
    private const int MaxFailedAccessAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var normalizedEmail = Normalize(request.Email);

        var exists = await db.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail);
        if (exists)
            throw new ConflictException("E-mail já cadastrado.");

        var user = new User
        {
            Email = request.Email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName
        };

        db.Users.Add(user);
        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var normalizedEmail = Normalize(request.Email);
        var user = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

        // Uniform failure for unknown email or wrong password (no user enumeration).
        if (user is null)
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (user.LockoutEnd is not null && user.LockoutEnd > DateTime.UtcNow)
            throw new UnauthorizedAccessException(
                "Conta temporariamente bloqueada por excesso de tentativas. Tente novamente mais tarde.");

        var verify = passwordHasher.Verify(user.PasswordHash, request.Password);
        if (verify == PasswordVerify.Failed)
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= MaxFailedAccessAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                user.AccessFailedCount = 0;
            }
            await db.SaveChangesAsync();
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        // Successful login: clear failure state and transparently upgrade legacy hashes.
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        if (verify == PasswordVerify.SuccessRehashNeeded)
            user.PasswordHash = passwordHasher.Hash(request.Password);

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request)
    {
        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var user = await db.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == hash);

        if (user is null || user.RefreshTokenExpiresAt is null
            || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Token de atualização inválido ou expirado.");

        return await IssueTokensAsync(user);
    }

    private async Task<AuthResponse> IssueTokensAsync(User user)
    {
        var accessToken = tokenService.CreateAccessToken(user);
        var refreshToken = tokenService.CreateRefreshToken();

        user.RefreshTokenHash = tokenService.HashRefreshToken(refreshToken);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);
        await db.SaveChangesAsync();

        return new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes));
    }

    private static string Normalize(string email) => email.Trim().ToUpperInvariant();
}
