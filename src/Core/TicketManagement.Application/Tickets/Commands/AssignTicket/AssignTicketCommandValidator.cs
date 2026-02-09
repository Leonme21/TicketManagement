using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using TicketManagement.Domain.Enums;

using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Application.Tickets.Commands.AssignTicket;

public class AssignTicketCommandValidator : AbstractValidator<AssignTicketCommand>
{
    private readonly IApplicationDbContext _context;

    public AssignTicketCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.TicketId)
            .GreaterThan(0).WithMessage("TicketId is required");

        RuleFor(x => x.AgentId)
            .GreaterThan(0).WithMessage("AgentId is required")
            .MustAsync(BeValidAgent).WithMessage("User is not an active agent");
    }

    private async Task<bool> BeValidAgent(int agentId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { agentId }, cancellationToken);
        return user != null && user.IsActive && (user.Role == UserRole.Agent || user.Role == UserRole.Admin);
    }
}
