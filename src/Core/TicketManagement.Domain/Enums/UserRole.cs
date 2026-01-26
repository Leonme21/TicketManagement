using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Domain.Enums;

/// <summary>
/// Roles de usuario en el sistema
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Usuario final que crea tickets
    /// </summary>
    Customer = 1,

    /// <summary>
    /// Agente de soporte que resuelve tickets
    /// </summary>
    Agent = 2,

    /// <summary>
    /// Administrador con acceso total
    /// </summary>
    Admin = 3
}