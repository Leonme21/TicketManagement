using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tags.Commands.AddTagToTicket;

/// <summary>
/// Handler para agregar tag a un ticket
/// ? REFACTORIZADO: Usa IUnitOfWork como único punto de acceso
/// </summary>
public class AddTagToTicketHandler : IRequestHandler<AddTagToTicketCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddTagToTicketHandler> _logger;

    public AddTagToTicketHandler(
        IUnitOfWork unitOfWork,
        ILogger<AddTagToTicketHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(AddTagToTicketCommand request, CancellationToken cancellationToken)
    {
        // Obtener ticket con sus tags actuales
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
        {
            return Result.NotFound($"Ticket {request.TicketId} not found");
        }

        // Verificar que el tag existe
        var tag = await _unitOfWork.Tags.GetByIdAsync(request.TagId, cancellationToken);
        if (tag == null)
        {
            return Result.NotFound($"Tag {request.TagId} not found");
        }

        // ? Lógica de dominio encapsulada (previene duplicados)
        ticket.AddTag(tag);

        // ? UnitOfWork maneja el SaveChanges
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tag {TagId} added to Ticket {TicketId}", request.TagId, request.TicketId);

        return Result.Success();
    }
}
