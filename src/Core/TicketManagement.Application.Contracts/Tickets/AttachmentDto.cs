namespace TicketManagement.Application.Contracts.Tickets;

public record AttachmentDto
{
    public required int Id { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long Size { get; init; }
    public required string DownloadUrl { get; init; }
    public required DateTimeOffset UploadedAt { get; init; }
    public required string UploadedBy { get; init; }
}
