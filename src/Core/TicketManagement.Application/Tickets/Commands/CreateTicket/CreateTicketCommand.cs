using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Tickets.Commands.CreateTicket;

/// <summary>
/// Command para crear nuevo ticket
/// </summary>
public record CreateTicketCommand : IRequest<int>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public TicketPriority Priority { get; init; } = TicketPriority.Medium; // Low, Medium, High, Critical
    public int CategoryId { get; init; }
}
