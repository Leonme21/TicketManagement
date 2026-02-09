using System;
using System.Collections.Generic;
using System.Linq;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Domain.Entities;

/// <summary>
/// Categora para clasificar tickets (ej: "Bug", "Feature Request", "Support")
///  Refactorizado: Validaciones, Domain Events, Encapsulacin completa y Result Pattern
/// </summary>
public class Category : BaseEntity, ISoftDeletable
{
    // ==================== CONSTANTS ====================
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 500;
    
    // ==================== CONSTRUCTORS ====================
    private Category() { }

    private Category(string name, string description)
    {
        Name = name;
        Description = description;
        IsActive = true;
    }
    
    // ==================== FACTORY METHOD ====================
    
    /// <summary>
    /// Factory Method para crear una nueva categora
    /// Encapsula validacin y emisin de Domain Events
    /// </summary>
    public static Result<Category> Create(string name, string description)
    {
        try
        {
            ValidateName(name);
            ValidateDescription(description);
            
            var category = new Category(name, description);
            
            // TODO: Implementar CategoryCreatedEvent cuando se necesite
            // category.AddDomainEvent(new CategoryCreatedEvent(category.Id, category.Name));
            
            return Result<Category>.Success(category);
        }
        catch (DomainException ex)
        {
            return Result<Category>.Failure(ex.Message);
        }
    }
    
    // ==================== PROPERTIES ====================

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation property
    private readonly List<Ticket> _tickets = new();
    public IReadOnlyCollection<Ticket> Tickets => _tickets.AsReadOnly();

    // ==================== BUSINESS LOGIC ====================
    
    /// <summary>
    /// ✅ REFACTORED: Usa Result Pattern consistentemente
    /// </summary>
    public Result UpdateDetails(string name, string description)
    {
        var nameValidation = ValidateName(name);
        if (nameValidation.IsFailure)
            return nameValidation;

        var descValidation = ValidateDescription(description);
        if (descValidation.IsFailure)
            return descValidation;
        
        Name = name;
        Description = description;

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure("Category is already inactive");
        
        if (HasOpenTickets())
            return Result.Failure("Cannot deactivate category with open tickets. Close all tickets first.");
        
        IsActive = false;
        
        // TODO: Implementar CategoryDeactivatedEvent cuando se necesite
        // AddDomainEvent(new CategoryDeactivatedEvent(Id, Name));

        return Result.Success();
    }
    
    public Result Activate()
    {
        if (IsActive)
            return Result.Failure("Category is already active");
        
        IsActive = true;
        
        // TODO: Implementar CategoryActivatedEvent cuando se necesite
        // AddDomainEvent(new CategoryActivatedEvent(Id, Name));

        return Result.Success();
    }
    
    private bool HasOpenTickets()
    {
        return _tickets.Any(t => t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved);
    }
    
    // ==================== VALIDATIONS ====================
    
    /// <summary>
    /// ✅ REFACTORED: Validaciones retornan Result en lugar de lanzar excepciones
    /// </summary>
    private static Result ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Invalid("Category name cannot be empty");
        
        if (name.Length > MaxNameLength)
            return Result.Invalid($"Category name cannot exceed {MaxNameLength} characters");

        return Result.Success();
    }
    
    private static Result ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Invalid("Category description cannot be empty");
        
        if (description.Length > MaxDescriptionLength)
            return Result.Invalid($"Category description cannot exceed {MaxDescriptionLength} characters");

        return Result.Success();
    }
}
