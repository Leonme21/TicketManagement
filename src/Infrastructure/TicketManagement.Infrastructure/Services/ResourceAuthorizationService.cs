using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Infrastructure.Services;

/// <summary>
/// 🔥 ENTERPRISE LEVEL: Resource-based authorization service
/// Implements fine-grained ticket-level authorization rules
/// </summary>
public sealed class ResourceAuthorizationService : IResourceAuthorizationService
{
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ResourceAuthorizationService> _logger;

    public ResourceAuthorizationService(
        ICurrentUserService currentUser,
        ILogger<ResourceAuthorizationService> logger)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<bool> CanViewTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        // Anyone can view if they are the creator, assigned agent, or admin
        bool canView = ticket.CreatorId == userId ||
                       ticket.AssignedToId == userId ||
                       _currentUser.Role == "Admin";

        LogAuthorizationCheck(nameof(CanViewTicketAsync), userId, ticket.Id, canView);
        return canView;
    }

    public async Task<bool> CanUpdateTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        // Creator or assigned agent can update (but not if closed)
        // Admin can always update
        bool canUpdate = ticket.Status != TicketStatus.Closed && (
                            ticket.CreatorId == userId ||
                            ticket.AssignedToId == userId ||
                            _currentUser.Role == "Admin"
                         );

        LogAuthorizationCheck(nameof(CanUpdateTicketAsync), userId, ticket.Id, canUpdate);
        return canUpdate;
    }

    public async Task<bool> CanDeleteTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        // Only admins can delete tickets
        bool canDelete = _currentUser.Role == "Admin";

        LogAuthorizationCheck(nameof(CanDeleteTicketAsync), userId, ticket.Id, canDelete);
        return canDelete;
    }

    public async Task<bool> CanAssignTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        // Creator or admin can assign (but not if closed)
        bool canAssign = ticket.Status != TicketStatus.Closed && (
                            ticket.CreatorId == userId ||
                            _currentUser.Role == "Admin"
                         );

        LogAuthorizationCheck(nameof(CanAssignTicketAsync), userId, ticket.Id, canAssign);
        return canAssign;
    }

    public async Task<bool> CanCommentTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        // Creator, assigned agent, or admin can comment
        bool canComment = ticket.CreatorId == userId ||
                          ticket.AssignedToId == userId ||
                          _currentUser.Role == "Admin";

        LogAuthorizationCheck(nameof(CanCommentTicketAsync), userId, ticket.Id, canComment);
        return canComment;
    }

    public async Task<bool> CanCloseTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        // Assigned agent or admin can close (but not if already closed)
        bool canClose = ticket.Status != TicketStatus.Closed && (
                           ticket.AssignedToId == userId ||
                           _currentUser.Role == "Admin"
                        );

        LogAuthorizationCheck(nameof(CanCloseTicketAsync), userId, ticket.Id, canClose);
        return canClose;
    }

    private void LogAuthorizationCheck(string operation, int userId, int ticketId, bool authorized)
    {
        var level = authorized ? LogLevel.Debug : LogLevel.Warning;
        _logger.Log(
            level,
            "Authorization check - Operation: {Operation}, UserId: {UserId}, TicketId: {TicketId}, Authorized: {Authorized}",
            operation,
            userId,
            ticketId,
            authorized);
    }
}
