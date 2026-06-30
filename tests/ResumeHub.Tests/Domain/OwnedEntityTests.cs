using ResumeHub.Domain.Entities;
using Xunit;

namespace ResumeHub.Tests.Domain;

public class OwnedEntityTests
{
    [Fact]
    public void New_entity_gets_a_non_empty_id()
    {
        var experience = new Experience();
        Assert.NotEqual(Guid.Empty, experience.Id);
    }

    [Fact]
    public void New_entity_ids_are_unique()
    {
        var a = new Project();
        var b = new Project();
        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void New_entity_timestamps_default_to_utc_now()
    {
        var before = DateTime.UtcNow.AddSeconds(-5);
        var skill = new Skill();
        var after = DateTime.UtcNow.AddSeconds(5);

        Assert.InRange(skill.CreatedAt, before, after);
        Assert.InRange(skill.UpdatedAt, before, after);
    }
}
