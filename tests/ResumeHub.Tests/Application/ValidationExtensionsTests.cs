using ResumeHub.Application.Validators;
using Xunit;

namespace ResumeHub.Tests.Application;

public class ValidationExtensionsTests
{
    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/path?q=1")]
    [InlineData("https://sub.domain.co/a/b")]
    public void BeHttpUrl_allows_http_and_https(string url)
    {
        Assert.True(ValidationExtensions.BeHttpUrl(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BeHttpUrl_allows_empty_for_optional_fields(string? url)
    {
        Assert.True(ValidationExtensions.BeHttpUrl(url));
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,<script>alert(1)</script>")]
    [InlineData("file:///etc/passwd")]
    [InlineData("ftp://example.com")]
    [InlineData("not a url")]
    [InlineData("//example.com")]
    public void BeHttpUrl_rejects_dangerous_or_relative_schemes(string url)
    {
        Assert.False(ValidationExtensions.BeHttpUrl(url));
    }

    [Theory]
    [InlineData("Senha123!")]
    [InlineData("a1@aaaaa")]
    [InlineData("P@ssw0rd")]
    public void IsStrongPassword_accepts_letter_digit_special_min8(string password)
    {
        Assert.True(ValidationExtensions.IsStrongPassword(password));
    }

    [Theory]
    [InlineData(null)]          // empty
    [InlineData("")]
    [InlineData("Ab1!")]        // too short
    [InlineData("abcdefg1")]    // no special
    [InlineData("abcdefg!")]    // no digit
    [InlineData("1234567!")]    // no letter
    public void IsStrongPassword_rejects_weak_passwords(string? password)
    {
        Assert.False(ValidationExtensions.IsStrongPassword(password));
    }
}
