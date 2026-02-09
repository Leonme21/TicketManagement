namespace TicketManagement.Application.Contracts.Tickets.ReadModels;

/// <summary>
/// ✅ SIMPLIFIED: Complete ticket details for single ticket view
/// </summary>
public record TicketDetailsReadModel
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required string Priority { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    // Creator info
    public required string CreatorName { get; init; }
    public required string CreatorEmail { get; init; }
    
    // Assignee info
    public string? AssigneeName { get; init; }
    public string? AssigneeEmail { get; init; }
    
    // Category info
    public required string CategoryName { get; init; }
    public required string CategoryColor { get; init; }
    
    // Related data
    public required List<CommentReadModel> Comments { get; init; } = new();
    public required List<AttachmentReadModel> Attachments { get; init; } = new();
    public required string[] Tags { get; init; } = Array.Empty<string>();
    
    // Concurrency control
    public required byte[] RowVersion { get; init; }
}

public record CommentReadModel
{
    public required int Id { get; init; }
    public required string Content { get; init; }
    public required string AuthorName { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required bool IsInternal { get; init; }
}

public record AttachmentReadModel
{
    public required int Id { get; init; }
    public required string FileName { get; init; }
    public required long FileSize { get; init; }
    public required string ContentType { get; init; }
    public required DateTime UploadedAt { get; init; }
}