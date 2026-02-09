using System;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Domain.Entities;

/// <summary>
/// ✅ SENIOR LEVEL: Clean Comment entity with proper factory method
/// </summary>
public class Comment : BaseEntity
{
    private Comment() { } // EF Core

    private Comment(string content, int ticketId, int authorId, bool isInternal = false)
    {
        Content = content;
        TicketId = ticketId;
        AuthorId = authorId;
        IsInternal = isInternal;
    }

    /// <summary>
    /// ✅ Factory method for creating comments with validation
    /// </summary>
    public static Result<Comment> Create(string content, int ticketId, int authorId, bool isInternal = false)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Result<Comment>.Failure("Comment content cannot be empty");

        if (content.Length > 2000)
            return Result<Comment>.Failure("Comment content cannot exceed 2000 characters");



        if (authorId <= 0)
            return Result<Comment>.Failure("Invalid author ID");

        var comment = new Comment(content, ticketId, authorId, isInternal);
        return Result<Comment>.Success(comment);
    }

    public string Content { get; private set; } = string.Empty;
    public bool IsInternal { get; private set; }

    // Foreign Keys
    public int TicketId { get; private set; }
    public int AuthorId { get; private set; }

    // Navigation Properties
    public Ticket Ticket { get; set; } = null!;
    public User Author { get; set; } = null!;

    /// <summary>
    /// ✅ Update comment content with validation
    /// </summary>
    public Result UpdateContent(string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            return Result.Failure("Comment content cannot be empty");

        if (newContent.Length > 2000)
            return Result.Failure("Comment content cannot exceed 2000 characters");

        Content = newContent;
        return Result.Success();
    }
}
