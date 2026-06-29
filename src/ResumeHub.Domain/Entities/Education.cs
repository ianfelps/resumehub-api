namespace ResumeHub.Domain.Entities;

/// <summary>
/// Academic background entry in the user's inventory.
/// </summary>
public class Education : OwnedEntity
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string? Field { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
