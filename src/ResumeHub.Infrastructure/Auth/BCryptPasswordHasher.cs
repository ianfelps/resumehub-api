using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using ResumeHub.Application.Abstractions;

namespace ResumeHub.Infrastructure.Auth;

/// <summary>
/// Hashes new passwords with BCrypt. Also verifies legacy ASP.NET Identity PBKDF2
/// hashes (v2 and v3 formats) so existing users keep logging in; when a legacy hash
/// matches, <see cref="PasswordVerify.SuccessRehashNeeded"/> signals the caller to
/// re-hash and persist with BCrypt.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public PasswordVerify Verify(string hash, string password)
    {
        if (string.IsNullOrEmpty(hash))
            return PasswordVerify.Failed;

        // BCrypt hashes start with "$2a$"/"$2b$"/"$2y$".
        if (hash.StartsWith("$2", StringComparison.Ordinal))
            return BCrypt.Net.BCrypt.Verify(password, hash)
                ? PasswordVerify.Success
                : PasswordVerify.Failed;

        // Otherwise assume a legacy Identity PBKDF2 hash (base64).
        return VerifyIdentityHash(hash, password)
            ? PasswordVerify.SuccessRehashNeeded
            : PasswordVerify.Failed;
    }

    /// <summary>
    /// Verifies an ASP.NET Identity <c>PasswordHasher</c> hash. Mirrors the format
    /// produced by Microsoft.AspNetCore.Identity without depending on the package.
    /// </summary>
    private static bool VerifyIdentityHash(string hashedPassword, string password)
    {
        byte[] decoded;
        try
        {
            decoded = Convert.FromBase64String(hashedPassword);
        }
        catch (FormatException)
        {
            return false;
        }

        if (decoded.Length == 0)
            return false;

        return decoded[0] switch
        {
            0x00 => VerifyV2(decoded, password),
            0x01 => VerifyV3(decoded, password),
            _ => false,
        };
    }

    // Version 2: PBKDF2 with HMAC-SHA1, 128-bit salt, 256-bit subkey, 1000 iterations.
    private static bool VerifyV2(byte[] decoded, string password)
    {
        const int iterCount = 1000;
        const int saltSize = 128 / 8;
        const int subkeyLength = 256 / 8;

        if (decoded.Length != 1 + saltSize + subkeyLength)
            return false;

        var salt = new byte[saltSize];
        Buffer.BlockCopy(decoded, 1, salt, 0, saltSize);

        var expected = new byte[subkeyLength];
        Buffer.BlockCopy(decoded, 1 + saltSize, expected, 0, subkeyLength);

        var actual = Pbkdf2(password, salt, HashAlgorithmName.SHA1, iterCount, subkeyLength);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    // Version 3: header carries PRF, iteration count and salt length (big-endian uint32s).
    private static bool VerifyV3(byte[] decoded, string password)
    {
        if (decoded.Length < 13)
            return false;

        var prf = BinaryPrimitives.ReadUInt32BigEndian(decoded.AsSpan(1, 4));
        var iterCount = (int)BinaryPrimitives.ReadUInt32BigEndian(decoded.AsSpan(5, 4));
        var saltLength = (int)BinaryPrimitives.ReadUInt32BigEndian(decoded.AsSpan(9, 4));

        if (saltLength < 128 / 8 || iterCount <= 0)
            return false;

        var algorithm = prf switch
        {
            0 => HashAlgorithmName.SHA1,
            1 => HashAlgorithmName.SHA256,
            2 => HashAlgorithmName.SHA512,
            _ => default,
        };
        if (algorithm == default)
            return false;

        var subkeyLength = decoded.Length - 13 - saltLength;
        if (subkeyLength < 128 / 8)
            return false;

        var salt = new byte[saltLength];
        Buffer.BlockCopy(decoded, 13, salt, 0, saltLength);

        var expected = new byte[subkeyLength];
        Buffer.BlockCopy(decoded, 13 + saltLength, expected, 0, subkeyLength);

        var actual = Pbkdf2(password, salt, algorithm, iterCount, subkeyLength);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] Pbkdf2(
        string password, byte[] salt, HashAlgorithmName algorithm, int iterations, int outputBytes)
        => Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password), salt, iterations, algorithm, outputBytes);
}
