namespace ResumeHub.Domain.Entities;

/// <summary>
/// Platform user. Owns the whole career inventory and the curated profiles.
/// Plain POCO — no ASP.NET Identity base type.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Login credentials. NormalizedEmail (upper-invariant) backs a unique index
    // and case-insensitive lookups.
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? FullName { get; set; }
    public string? Headline { get; set; }
    public string? Location { get; set; }

    // Whether the account e-mail is surfaced on the public resume. Phone number
    // always shows when set.
    public bool ShowEmailOnResume { get; set; }

    // Social / contact links surfaced on the public resume.
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? WebsiteUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Brute-force lockout (managed by AuthService, replacing Identity's lockout).
    public int AccessFailedCount { get; set; }
    public DateTime? LockoutEnd { get; set; }

    // Refresh token (single active session model for the MVP).
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public ICollection<Experience> Experiences { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];
    public ICollection<Skill> Skills { get; set; } = [];
    public ICollection<Language> Languages { get; set; } = [];
    public ICollection<Education> Education { get; set; } = [];
    public ICollection<Course> Courses { get; set; } = [];
    public ICollection<Profile> Profiles { get; set; } = [];
}
