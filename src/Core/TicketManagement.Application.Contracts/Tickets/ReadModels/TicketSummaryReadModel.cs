namespace TicketManagement.Application.Contracts.Tickets.ReadModels;

/// <summary>
/// 🔥 BIG TECH LEVEL: Optimized read model for ticket summaries
/// Denormalized for maximum query performance
/// </summary>
public sealed record TicketSummaryReadModel
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public required string Status { get; init; }
    public required string Priority { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    // Denormalized creator info
    public required string CreatorName { get; init; }
    public required string CreatorEmail { get; init; }
    
    // Denormalized assignee info
    public string? AssigneeName { get; init; }
    public string? AssigneeEmail { get; init; }
    
    // Denormalized category info
    public required string CategoryName { get; init; }
    public required string CategoryColor { get; init; }
    
    // Business metrics
    public TimeSpan? ResolutionTime { get; init; }
    public bool IsSlaViolated { get; init; }
    public TimeSpan? TimeToFirstResponse { get; init; }
    
    // Aggregated data
    public int CommentCount { get; init; }
    public int AttachmentCount { get; init; }
    public string[] Tags { get; init; } = Array.Empty<string>();
    
    // Search optimization
    public required string SearchVector { get; init; } // For full-text search
}

