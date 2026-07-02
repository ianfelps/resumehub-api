namespace ResumeHub.Application.Abstractions;

/// <summary>Outcome of verifying a plaintext password against a stored hash.</summary>
public enum PasswordVerify
{
    /// <summary>The password does not match.</summary>
    Failed,

    /// <summary>The password matches and the stored hash is up to date.</summary>
    Success,

    /// <summary>
    /// The password matches but the stored hash uses a legacy format (e.g. the old
    /// ASP.NET Identity PBKDF2 hashes). The caller should re-hash and persist.
    /// </summary>
    SuccessRehashNeeded,
}

/// <summary>
/// Password hashing port. The concrete implementation lives in Infrastructure and
/// hashes new passwords with BCrypt while still verifying legacy Identity hashes.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    PasswordVerify Verify(string hash, string password);
}
