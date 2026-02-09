using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Application.Common.Exceptions;

/// <summary>
/// Excepción cuando usuario no tiene permisos
/// Middleware la captura y retorna 403 Forbidden
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("Access forbidden.")
    {
    }

    public ForbiddenAccessException(string message)
        : base(message)
    {
    }
}
