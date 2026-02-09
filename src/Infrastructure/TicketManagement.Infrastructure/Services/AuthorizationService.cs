using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;
using TicketManagement.Infrastructure.Persistence;

namespace TicketManagement.Infrastructure.Services;

public sealed class AuthorizationService : IAuthorizationService
{
    private readonly ApplicationDbContext _context;

    public AuthorizationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsAuthorizedAsync(int userId, string policyName, CancellationToken cancellationToken = default)
    {
        // Simple policy check based on roles for now
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return false;

        return policyName switch
        {
            "CanAssignTickets" => user.Role == UserRole.Admin || user.Role == UserRole.Agent,
            "CanDeleteTickets" => user.Role == UserRole.Admin,
            "IsAgentOrAdmin" => user.Role == UserRole.Admin || user.Role == UserRole.Agent,
            "IsAdmin" => user.Role == UserRole.Admin,
            _ => true // Default permit
        };
    }

    public async Task<bool> IsAuthorizedAsync(int userId, string resource, string action, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return false;

        return (resource.ToLower(), action.ToLower(), user.Role) switch
        {
            // Ticket permissions
            ("ticket", "create", _) => true,
            ("ticket", "read", _) => true,
            ("ticket", "update", UserRole.Admin) => true,
            ("ticket", "update", UserRole.Agent) => true,
            ("ticket", "update", UserRole.Customer) => true, // Check ownership in command handler
            ("ticket", "delete", UserRole.Admin) => true,
            ("ticket", "assign", UserRole.Admin) => true,
            ("ticket", "assign", UserRole.Agent) => true,
            ("ticket", "close", UserRole.Admin) => true,
            ("ticket", "close", UserRole.Agent) => true,
            ("ticket", "comment", _) => true,

            // Category permissions
            ("category", "read", _) => true,
            ("category", "manage", UserRole.Admin) => true,

            // User permissions
            ("user", "read", UserRole.Admin) => true,
            ("user", "read", UserRole.Agent) => true,
            ("user", "manage", UserRole.Admin) => true,

            _ => false
        };
    }

    public async Task<bool> HasPermissionAsync(int userId, string action, int? resourceId = null, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return false;

        if (user.Role == UserRole.Admin) return true;

        // Basic logic: Agents can do most things except admin tasks
        if (user.Role == UserRole.Agent)
        {
            return action != "delete" && action != "configure";
        }

        return action == "read" || action == "create";
    }

    public async Task<Result> CanAccessTicketAsync(int userId, int ticketId, string action, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return Result.Failure(DomainErrors.User.InvalidCredentials);

        if (user.Role == UserRole.Admin) return Result.Success();

        var ticket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
        if (ticket == null) return Result.Failure(DomainErrors.Ticket.NotFound(ticketId));

        // Agents can access all tickets
        if (user.Role == UserRole.Agent) return Result.Success();

        // Customers can only access their own tickets
        if (ticket.CreatorId == userId) return Result.Success();

        return Result.Failure(DomainErrors.User.InsufficientPermissions);
    }

    public async Task<Result> CanCreateTicketInCategoryAsync(int userId, int categoryId, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return Result.Success(); // Allow all for now
    }

    public async Task<Result> CanAssignTicketAsync(int userId, int ticketId, int targetAgentId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return Result.Failure(DomainErrors.User.InvalidCredentials);

        return (user.Role == UserRole.Admin || user.Role == UserRole.Agent)
            ? Result.Success()
            : Result.Failure(DomainErrors.User.InsufficientPermissions);
    }

    public async Task<bool> CanUserUpdateTicketAsync(int userId, int ticketId, CancellationToken cancellationToken = default)
    {
        var result = await CanAccessTicketAsync(userId, ticketId, "update", cancellationToken);
        return result.IsSuccess;
    }

    public async Task<bool> CanUserViewTicketAsync(int userId, int ticketId, CancellationToken cancellationToken = default)
    {
        var result = await CanAccessTicketAsync(userId, ticketId, "read", cancellationToken);
        return result.IsSuccess;
    }

    public async Task<bool> CanUserAssignTicketAsync(int userId, int ticketId, CancellationToken cancellationToken = default)
    {
        var result = await CanAssignTicketAsync(userId, ticketId, 0, cancellationToken);
        return result.IsSuccess;
    }

    public async Task<bool> CanUserDeleteTicketAsync(int userId, int ticketId, CancellationToken cancellationToken = default)
    {
         var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
         return user?.Role == UserRole.Admin;
    }

    public async Task<bool> CanUserCloseTicketAsync(int userId, int ticketId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return false;
        
        if (user.Role == UserRole.Admin || user.Role == UserRole.Agent) return true;
        
        var ticket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
        return ticket?.CreatorId == userId;
    }

    public async Task<bool> CanUserAddCommentAsync(int userId, int ticketId, bool isInternal = false, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return false;

        if (isInternal && user.Role == UserRole.Customer) return false;
        
        return await CanUserViewTicketAsync(userId, ticketId, cancellationToken);
    }

    public async Task<bool> IsUserInRoleAsync(int userId, UserRole role, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return user?.Role == role;
    }

    public async Task<bool> HasElevatedPermissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return user?.Role == UserRole.Admin || user?.Role == UserRole.Agent;
    }
}