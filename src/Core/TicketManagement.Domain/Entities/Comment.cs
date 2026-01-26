using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Domain.Entities;

/// <summary>
/// Comentario en un ticket (de cliente o agente)
/// </summary>
public class Comment : BaseEntity
{
    private Comment() { } // EF Core

    /// <summary>
    /// Constructor para crear nuevo comentario
    /// </summary>
    public Comment(string content, int ticketId, int authorId)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Comment content cannot be empty");

        if (content.Length > 5000)
            throw new DomainException("Comment content cannot exceed 5000 characters");

        Content = content;
        TicketId = ticketId;
        AuthorId = authorId;
    }

    public string Content { get; private set; } = string.Empty;

    // Foreign Keys
    public int TicketId { get; private set; }
    public int AuthorId { get; private set; }

    // Navigation Properties
    public Ticket Ticket { get; set; } = null!;
    public User Author { get; set; } = null!;

    /// <summary>
    /// Edita el contenido del comentario
    /// </summary>
    public void UpdateContent(string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            throw new DomainException("Comment content cannot be empty");

        if (newContent.Length > 5000)
            throw new DomainException("Comment content cannot exceed 5000 characters");

        Content = newContent;
    }
}
