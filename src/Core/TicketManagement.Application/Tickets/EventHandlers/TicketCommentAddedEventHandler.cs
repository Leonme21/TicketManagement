using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Events;
using TicketManagement.Domain.Events;

namespace TicketManagement.Application.Tickets.EventHandlers;

/// <summary>
/// ? Event handler for TicketCommentAddedEvent
/// Triggered when a comment is added to a ticket
/// </summary>
public class TicketCommentAddedEventHandler : INotificationHandler<DomainEventNotification<TicketCommentAddedEvent>>
{
    private readonly ILogger<TicketCommentAddedEventHandler> _logger;
    // Future: IEmailService, IUserRepository

    public TicketCommentAddedEventHandler(ILogger<TicketCommentAddedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<TicketCommentAddedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;

        _logger.LogInformation(
            "? Processing TicketCommentAddedEvent: Comment added to ticket {TicketId} by user {UserId}",
            evt.TicketId, evt.AuthorId);

        // ? TODO: Implement notification logic
        // Examples:
        // 1. Notify ticket creator (if not the commenter)
        // if (ticket.CreatorId != evt.AuthorId)
        // {
        //     await _emailService.SendAsync(new NewCommentEmail(ticket.Creator.Email, evt), ct);
        // }
        
        // 2. Notify assigned agent (if not the commenter)
        // if (ticket.AssignedToId != null && ticket.AssignedToId != evt.AuthorId)
        // {
        //     await _pushService.NotifyUserAsync(ticket.AssignedToId.Value, 
        //         $"New comment on ticket #{evt.TicketId}", ct);
        // }
        
        // 3. Send real-time notification via SignalR
        // await _hubContext.Clients.Group($"ticket-{evt.TicketId}")
        //     .SendAsync("NewComment", evt, ct);
        
        // 4. Update last activity timestamp
        // await _analyticsService.UpdateTicketActivityAsync(evt.TicketId, evt.CreatedAt, ct);

        await Task.CompletedTask;
    }
}
