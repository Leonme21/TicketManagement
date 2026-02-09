namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// Abstracción para obtener información del usuario autenticado
/// Infrastructure lo implementa leyendo el JWT token
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    int? UserIdInt { get; }
    int GetUserId(); // Throws if not authenticated
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}
