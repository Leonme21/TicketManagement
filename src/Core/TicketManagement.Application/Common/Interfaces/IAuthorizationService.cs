using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// 🔥 PRODUCTION-READY: Enhanced authorization service interface
/// Provides comprehensive permission checking for all business operations
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Verifica si el usuario tiene permiso para realizar una acción en un recurso específico
    /// </summary>
    Task<bool> HasPermissionAsync(int userId, string action, int? resourceId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si el usuario cumple con una política de autorización específica
    /// </summary>
    Task<bool> IsAuthorizedAsync(int userId, string policyName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si el usuario tiene permiso para realizar una acción en un recurso (string)
    /// </summary>
    Task<bool> IsAuthorizedAsync(int userId, string resource, string action, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si el usuario puede acceder a un ticket específico
    /// </summary>
    Task<Result> CanAccessTicketAsync(int userId, int ticketId, string action, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si el usuario puede crear tickets en una categoría específica
    /// </summary>
    Task<Result> CanCreateTicketInCategoryAsync(int userId, int categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si el usuario puede asignar tickets
    /// </summary>
    Task<Result> CanAssignTicketAsync(int userId, int ticketId, int targetAgentId, CancellationToken cancellationToken = default);

    // 🔥 NEW: Granular permission methods with security logging
    /// <summary>
    /// Verifica si el usuario puede actualizar un ticket específico
    /// </summary>
    Task<bool> CanUserUpdateTicketAsync(int userId, int ticketId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si el usuario puede ver un ticket específico
    /// </summary>
    Task<bool> CanUserViewTicketAsync(int userId, int ticketId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si el usuario puede asignar tickets (general)
    /// </summary>
    Task<bool> CanUserAssignTicketAsync(int userId, int ticketId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si el usuario puede eliminar un ticket específico
    /// </summary>
    Task<bool> CanUserDeleteTicketAsync(int userId, int ticketId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si el usuario puede cerrar un ticket específico
    /// </summary>
    Task<bool> CanUserCloseTicketAsync(int userId, int ticketId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si el usuario puede agregar comentarios a un ticket
    /// </summary>
    Task<bool> CanUserAddCommentAsync(int userId, int ticketId, bool isInternal = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si el usuario tiene un rol específico
    /// </summary>
    Task<bool> IsUserInRoleAsync(int userId, UserRole role, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si el usuario tiene permisos elevados (Agent o Admin)
    /// </summary>
    Task<bool> HasElevatedPermissionsAsync(int userId, CancellationToken cancellationToken = default);
}