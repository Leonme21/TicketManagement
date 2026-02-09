using Microsoft.Extensions.Logging;

namespace TicketManagement.Application.Tickets.Commands.CreateTicket;

public static partial class CreateTicketLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Creating ticket for User {UserId} in Category {CategoryId}")]
    public static partial void CreatingTicket(this ILogger logger, int userId, int categoryId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Creation failed: Category {CategoryId} not found")]
    public static partial void CategoryNotFound(this ILogger logger, int categoryId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Ticket {TicketId} created successfully")]
    public static partial void TicketCreated(this ILogger logger, int ticketId);
}
