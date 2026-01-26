using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Application.Common.Exceptions;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Interfaces;

namespace TicketManagement.Application.Tags.Commands.AddTagToTicket;

public class AddTagToTicketCommandHandler : IRequestHandler<AddTagToTicketCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddTagToTicketCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddTagToTicketCommand request, CancellationToken cancellationToken)
    {
        // 1. Buscar el Ticket (incluyendo sus tags actuales para no duplicar)
        // Nota: Asumimos que GetById ya trae los tags, o usamos Include en el repo.
        // Por ahora usaremos el GetById básico.
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null)
            throw new NotFoundException(nameof(Ticket), request.TicketId);

        // 2. Buscar el Tag
        var tag = await _unitOfWork.Tags.GetByIdAsync(request.TagId, cancellationToken);
        if (tag == null)
            throw new NotFoundException(nameof(Tag), request.TagId);

        // 3. Unir (Usando el método de Dominio que creaste)
        ticket.AddTag(tag);

        // 4. Guardar
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}