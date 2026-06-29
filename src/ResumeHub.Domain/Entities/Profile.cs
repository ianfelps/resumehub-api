namespace ResumeHub.Domain.Entities;

/// <summary>
/// A curated selection of inventory items tailored for a specific opportunity.
/// Exposed publicly through its <see cref="Slug"/> when <see cref="IsPublic"/> is true.
/// </summary>
public class Profile : OwnedEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public bool IsPublic { get; set; }

    public ICollection<ProfileExperience> Experiences { get; set; } = [];
    public ICollection<ProfileProject> Projects { get; set; } = [];
    public ICollection<ProfileSkill> Skills { get; set; } = [];
    public ICollection<ProfileLanguage> Languages { get; set; } = [];
    public ICollection<ProfileEducation> Education { get; set; } = [];
    public ICollection<ProfileCourse> Courses { get; set; } = [];
}
