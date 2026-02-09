using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Domain.Enums;

/// <summary>
/// Estados del ciclo de vida de un ticket
/// </summary>
public enum TicketStatus
{
    /// <summary>
    /// Ticket recién creado, sin asignar
    /// </summary>
    Open = 1,

    /// <summary>
    /// Ticket asignado a un agente, en trabajo
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Ticket resuelto, esperando confirmación
    /// </summary>
    Resolved = 3,

    /// <summary>
    /// Ticket cerrado definitivamente
    /// </summary>
    Closed = 4,

    /// <summary>
    /// Ticket reabierto después de cerrado
    /// </summary>
    Reopened = 5
}
