using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;


namespace TicketManagement.Infrastructure.Identity;

public class TicketAuthorizationService : ITicketAuthorizationService
{
    private readonly IApplicationDbContext _context;

    public TicketAuthorizationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanAddCommentAsync(int userId, Ticket ticket, CancellationToken cancellationToken)
    {
        // Creator can always comment
        if (ticket.CreatorId == userId) return true;

        // Admins and Agents can comment
        var userRole = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Role)
            .FirstOrDefaultAsync(cancellationToken);
            
        return userRole == UserRole.Admin || userRole == UserRole.Agent;
    }

    public async Task<bool> CanModifyTicketAsync(int userId, Ticket ticket, CancellationToken cancellationToken)
    {
        if (ticket.CreatorId == userId) return true;

        var userRole = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Role)
            .FirstOrDefaultAsync(cancellationToken);

        return userRole == UserRole.Admin;
    }
}
