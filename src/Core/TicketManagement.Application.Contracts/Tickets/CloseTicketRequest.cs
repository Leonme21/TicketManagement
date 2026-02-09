using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Application.Contracts.Tickets;

/// <summary>
/// ?? BIG TECH LEVEL: Request DTO for closing a ticket
/// </summary>
public record CloseTicketApiRequest
{
    /// <summary>
    /// Optional reason for closing the ticket
    /// </summary>
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; init; }

    /// <summary>
    /// Optional resolution description
    /// </summary>
    [StringLength(2000, ErrorMessage = "Resolution cannot exceed 2000 characters")]
    public string? Resolution { get; init; }
}
