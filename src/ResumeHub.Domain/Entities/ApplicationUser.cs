using Microsoft.AspNetCore.Identity;

namespace ResumeHub.Domain.Entities;

/// <summary>
/// Platform user. Owns the whole career inventory and the curated profiles.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName { get; set; }
    public string? Headline { get; set; }
    public string? Location { get; set; }

    // Social / contact links surfaced on the public resume.
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? WebsiteUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

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
