﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Domain.Common;

/// <summary>
/// Clase base para todas las entidades del dominio
/// Proporciona:  ID, auditoría (CreatedAt/UpdatedAt/CreatedBy/UpdatedBy)
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador único de la entidad
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Fecha de creación (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha de última modificación (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Usuario que creó la entidad (UserId o email)
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Usuario que modificó por última vez
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Token de concurrencia optimista (EF Core)
    /// </summary>
    [Timestamp]
    public DateTime RowVersion { get; set; }
}
