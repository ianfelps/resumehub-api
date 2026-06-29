namespace ResumeHub.Domain.Entities;

/// <summary>
/// A course / certification entry in the user's inventory.
/// </summary>
public class Course : OwnedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public DateOnly? CompletionDate { get; set; }
    public string? CertificateUrl { get; set; }
}
