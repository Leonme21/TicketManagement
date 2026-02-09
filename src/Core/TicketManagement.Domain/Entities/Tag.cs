using System;
using System.Collections.Generic;
using TicketManagement.Domain.Common;

namespace TicketManagement.Domain.Entities;

/// <summary>
/// Entidad Tag para clasificar tickets
/// ? REFACTORIZADO: Factory Method, validaciones, encapsulaci�n
/// </summary>
public class Tag : BaseEntity
{
    // Constructor privado para EF Core
    private Tag() { }

    // Constructor interno para Factory Method
    private Tag(string name, string color)
    {
        Name = name;
        Color = string.IsNullOrWhiteSpace(color) ? "#808080" : color;
    }

    /// <summary>
    /// Factory Method para crear nuevo Tag
    /// </summary>
    public static Result<Tag> Create(string name, string color)
    {
        var nameValidation = ValidateName(name);
        if (nameValidation.IsFailure) return Result<Tag>.Failure(nameValidation.Error);
        
        var colorValidation = ValidateColor(color);
        if (colorValidation.IsFailure) return Result<Tag>.Failure(colorValidation.Error);

        return Result<Tag>.Success(new Tag(name, color));
    }

    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;

    // Relaci�n Many-to-Many
    private readonly List<Ticket> _tickets = new();
    public IReadOnlyCollection<Ticket> Tickets => _tickets.AsReadOnly();

    // ==================== BUSINESS LOGIC ====================

    /// <summary>
    /// Actualiza nombre y color del tag
    /// </summary>
    public Result Update(string name, string color)
    {
        var nameValidation = ValidateName(name);
        if (nameValidation.IsFailure) return nameValidation;

        var colorValidation = ValidateColor(color);
        if (colorValidation.IsFailure) return colorValidation;

        Name = name;
        Color = color;

        return Result.Success();
    }

    // ==================== VALIDATIONS ====================

    private static Result ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Tag name cannot be empty");

        if (name.Length > 50)
            return Result.Failure("Tag name cannot exceed 50 characters");

        return Result.Success();
    }

    private static Result ValidateColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return Result.Success(); // Permitir null, se usará default

        if (!color.StartsWith("#") || (color.Length != 7 && color.Length != 4))
            return Result.Failure("Color must be a valid hex color (e.g., #FF5733)");

        return Result.Success();
    }
}
