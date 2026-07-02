using ResumeHub.Application.Abstractions;
using ResumeHub.Infrastructure.Auth;
using Xunit;

namespace ResumeHub.Tests.Auth;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    // Produced by the real Microsoft.AspNetCore.Identity PasswordHasher (v3: SHA512,
    // 100k iterations) for the password "Password123!". Guards the legacy verification
    // path used to migrate existing users transparently.
    private const string IdentityV3Hash =
        "AQAAAAIAAYagAAAAEHG6zyJ9n92pUA80I5gVxXYi0TW/ePGV6fu5E8NZuKMQT4O28Zft7aAF6xoqHQs5Rg==";

    [Fact]
    public void Hash_produces_bcrypt_hash()
    {
        var hash = _hasher.Hash("Password123!");
        Assert.StartsWith("$2", hash);
    }

    [Fact]
    public void Verify_bcrypt_roundtrip_succeeds()
    {
        var hash = _hasher.Hash("Password123!");
        Assert.Equal(PasswordVerify.Success, _hasher.Verify(hash, "Password123!"));
    }

    [Fact]
    public void Verify_bcrypt_wrong_password_fails()
    {
        var hash = _hasher.Hash("Password123!");
        Assert.Equal(PasswordVerify.Failed, _hasher.Verify(hash, "wrong-password"));
    }

    [Fact]
    public void Verify_legacy_identity_hash_signals_rehash()
    {
        Assert.Equal(
            PasswordVerify.SuccessRehashNeeded,
            _hasher.Verify(IdentityV3Hash, "Password123!"));
    }

    [Fact]
    public void Verify_legacy_identity_hash_wrong_password_fails()
    {
        Assert.Equal(PasswordVerify.Failed, _hasher.Verify(IdentityV3Hash, "wrong-password"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-base64-and-not-bcrypt!!")]
    public void Verify_garbage_hash_fails(string hash)
    {
        Assert.Equal(PasswordVerify.Failed, _hasher.Verify(hash, "Password123!"));
    }
}
