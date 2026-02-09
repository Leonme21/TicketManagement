using MediatR;
using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common.Events;
using TicketManagement.Domain.Events;

namespace TicketManagement.Application.Tickets.EventHandlers;

/// <summary>
/// ? Event handler for TicketAssignedEvent
/// Triggered when a ticket is assigned to an agent
/// </summary>
public class TicketAssignedEventHandler : INotificationHandler<DomainEventNotification<TicketAssignedEvent>>
{
    private readonly ILogger<TicketAssignedEventHandler> _logger;
    // Future: IEmailService, IUserRepository

    public TicketAssignedEventHandler(ILogger<TicketAssignedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<TicketAssignedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;

        _logger.LogInformation(
            "? Processing TicketAssignedEvent: Ticket {TicketId} assigned to agent {AgentId}",
            evt.TicketId, evt.NewAgentId);

        // ? TODO: Implement notification logic
        // Examples:
        // 1. Send email to assigned agent
        // var agent = await _userRepository.GetByIdAsync(evt.NewAgentId, ct);
        // await _emailService.SendAsync(new TicketAssignedEmail(agent.Email, evt), ct);
        
        // 2. Send push notification to agent's mobile device
        // await _pushService.NotifyUserAsync(evt.NewAgentId, 
        //     $"New ticket assigned: #{evt.TicketId}", ct);
        
        // 3. Create in-app notification
        // await _notificationService.CreateAsync(new Notification
        // {
        //     UserId = evt.NewAgentId,
        //     Title = "New Ticket Assigned",
        //     Message = $"Ticket #{evt.TicketId} has been assigned to you",
        //     Type = NotificationType.TicketAssigned
        // }, ct);
        
        // 4. Update agent's workload metrics
        // await _metricsService.IncrementAgentWorkloadAsync(evt.NewAgentId, ct);

        await Task.CompletedTask;
    }
}
