using System;
using TicketManagement.Domain.Common;

namespace TicketManagement.Domain.Entities;

/// <summary>
/// ✅ Entidad RefreshToken para implementar patrón de tokens seguros
/// </summary>
public class RefreshToken : BaseEntity
{
    // ==================== CONSTANTS ====================
    public const int DefaultExpiryDays = 7;
    public const int TokenLength = 64; // Base64 encoded 48 bytes = 64 chars
    
    // ==================== CONSTRUCTORS ====================
    private RefreshToken() { } // EF Core

    private RefreshToken(string token, int userId, DateTime expiresAt, string? deviceInfo = null)
    {
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
        DeviceInfo = deviceInfo;
        IsActive = true;
    }
    
    // ==================== FACTORY METHOD ====================
    
    /// <summary>
    /// Factory Method para crear un nuevo RefreshToken
    /// </summary>
    public static Result<RefreshToken> Create(string token, int userId, DateTime expiresAt, string? deviceInfo = null)
    {
        if (string.IsNullOrWhiteSpace(token)) return Result<RefreshToken>.Invalid("Token is required");
        if (userId <= 0) return Result<RefreshToken>.Invalid("Invalid user ID");
        if (expiresAt <= DateTime.UtcNow) return Result<RefreshToken>.Invalid("Expiry date must be in the future");

        var refreshToken = new RefreshToken(token, userId, expiresAt, deviceInfo);
        
        return Result<RefreshToken>.Success(refreshToken);
    }

    /// <summary>
    /// Factory Method con expiración por defecto (7 días)
    /// </summary>
    public static Result<RefreshToken> CreateWithDefaultExpiry(string token, int userId, string? deviceInfo = null)
    {
        var expiresAt = DateTime.UtcNow.AddDays(DefaultExpiryDays);
        return Create(token, userId, expiresAt, deviceInfo);
    }

    // ==================== PROPERTIES ====================

    public string Token { get; private set; } = string.Empty;
    public int UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsUsed { get; private set; } = false;
    public DateTime? UsedAt { get; private set; }
    public string? DeviceInfo { get; private set; } // User-Agent, IP, etc.
    public string? ReplacedByToken { get; private set; } // Para token rotation

    // Navigation property
    public User User { get; private set; } = null!;

    // Computed properties
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => IsActive && !IsUsed && !IsExpired;

    // ==================== BUSINESS LOGIC ====================
    
    /// <summary>
    /// Marca el token como usado (para token rotation)
    /// </summary>
    public Result MarkAsUsed(string? replacedByToken = null)
    {
        if (IsUsed)
            return Result.Failure("Refresh token is already used");
        
        if (!IsActive)
            return Result.Failure("Cannot use an inactive refresh token");
        
        if (IsExpired)
            return Result.Failure("Cannot use an expired refresh token");
        
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
        
        return Result.Success();
    }

    /// <summary>
    /// Valida que el token pueda usarse
    /// </summary>
    public Result ValidateForUse()
    {
        if (IsUsed) return Result.Failure("Token already used");
        if (!IsActive) return Result.Failure("Token is inactive");
        if (IsExpired) return Result.Failure("Token expired");
        return Result.Success();
    }

    /// <summary>
    /// Revoca el token
    /// </summary>
    public void Revoke() => IsActive = false;

    /// <summary>
    /// Extiende la fecha de expiración
    /// </summary>
    public void Extend(int additionalDays)
    {
        if (additionalDays > 0 && additionalDays <= 30)
            ExpiresAt = ExpiresAt.AddDays(additionalDays);
    }
}
