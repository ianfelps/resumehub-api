using ResumeHub.Domain.Enums;

namespace ResumeHub.Domain.Entities;

/// <summary>
/// A spoken language and the user's proficiency.
/// </summary>
public class Language : OwnedEntity
{
    public string Name { get; set; } = string.Empty;
    public LanguageProficiency Proficiency { get; set; }
}
