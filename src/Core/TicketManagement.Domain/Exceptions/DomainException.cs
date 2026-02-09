using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Domain.Exceptions;

/// <summary>
/// Excepción base para violaciones de reglas de negocio en Domain
/// </summary>
public class DomainException : Exception
{
    public DomainException()
        : base("Domain rule violation occurred.")
    {
    }

    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
