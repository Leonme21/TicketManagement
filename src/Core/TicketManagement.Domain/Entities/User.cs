using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Domain.Entities;

/// <summary>
/// Usuario del sistema (Customer, Agent o Admin)
/// </summary>
public class User : BaseEntity, ISoftDeletable
{
    private User() { } // EF Core

    public User(string firstName, string lastName, string email, string passwordHash, UserRole role)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation properties
    public ICollection<Ticket> CreatedTickets { get; private set; } = new List<Ticket>();
    public ICollection<Ticket> AssignedTickets { get; private set; } = new List<Ticket>();
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

    // Computed property
    public string FullName => $"{FirstName} {LastName}";

    // Business methods
    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
