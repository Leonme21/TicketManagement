using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Tickets.Commands.AssignTicket;

/// <summary>
/// Command para asignar ticket a un agente
/// ? REFACTORIZADO: Devuelve Result<Unit> para consistencia
/// </summary>
public record AssignTicketCommand : IRequest<Result>
{
    public int TicketId { get; init; }
    public int AgentId { get; init; }
}
