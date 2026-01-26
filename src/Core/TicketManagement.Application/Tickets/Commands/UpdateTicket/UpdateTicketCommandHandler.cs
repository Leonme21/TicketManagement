using MediatR;
using TicketManagement.Application.Common.Exceptions;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Interfaces;
using TicketManagement.Domain.Constants;

namespace TicketManagement.Application.Tickets.Commands.UpdateTicket;

public class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTicketCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        // Use lightweight GetByIdAsync instead of implicit heavy loading
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
        {
            throw new NotFoundException(nameof(Ticket), request.TicketId);
        }

        // Validar permisos: solo el creador o admins pueden actualizar
        // ROBUSTNESS FIX: Prefer strongly typed ID if available, otherwise safe parse.
        int userId = _currentUserService.UserIdInt ?? 0;
        
        if (userId == 0 && !int.TryParse(_currentUserService.UserId, out userId))
        {
             // If ID is completely missing or not an integer (and we require int for DB), fail fast but gracefully.
             throw new ForbiddenAccessException("User is not authenticated or ID format is invalid.");
        }

        // Verificar permisos usando lógica de dominio
        if (!ticket.CanModify(userId, _currentUserService.Role))
        {
            throw new ForbiddenAccessException("You can only update your own tickets, or be an Admin");
        }

        // Validar existencia de categoría (Business Rule)
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        // Priority ya es enum, no necesita parsing - usar directamente
        ticket.Update(request.Title, request.Description, request.Priority);

        // _unitOfWork.Tickets.Update(ticket); // Removed: Entity is already tracked
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
