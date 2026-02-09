using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Infrastructure.Identity;

/// <summary>
/// ?? BIG TECH LEVEL: Secure password hashing service using BCrypt
/// - Work factor of 12 provides good security/performance balance
/// - BCrypt is resistant to rainbow table attacks
/// - Automatically handles salt generation
/// </summary>
public sealed class PasswordHasher
{
    // ?? SECURITY: Work factor 12 is recommended for production
    // Each increment doubles the computation time
    // 12 takes ~250ms on modern hardware, providing good security
    private const int WorkFactor = 12;

    /// <summary>
    /// Hashes a password using BCrypt with secure work factor
    /// </summary>
    /// <param name="password">Plain text password to hash</param>
    /// <returns>BCrypt hash with embedded salt</returns>
    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    /// <summary>
    /// Verifies a password against a BCrypt hash
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="passwordHash">BCrypt hash to verify against</param>
    /// <returns>True if password matches, false otherwise</returns>
    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            // Invalid hash format - return false instead of throwing
            return false;
        }
    }

    /// <summary>
    /// Checks if a password hash needs to be upgraded to current work factor
    /// </summary>
    /// <param name="passwordHash">Existing BCrypt hash</param>
    /// <returns>True if hash should be upgraded</returns>
    public bool NeedsUpgrade(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return false;

        try
        {
            // BCrypt hash format: $2a$WF$... where WF is the work factor
            var parts = passwordHash.Split('$');
            if (parts.Length >= 3 && int.TryParse(parts[2], out var existingWorkFactor))
            {
                return existingWorkFactor < WorkFactor;
            }
        }
        catch
        {
            // Invalid hash format
        }

        return false;
    }
}
