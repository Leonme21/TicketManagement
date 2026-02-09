using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// Abstracción para obtener fecha/hora actual
/// Facilita testing (mock de tiempo)
/// </summary>
public interface IDateTime
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}
