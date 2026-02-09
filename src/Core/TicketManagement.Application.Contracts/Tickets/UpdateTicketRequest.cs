using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Contracts.Tickets;

/// <summary>
/// ?? BIG TECH LEVEL: Request DTO for updating a ticket
/// Includes RowVersion for optimistic concurrency control
/// </summary>
public record UpdateTicketApiRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public required string Title { get; init; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
    public required string Description { get; init; }

    [Required(ErrorMessage = "Priority is required")]
    public required TicketPriority Priority { get; init; }

    [Required(ErrorMessage = "CategoryId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0")]
    public required int CategoryId { get; init; }

    /// <summary>
    /// RowVersion for optimistic concurrency control
    /// Must be provided from the last read operation
    /// </summary>
    [Required(ErrorMessage = "RowVersion is required for concurrency control")]
    public required byte[] RowVersion { get; init; }
}

/// <summary>
/// Legacy class maintained for backward compatibility
/// </summary>
[Obsolete("Use UpdateTicketApiRequest instead")]
public class UpdateTicketRequest
{
    public int TicketId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Priority is required")]
    public string Priority { get; set; } = string.Empty;
}
