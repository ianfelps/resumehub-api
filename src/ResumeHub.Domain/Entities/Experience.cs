namespace ResumeHub.Domain.Entities;

/// <summary>
/// A job / professional experience entry in the user's inventory.
/// </summary>
public class Experience : OwnedEntity
{
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Description { get; set; }
}
