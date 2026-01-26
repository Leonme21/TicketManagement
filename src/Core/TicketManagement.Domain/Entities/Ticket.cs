using System;
using System.Collections.Generic;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Exceptions;
using TicketManagement.Domain.Constants;

namespace TicketManagement.Domain.Entities;

/// <summary>
/// Entidad principal:  Ticket de soporte técnico
/// Contiene toda la lógica de negocio del ciclo de vida
/// </summary>
public class Ticket : BaseEntity, ISoftDeletable
{
    // Constructor privado para EF Core
    private Ticket() { }

    /// <summary>
    /// Constructor para crear nuevo ticket
    /// </summary>
    public Ticket(string title, string description, TicketPriority priority, int categoryId, int creatorId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Ticket title cannot be empty");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Ticket description cannot be empty");

        Title = title;
        Description = description;
        Priority = priority;
        CategoryId = categoryId;
        CreatorId = creatorId;
        Status = TicketStatus.Open;
    }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }

    // Foreign Keys
    public int CreatorId { get; private set; }
    public int? AssignedToId { get; private set; }
    public int CategoryId { get; private set; }

    // Navigation Properties
    public User Creator { get; private set; } = null!;
    public User? AssignedTo { get; private set; }
    public Category Category { get; private set; } = null!;

    private readonly List<Comment> _comments = new();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    private readonly List<Attachment> _attachments = new();
    public IReadOnlyCollection<Attachment> Attachments => _attachments.AsReadOnly();

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // ==================== BUSINESS LOGIC ====================

    /// <summary>
    /// Asigna el ticket a un agente
    /// Cambia estado a InProgress si estaba Open
    /// </summary>
    public void Assign(int agentId)
    {
        if (Status == TicketStatus.Closed)
            throw new DomainException("Cannot assign a closed ticket.  Reopen it first.");

        if (agentId <= 0)
            throw new DomainException("Invalid agent ID");

        AssignedToId = agentId;

        // Transición automática de estado
        if (Status == TicketStatus.Open)
            Status = TicketStatus.InProgress;
    }

    /// <summary>
    /// Desasigna el ticket (vuelve a pool de tickets sin asignar)
    /// </summary>
    public void Unassign()
    {
        if (Status == TicketStatus.Closed)
            throw new DomainException("Cannot unassign a closed ticket");

        AssignedToId = null;
        Status = TicketStatus.Open;
    }

    /// <summary>
    /// Cambia el estado del ticket
    /// Valida transiciones permitidas
    /// </summary>
    public void UpdateStatus(TicketStatus newStatus)
    {
        // Validar transiciones de estado
        if (Status == TicketStatus.Closed && newStatus != TicketStatus.Reopened)
            throw new DomainException("Closed tickets can only be reopened");

        if (newStatus == TicketStatus.InProgress && AssignedToId == null)
            throw new DomainException("Cannot set status to InProgress without assigning an agent");

        Status = newStatus;
    }

    /// <summary>
    /// Marca el ticket como resuelto (esperando confirmación del cliente)
    /// </summary>
    public void Resolve()
    {
        if (Status == TicketStatus.Closed)
            throw new DomainException("Cannot resolve a closed ticket");

        if (AssignedToId == null)
            throw new DomainException("Only assigned tickets can be resolved");

        Status = TicketStatus.Resolved;
    }

    /// <summary>
    /// Cierra el ticket definitivamente
    /// </summary>
    public void Close()
    {
        if (Status == TicketStatus.Closed)
            throw new DomainException("Ticket is already closed");

        Status = TicketStatus.Closed;
    }

    /// <summary>
    /// Reabre un ticket cerrado (por escalación o error)
    /// </summary>
    public void Reopen()
    {
        if (Status != TicketStatus.Closed)
            throw new DomainException("Only closed tickets can be reopened");

        Status = TicketStatus.Reopened;
    }

    /// <summary>
    /// Actualiza título, descripción y prioridad
    /// </summary>
    public void Update(string title, string description, TicketPriority priority)
    {
        if (Status == TicketStatus.Closed)
            throw new DomainException("Cannot update a closed ticket");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Ticket title cannot be empty");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Ticket description cannot be empty");

        Title = title;
        Description = description;
        Priority = priority;
    }

    /// <summary>
    /// Cambia la prioridad (escalación)
    /// </summary>
    public void ChangePriority(TicketPriority newPriority)
    {
        if (Status == TicketStatus.Closed)
            throw new DomainException("Cannot change priority of a closed ticket");

        Priority = newPriority;
    }

    /// <summary>
    /// Cambia la categoría del ticket
    /// </summary>
    public void ChangeCategory(int newCategoryId)
    {
        if (Status == TicketStatus.Closed)
            throw new DomainException("Cannot change category of a closed ticket");

        if (newCategoryId <= 0)
            throw new DomainException("Invalid category ID");

        CategoryId = newCategoryId;
    }

    /// <summary>
    /// Agrega un comentario al ticket
    /// </summary>
    public Comment AddComment(string content, int userId)
    {
        var comment = new Comment(content, Id, userId);
        _comments.Add(comment);
        return comment;
    }


    /// <summary>
    /// Agrega un archivo adjunto al ticket
    /// </summary>
    public void AddAttachment(Attachment attachment)
    {
        _attachments.Add(attachment);
    }

    /// <summary>
    /// Verifica si un usuario tiene permisos para modificar este ticket
    /// </summary>
    public bool CanModify(int userId, string? userRole)
    {
        // El creador siempre puede modificar
        if (CreatorId == userId) return true;

        // Los administradores siempre pueden modificar
        if (userRole == Roles.Admin) return true;

        return false;
    }

    private readonly List<Tag> _tags = new();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    public void AddTag(Tag tag)
    {
        // Validación de negocio (opcional): Evitar duplicados
        if (_tags.Any(t => t.Id == tag.Id))
        {
            return; // O lanzar excepción si eres estricto
        }

        _tags.Add(tag);
    }
    public void RemoveTag(Tag tag)
    {
        _tags.Remove(tag);
    }
}
