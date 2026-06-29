namespace ResumeHub.Domain.Entities;

/// <summary>
/// Join entities linking a <see cref="Profile"/> to the inventory items it selects,
/// carrying the display order chosen for the assembled resume.
/// </summary>
public class ProfileExperience
{
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }
    public Guid ExperienceId { get; set; }
    public Experience? Experience { get; set; }
    public int DisplayOrder { get; set; }
}

public class ProfileProject
{
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public int DisplayOrder { get; set; }
}

public class ProfileSkill
{
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }
    public Guid SkillId { get; set; }
    public Skill? Skill { get; set; }
    public int DisplayOrder { get; set; }
}

public class ProfileLanguage
{
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }
    public Guid LanguageId { get; set; }
    public Language? Language { get; set; }
    public int DisplayOrder { get; set; }
}

public class ProfileEducation
{
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }
    public Guid EducationId { get; set; }
    public Education? Education { get; set; }
    public int DisplayOrder { get; set; }
}

public class ProfileCourse
{
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }
    public Guid CourseId { get; set; }
    public Course? Course { get; set; }
    public int DisplayOrder { get; set; }
}
