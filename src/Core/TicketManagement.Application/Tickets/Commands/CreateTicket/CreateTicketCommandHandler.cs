using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Exceptions;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Interfaces;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Tickets.Commands.CreateTicket;

/// <summary>
/// 🔥 STAFF LEVEL: Pure application logic without infrastructure dependencies
/// No references to EF Core - abstraction preserved
/// Concurrency handling moved to Infrastructure layer
/// </summary>
public sealed class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, Result<CreateTicketResponse>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateTicketCommandHandler> _logger;

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CreateTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<CreateTicketResponse>> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        // 1. Create Aggregate (Pure Domain Logic)
        var ticketResult = Ticket.Create(
            request.Title,
            request.Description,
            request.Priority,
            request.CategoryId,
            userId);

        if (ticketResult.IsFailure)
            return Result.Failure<CreateTicketResponse>(ticketResult.Error);

        var ticket = ticketResult.Value!;
        
        // 2. Add to Repository
        await _ticketRepository.AddAsync(ticket, cancellationToken);
        
        // 3. Commit - Concurrency exceptions are caught by TransactionBehavior
        // ✅ FIXED: No try-catch for DbUpdateConcurrencyException (abstraction leak removed)
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Ticket {TicketId} created successfully by User {UserId}",
            ticket.Id,
            userId);

        // 4. Return response
        return Result.Success(new CreateTicketResponse
        {
            TicketId = ticket.Id,
            Message = "Ticket created successfully",
            Priority = ticket.Priority.ToString(),
            Status = ticket.Status.ToString(),
            CreatedAt = ticket.CreatedAt,
            EstimatedResolutionTime = null 
        });
    }
}
