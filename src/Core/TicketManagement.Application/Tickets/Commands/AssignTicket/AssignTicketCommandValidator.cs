using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tickets.Commands.AssignTicket;

public class AssignTicketCommandValidator : AbstractValidator<AssignTicketCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignTicketCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.TicketId)
            .GreaterThan(0).WithMessage("TicketId is required");

        RuleFor(x => x.AgentId)
            .GreaterThan(0).WithMessage("AgentId is required")
            .MustAsync(BeValidAgent).WithMessage("User is not an active agent");
    }

    private async Task<bool> BeValidAgent(int agentId, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(agentId, cancellationToken);
        return user != null && user.IsActive && (user.Role == UserRole.Agent || user.Role == UserRole.Admin);
    }
}
