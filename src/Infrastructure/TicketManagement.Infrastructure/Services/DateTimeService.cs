using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Infrastructure.Services;

/// <summary>
/// Servicio para obtener fecha/hora actual
/// Facilita testing (mock del tiempo)
/// </summary>
public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
