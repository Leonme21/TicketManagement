using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Contracts.Tickets;

/// <summary>
/// 🔥 PRODUCTION-READY: Comprehensive ticket details DTO with all related data
/// </summary>
public record TicketDetailsDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required TicketStatus Status { get; init; }
    public required TicketPriority Priority { get; init; }
    
    // Category information
    public required int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    
    // User information
    public required int CreatorId { get; init; }
    public required string CreatorName { get; init; }
    public required string CreatorEmail { get; init; }
    
    public int? AssignedToId { get; init; }
    public string? AssignedToName { get; init; }
    public string? AssignedToEmail { get; init; }
    
    // Timestamps
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    
    // Related data
    public required IReadOnlyList<CommentDto> Comments { get; init; }
    public required IReadOnlyList<AttachmentDto> Attachments { get; init; }
    public required IReadOnlyList<TagDto> Tags { get; init; }
    
    // SLA information
    public TimeSpan? EstimatedResolutionTime { get; init; }
    public bool IsOverdue { get; init; }
    public TimeSpan? TimeToResolution { get; init; }
    
    // Concurrency control
    public required byte[] RowVersion { get; init; }
}

