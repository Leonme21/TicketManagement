using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Exceptions;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tickets.Commands.CreateTicket;

/// <summary>
/// Handler para crear ticket
/// </summary>
public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateTicketCommandHandler> _logger;

    public CreateTicketCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateTicketCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<int> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        // Obtener ID del usuario autenticado
        var userId = _currentUserService.UserIdInt;
        if (!userId.HasValue)
        {
            throw new ForbiddenAccessException("User is not authenticated");
        }

        _logger.LogInformation("Creating new ticket. UserId: {UserId}, CategoryId: {CategoryId}, Priority: {Priority}", 
            userId.Value, request.CategoryId, request.Priority);

        // Validar Regla de Negocio: Existencia de Categoría
        var categoryExists = await _unitOfWork.Categories.ExistsAsync(request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        // Crear entidad de dominio
        var ticket = new Ticket(
            request.Title,
            request.Description,
            request.Priority,
            request.CategoryId,
            userId.Value);

        // Guardar en BD
        _unitOfWork.Tickets.Add(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} created successfully for user {UserId}", ticket.Id, userId.Value);

        return ticket.Id;
    }
}
