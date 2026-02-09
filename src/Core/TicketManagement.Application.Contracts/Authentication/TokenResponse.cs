namespace TicketManagement.Application.Contracts.Authentication;

/// <summary>
/// ✅ Response DTO para operaciones de autenticación
/// Incluye Access Token y Refresh Token para seguridad mejorada
/// </summary>
public class TokenResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required string TokenType { get; init; } = "Bearer";
    public required int ExpiresInSeconds { get; init; }
    
    /// <summary>
    /// Información del usuario autenticado
    /// </summary>
    public required UserInfo User { get; init; }
}

/// <summary>
/// Información básica del usuario para el token response
/// </summary>
public class UserInfo
{
    public required int Id { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required string Role { get; init; }
}
