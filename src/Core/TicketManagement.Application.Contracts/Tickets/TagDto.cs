namespace TicketManagement.Application.Contracts.Tickets;

public record TagDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Color { get; init; }
}
