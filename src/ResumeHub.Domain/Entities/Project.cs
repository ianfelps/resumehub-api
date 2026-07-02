namespace ResumeHub.Domain.Entities;

/// <summary>
/// A highlighted project in the user's inventory.
/// </summary>
public class Project : OwnedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? RepoUrl { get; set; }
    public DateOnly? Date { get; set; }
}
