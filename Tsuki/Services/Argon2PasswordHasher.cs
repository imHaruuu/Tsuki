using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using Tsuki.Models;

namespace Tsuki.Services;

/// <summary>
/// Argon2id password hasher with transparent PBKDF2 → Argon2id migration.
///
/// Migration strategy (zero-downtime, backward-compatible):
///   1. New passwords   → hashed with Argon2id immediately.
///   2. Legacy passwords (PBKDF2 from ASP.NET Identity v2/v3) → still verified
///      correctly on login; returns SuccessRehashNeeded to signal Identity to
///      automatically re-hash with Argon2id before the session is saved.
///   3. No manual data migration or downtime needed — each user is silently
///      upgraded on their next successful login.
///
/// Security parameters (OWASP 2024 recommended Argon2id minimum):
///   - Memory:      19 MiB  (19456 KB)
///   - Iterations:  2
///   - Parallelism: 1
///   - Salt:        128-bit (16 bytes), cryptographically random per hash
///   - Output:      256-bit (32 bytes)
///   - Comparison:  CryptographicOperations.FixedTimeEquals (timing-safe)
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher<ApplicationUser>
{
    // ── Format marker ───────────────────────────────────────────────────────
    // All Argon2id hashes start with this prefix so we can distinguish them
    // from the Base64-encoded PBKDF2 hashes produced by the default hasher.
    private const string Argon2Prefix = "$argon2id$v1$";

    // ── Argon2id parameters (OWASP 2024 minimum configuration) ─────────────
    private const int MemorySize   = 19456; // 19 MiB
    private const int Iterations   = 2;     // time cost
    private const int Parallelism  = 1;     // degree of parallelism
    private const int SaltSize     = 16;    // bytes  → 128-bit salt
    private const int HashSize     = 32;    // bytes  → 256-bit output

    // Fallback to the default Identity hasher for legacy PBKDF2 verification
    private readonly IPasswordHasher<ApplicationUser> _legacyHasher
        = new PasswordHasher<ApplicationUser>();

    // ── Hash ────────────────────────────────────────────────────────────────
    /// <summary>Produces a new Argon2id hash for the given plain-text password.</summary>
    public string HashPassword(ApplicationUser user, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        using var argon2 = CreateArgon2(password, salt);
        var hash = argon2.GetBytes(HashSize);

        // Encode as: $argon2id$v1$<base64salt>$<base64hash>
        return $"{Argon2Prefix}{Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    // ── Verify ──────────────────────────────────────────────────────────────
    /// <summary>
    /// Verifies a stored hash against a provided plain-text password.
    /// Returns SuccessRehashNeeded for legacy PBKDF2 hashes so Identity
    /// will transparently upgrade the stored hash on the next save.
    /// </summary>
    public PasswordVerificationResult VerifyHashedPassword(
        ApplicationUser user, string hashedPassword, string providedPassword)
    {
        if (hashedPassword.StartsWith(Argon2Prefix, StringComparison.Ordinal))
        {
            // ── Path A: already an Argon2id hash ───────────────────────────
            return VerifyArgon2Hash(hashedPassword, providedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }

        // ── Path B: legacy PBKDF2 hash (Identity v2 or v3) ─────────────────
        // Delegate verification to the built-in hasher, then signal rehash.
        var legacyResult = _legacyHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);

        return legacyResult switch
        {
            PasswordVerificationResult.Failed => PasswordVerificationResult.Failed,
            // SuccessRehashNeeded causes Identity's UserManager to call
            // HashPassword() with Argon2id and persist the new hash,
            // completing the silent per-user migration on next login.
            _ => PasswordVerificationResult.SuccessRehashNeeded
        };
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private static bool VerifyArgon2Hash(string hashedPassword, string providedPassword)
    {
        // Parse stored format: $argon2id$v1$<base64salt>$<base64hash>
        var payload = hashedPassword[Argon2Prefix.Length..];
        var parts   = payload.Split('$');
        if (parts.Length != 2) return false;

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt         = Convert.FromBase64String(parts[0]);
            expectedHash = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        using var argon2 = CreateArgon2(providedPassword, salt);
        var actualHash = argon2.GetBytes(HashSize);

        // Constant-time comparison — prevents timing-based side-channel attacks
        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }

    private static Argon2id CreateArgon2(string password, byte[] salt) =>
        new(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt                = salt,
            MemorySize          = MemorySize,
            Iterations          = Iterations,
            DegreeOfParallelism = Parallelism
        };
}
