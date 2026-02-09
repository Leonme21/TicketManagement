using System;
using System.Collections.Generic;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Exceptions;
using TicketManagement.Domain.ValueObjects;

namespace TicketManagement.Domain.Entities;

/// <summary>
/// Usuario del sistema (Customer, Agent o Admin)
///  Refactorizado: Validaciones, Result Pattern, Encapsulacin completa
/// </summary>
public class User : BaseEntity, ISoftDeletable
{
    // ==================== CONSTANTS ====================
    public const int MaxNameLength = 100;
    public const int MinPasswordHashLength = 60; // BCrypt hash length
    
    // ==================== CONSTRUCTORS ====================
    private User() { } // EF Core

    private User(string firstName, string lastName, Email email, string passwordHash, UserRole role)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }
    
    // ==================== FACTORY METHOD ====================
    
    /// <summary>
    /// ✅ REFACTORED: Factory Method using Email Value Object
    /// Validates all inputs before creating the entity (Fail Fast principle)
    /// </summary>
    public static Result<User> Create(string firstName, string lastName, string emailStr, string passwordHash, UserRole role)
    {
        // 1. Create Value Objects first (Fail Fast)
        var emailResult = Email.Create(emailStr);
        if (emailResult.IsFailure) 
            return Result<User>.Failure(emailResult.Error);

        var fNameVal = ValidateFirstName(firstName);
        if (fNameVal.IsFailure) 
            return Result<User>.Failure(fNameVal.Error);

        var lNameVal = ValidateLastName(lastName);
        if (lNameVal.IsFailure) 
            return Result<User>.Failure(lNameVal.Error);

        var pwdVal = ValidatePasswordHash(passwordHash);
        if (pwdVal.IsFailure) 
            return Result<User>.Failure(pwdVal.Error);
        
        // 2. Instantiate with valid objects
        return Result<User>.Success(new User(firstName, lastName, emailResult.Value!, passwordHash, role));
    }

    // ==================== PROPERTIES ====================

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    
    /// <summary>
    /// ✅ REFACTORED: Email as Value Object (no more primitive obsession)
    /// </summary>
    public Email Email { get; private set; } = null!;
    
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation properties
    private readonly List<Ticket> _createdTickets = new();
    public IReadOnlyCollection<Ticket> CreatedTickets => _createdTickets.AsReadOnly();
    
    private readonly List<Ticket> _assignedTickets = new();
    public IReadOnlyCollection<Ticket> AssignedTickets => _assignedTickets.AsReadOnly();
    
    private readonly List<Comment> _comments = new();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    public string FullName => $"{FirstName} {LastName}";

    // ==================== BUSINESS LOGIC ====================
    
    public Result UpdateProfile(string firstName, string lastName)
    {
        var fNameVal = ValidateFirstName(firstName);
        if (fNameVal.IsFailure) return fNameVal;

        var lNameVal = ValidateLastName(lastName);
        if (lNameVal.IsFailure) return lNameVal;
        
        FirstName = firstName;
        LastName = lastName;

        return Result.Success();
    }

    /// <summary>
    /// ✅ NEW: Update email using Value Object validation
    /// </summary>
    public Result UpdateEmail(string newEmailStr)
    {
        var emailResult = Email.Create(newEmailStr);
        if (emailResult.IsFailure) 
            return Result.Failure(emailResult.Error);

        Email = emailResult.Value!;
        return Result.Success();
    }

    public Result UpdatePassword(string newPasswordHash)
    {
        var pwdVal = ValidatePasswordHash(newPasswordHash);
        if (pwdVal.IsFailure) return pwdVal;

        PasswordHash = newPasswordHash;
        return Result.Success();
    }

    public Result ChangeRole(UserRole newRole)
    {
        if (Role == newRole)
            return Result.Failure($"User already has role {newRole}");
        
        Role = newRole;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure("User is already deactivated");
        
        IsActive = false;
        return Result.Success();
    }
    
    public Result Activate()
    {
        if (IsActive)
            return Result.Failure("User is already active");
        
        IsActive = true;
        return Result.Success();
    }
    
    // ==================== VALIDATIONS ====================
    
    private static Result ValidateFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure("First name cannot be empty");
        
        if (firstName.Length > MaxNameLength)
            return Result.Failure($"First name cannot exceed {MaxNameLength} characters");

        return Result.Success();
    }
    
    private static Result ValidateLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure("Last name cannot be empty");
        
        if (lastName.Length > MaxNameLength)
            return Result.Failure($"Last name cannot exceed {MaxNameLength} characters");

        return Result.Success();
    }
    
    // ❌ REMOVED: ValidateEmail - Logic moved to Email Value Object
    // Email validation is now centralized in the Email Value Object
    
    private static Result ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure("Password hash cannot be empty");
        
        if (passwordHash.Length < MinPasswordHashLength)
            return Result.Failure("Invalid password hash format");

        return Result.Success();
    }
}
