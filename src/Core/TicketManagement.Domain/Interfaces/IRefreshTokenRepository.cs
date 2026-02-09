using TicketManagement.Domain.Entities;

namespace TicketManagement.Domain.Interfaces;

/// <summary>
/// ✅ Repository interface para RefreshToken
/// Operaciones específicas para manejo de tokens de renovación
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Busca un refresh token por su valor
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene todos los refresh tokens activos de un usuario
    /// </summary>
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Agrega un nuevo refresh token
    /// </summary>
    void Add(RefreshToken refreshToken);
    
    /// <summary>
    /// Revoca todos los refresh tokens de un usuario (logout de todos los dispositivos)
    /// </summary>
    Task RevokeAllUserTokensAsync(int userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Limpia tokens expirados (para job de limpieza)
    /// </summary>
    Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si existe un token activo para un usuario
    /// </summary>
    Task<bool> HasActiveTokenAsync(int userId, CancellationToken cancellationToken = default);
}
