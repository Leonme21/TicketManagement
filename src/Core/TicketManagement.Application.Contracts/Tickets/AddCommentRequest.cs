using System;
using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Application.Contracts.Tickets;

/// <summary>
/// ?? BIG TECH LEVEL: Request DTO for adding a comment to a ticket
/// </summary>
public record AddCommentApiRequest
{
    [Required(ErrorMessage = "Content is required")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 2000 characters")]
    public required string Content { get; init; }

    /// <summary>
    /// If true, the comment is only visible to agents and admins
    /// </summary>
    public bool? IsInternal { get; init; }
}

/// <summary>
/// Legacy class maintained for backward compatibility
/// </summary>
[Obsolete("Use AddCommentApiRequest instead")]
public class AddCommentRequest
{
    public int TicketId { get; set; }

    [Required(ErrorMessage = "Content is required")]
    [StringLength(1000, ErrorMessage = "Content cannot exceed 1000 characters")]
    public string Content { get; set; } = string.Empty;
}
