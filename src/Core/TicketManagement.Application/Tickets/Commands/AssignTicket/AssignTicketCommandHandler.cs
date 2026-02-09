using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tickets.Commands.AssignTicket;

/// <summary>
/// ?? BIG TECH LEVEL: Handler for assigning ticket to an agent
/// Uses Repository for aggregate operations, DbContext for SaveChanges
/// </summary>
public sealed class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand, Result<Unit>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly IApplicationDbContext _dbContext;

    public AssignTicketCommandHandler(
        ITicketRepository ticketRepository,
        IUserRepository userRepository,
        IApplicationDbContext dbContext)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _dbContext = dbContext;
    }

    public async Task<Result<Unit>> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
    {
        // Get the ticket aggregate
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket == null)
        {
            return Result.NotFound<Unit>("Ticket", request.TicketId);
        }

        // Verify that the agent exists and is active
        if (!await _userRepository.ExistsAndActiveAsync(request.AgentId, cancellationToken))
        {
            return Result.Invalid<Unit>($"Agent {request.AgentId} not found or inactive");
        }

        // ?? Domain logic encapsulated in aggregate - emits domain event
        var assignResult = ticket.Assign(request.AgentId);
        if (assignResult.IsFailure)
        {
            return Result.Failure<Unit>(assignResult.Error);
        }

        // Save changes via DbContext
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}
