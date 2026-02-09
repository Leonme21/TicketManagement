using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// ✅ Service interface para generación y manejo de tokens JWT
/// Incluye Access Tokens y Refresh Tokens para seguridad mejorada
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Genera un Access Token JWT para el usuario
    /// </summary>
    Task<AccessTokenResult> GenerateAccessTokenAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un Refresh Token seguro para el usuario
    /// </summary>
    Task<RefreshToken> GenerateRefreshTokenAsync(int userId, string? deviceInfo = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida un Access Token y extrae los claims
    /// </summary>
    Task<TokenValidationResult> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoca todos los refresh tokens de un usuario
    /// </summary>
    Task RevokeAllUserTokensAsync(int userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado de generación de Access Token
/// </summary>
public class AccessTokenResult
{
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required int ExpiresInSeconds { get; init; }
}

/// <summary>
/// Resultado de validación de token
/// </summary>
public class TokenValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public int UserId { get; init; }
    public string? Email { get; init; }
    public string? Role { get; init; }
}
