using ResumeHub.Domain.Enums;

namespace ResumeHub.Domain.Entities;

/// <summary>
/// A technology, tool or soft skill the user masters.
/// </summary>
public class Skill : OwnedEntity
{
    public string Name { get; set; } = string.Empty;
    public SkillCategory Category { get; set; }
    public SkillLevel Level { get; set; }
}
