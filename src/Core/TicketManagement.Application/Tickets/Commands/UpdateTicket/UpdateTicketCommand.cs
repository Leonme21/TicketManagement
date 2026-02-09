using MediatR;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Tickets.Commands.UpdateTicket;

/// <summary>
/// Command para actualizar ticket existente
/// Incluye RowVersion para manejo de concurrencia optimista
/// </summary>
public record UpdateTicketCommand : IRequest<Result>
{
    public int TicketId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public TicketPriority Priority { get; init; } = TicketPriority.Medium;
    public int CategoryId { get; init; }
    
    /// <summary>
    /// Token de concurrencia optimista - debe coincidir con el valor actual en BD
    /// </summary>
    public required byte[] RowVersion { get; init; } = Array.Empty<byte>();
}
