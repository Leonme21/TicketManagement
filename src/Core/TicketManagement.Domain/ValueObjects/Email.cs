using System.Text.RegularExpressions;
using TicketManagement.Domain.Common;

namespace TicketManagement.Domain.ValueObjects;

/// <summary>
/// 🔥 BIG TECH LEVEL: Email Value Object with comprehensive validation
/// Encapsulates email validation logic and prevents primitive obsession
/// </summary>
public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly HashSet<string> ProhibitedDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "tempmail.com",
        "throwaway.email",
        "guerrillamail.com",
        "10minutemail.com",
        "mailinator.com"
    };

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Factory method to create an Email Value Object with validation
    /// </summary>
    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email>.Invalid("Email cannot be empty");

        email = email.Trim().ToLowerInvariant();

        // RFC 5321: Maximum email length is 254 characters
        if (email.Length > 254)
            return Result<Email>.Invalid("Email must not exceed 254 characters");

        if (!EmailRegex.IsMatch(email))
            return Result<Email>.Invalid("Invalid email format");

        // Validate domain is not in prohibited list
        var domain = email.Split('@')[1];
        if (ProhibitedDomains.Contains(domain))
            return Result<Email>.Invalid($"Email domain '{domain}' is not allowed");

        return Result<Email>.Success(new Email(email));
    }

    public override string ToString() => Value;

    /// <summary>
    /// Implicit conversion to string for convenience
    /// </summary>
    public static implicit operator string(Email email) => email.Value;
}
