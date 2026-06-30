namespace ResumeHub.Application.Auth;

public record RegisterRequest(string Email, string Password, string? FullName);

public record LoginRequest(string Email, string Password);

public record RefreshRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt);
