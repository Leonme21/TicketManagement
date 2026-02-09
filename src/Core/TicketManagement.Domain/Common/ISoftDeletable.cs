using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Domain.Common;

/// <summary>
/// Marca entidades que soportan eliminación lógica (soft delete)
/// En lugar de DELETE, se marca IsDeleted = true
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
