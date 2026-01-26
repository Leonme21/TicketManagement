using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace TicketManagement.Application.Tickets.Commands.AssignTicket;

/// <summary>
/// Command para asignar ticket a un agente
/// Solo Admins/Agents pueden ejecutar esto
/// </summary>
public record AssignTicketCommand : IRequest<Unit>
{
    public int TicketId { get; init; }
    public int AgentId { get; init; }
}
