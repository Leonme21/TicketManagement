using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Application.Contracts.Tickets;

/// <summary>
/// Request para asignar ticket a un agente
/// </summary>
public class AssignTicketRequest
{
    public int AgentId { get; set; }
}