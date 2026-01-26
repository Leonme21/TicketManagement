﻿using System;
using System.Collections.Generic;
using TicketManagement.Domain.Common;

namespace TicketManagement.Domain.Entities;

/// <summary>
/// Categoría para clasificar tickets (ej: "Bug", "Feature Request", "Support")
/// </summary>
public class Category : BaseEntity
{
    // Constructor sin parámetros requerido por EF Core
    private Category() { }

    /// <summary>
    /// Constructor para crear nueva categoría
    /// </summary>
    public Category(string name, string description)
    {
        Name = name;
        Description = description;
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    // Navigation property (EF Core carga los tickets relacionados)
    public ICollection<Ticket> Tickets { get; private set; } = new List<Ticket>();

    // Métodos de negocio
    public void UpdateDetails(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
