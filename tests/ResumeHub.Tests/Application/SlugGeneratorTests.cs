using ResumeHub.Application.Common;
using Xunit;

namespace ResumeHub.Tests.Application;

public class SlugGeneratorTests
{
    [Theory]
    [InlineData("Senior .NET Developer!", "senior-net-developer")]
    [InlineData("  Backend   Engineer  ", "backend-engineer")]
    [InlineData("Programação Avançada", "programacao-avancada")]
    [InlineData("C# & SQL", "c-sql")]
    public void Slugify_produces_url_safe_slug(string input, string expected)
    {
        Assert.Equal(expected, SlugGenerator.Slugify(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!@#$%")]
    public void Slugify_falls_back_to_random_token_when_no_usable_chars(string input)
    {
        var slug = SlugGenerator.Slugify(input);

        Assert.False(string.IsNullOrWhiteSpace(slug));
        Assert.Equal(8, slug.Length);
        Assert.Matches("^[a-z0-9]+$", slug);
    }

    [Fact]
    public void Slugify_has_no_leading_or_trailing_hyphens()
    {
        var slug = SlugGenerator.Slugify("--Lead Architect--");
        Assert.Equal("lead-architect", slug);
    }
}
