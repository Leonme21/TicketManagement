using System.Threading;
using System.Threading.Tasks;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// Service for resource-level authorization checks
/// Validates if a user can perform specific actions on a ticket
/// </summary>
public interface IResourceAuthorizationService
{
    /// <summary>
    /// Checks if user can view ticket details
    /// </summary>
    Task<bool> CanViewTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user can update ticket (change title, description, priority, etc)
    /// </summary>
    Task<bool> CanUpdateTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user can delete ticket
    /// </summary>
    Task<bool> CanDeleteTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user can assign ticket to an agent
    /// </summary>
    Task<bool> CanAssignTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user can add comments to ticket
    /// </summary>
    Task<bool> CanCommentTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user can close ticket
    /// </summary>
    Task<bool> CanCloseTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken = default);
}
