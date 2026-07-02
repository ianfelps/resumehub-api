namespace ResumeHub.Domain.Entities;

/// <summary>
/// Base for every inventory item that belongs to a single user.
/// </summary>
public abstract class OwnedEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
