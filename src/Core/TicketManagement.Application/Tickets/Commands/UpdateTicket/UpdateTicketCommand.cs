using MediatR;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Tickets.Commands.UpdateTicket;

/// <summary>
/// Command para actualizar ticket existente
/// </summary>
public record UpdateTicketCommand : IRequest<Unit>
{
    public int TicketId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public TicketPriority Priority { get; init; } = TicketPriority.Medium;
    public int CategoryId { get; init; }
}
