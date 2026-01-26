namespace TicketManagement.Application.Contracts.Tickets;

/// <summary>
/// DTO ligero para lista de tickets (sin comentarios/attachments)
/// </summary>
public class TicketDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;

    // Creator info
    public int CreatorId { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public string CreatorEmail { get; set; } = string.Empty;

    // Assigned agent info
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }

    // Category info
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Counts
    public int CommentsCount { get; set; }
    public int AttachmentsCount { get; set; }
}
