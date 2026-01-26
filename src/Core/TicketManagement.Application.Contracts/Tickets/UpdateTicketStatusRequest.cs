using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Application.Contracts.Tickets;

/// <summary>
/// Request para cambiar estado del ticket
/// </summary>
public class UpdateTicketStatusRequest
{
    public string Status { get; set; } = string.Empty; // Open, InProgress, Resolved, Closed, Reopened
}
