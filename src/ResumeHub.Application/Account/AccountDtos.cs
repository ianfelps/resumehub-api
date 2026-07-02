namespace ResumeHub.Application.Account;

/// <summary>The authenticated user's account/profile data (powers the resume owner line).</summary>
public record AccountResponse(
    string Email, string? FullName, string? Headline, string? Location,
    string? PhoneNumber, bool ShowEmailOnResume,
    string? LinkedInUrl, string? GitHubUrl, string? WebsiteUrl);

public record UpdateAccountRequest(
    string? FullName, string? Headline, string? Location,
    string? PhoneNumber, bool ShowEmailOnResume,
    string? LinkedInUrl, string? GitHubUrl, string? WebsiteUrl);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record DeleteAccountRequest(string Password);
