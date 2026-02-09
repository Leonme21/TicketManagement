using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TicketManagement.Infrastructure.Persistence.Outbox;
using TicketManagement.Domain.Events;

namespace TicketManagement.Infrastructure.BackgroundServices;

/// <summary>
/// 🔥 BIG TECH LEVEL: Background service for processing outbox events
/// Ensures reliable processing of domain events with retry logic
/// </summary>
public sealed class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30);

    public OutboxProcessorService(IServiceProvider serviceProvider, ILogger<OutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox processor service stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService>();

        var messages = await outboxService.GetUnprocessedMessagesAsync(50, cancellationToken);
        
        if (messages.Count == 0)
            return;

        _logger.LogDebug("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                await ProcessMessageAsync(message, scope.ServiceProvider, cancellationToken);
                await outboxService.MarkAsProcessedAsync(message.Id, cancellationToken);
                
                _logger.LogDebug("Successfully processed outbox message {MessageId} of type {Type}", 
                    message.Id, message.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox message {MessageId} of type {Type}", 
                    message.Id, message.Type);
                
                await outboxService.MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }

    private async Task ProcessMessageAsync(OutboxMessage message, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // In a real implementation, you would have event handlers registered
        // For now, we'll just log the events
        
        switch (message.Type)
        {
            case nameof(TicketCreatedEvent):
                var ticketCreated = JsonSerializer.Deserialize<TicketCreatedEvent>(message.Data);
                await HandleTicketCreatedAsync(ticketCreated!, serviceProvider, cancellationToken);
                break;
                
            case nameof(TicketAssignedEvent):
                var ticketAssigned = JsonSerializer.Deserialize<TicketAssignedEvent>(message.Data);
                await HandleTicketAssignedAsync(ticketAssigned!, serviceProvider, cancellationToken);
                break;
                
            case nameof(TicketClosedEvent):
                var ticketClosed = JsonSerializer.Deserialize<TicketClosedEvent>(message.Data);
                await HandleTicketClosedAsync(ticketClosed!, serviceProvider, cancellationToken);
                break;
                
            case nameof(TicketUpdatedEvent):
                var ticketUpdated = JsonSerializer.Deserialize<TicketUpdatedEvent>(message.Data);
                await HandleTicketUpdatedAsync(ticketUpdated!, serviceProvider, cancellationToken);
                break;
                
            case nameof(TicketCommentAddedEvent):
                var commentAdded = JsonSerializer.Deserialize<TicketCommentAddedEvent>(message.Data);
                await HandleTicketCommentAddedAsync(commentAdded!, serviceProvider, cancellationToken);
                break;
                
            default:
                _logger.LogWarning("Unknown event type: {EventType}", message.Type);
                break;
        }
    }

    private async Task HandleTicketCreatedAsync(TicketCreatedEvent @event, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // Example: Send notification, update search index, etc.
        _logger.LogInformation("Handling TicketCreated event for ticket {TicketId}", @event.TicketId);
        
        // In a real implementation:
        // - Send email notifications
        // - Update search indexes
        // - Update analytics/metrics
        // - Trigger workflows
        
        await Task.CompletedTask;
    }

    private async Task HandleTicketAssignedAsync(TicketAssignedEvent @event, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling TicketAssigned event for ticket {TicketId} to agent {AgentId}", 
            @event.TicketId, @event.NewAgentId);
        
        await Task.CompletedTask;
    }

    private async Task HandleTicketClosedAsync(TicketClosedEvent @event, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling TicketClosed event for ticket {TicketId}", @event.TicketId);
        
        await Task.CompletedTask;
    }

    private async Task HandleTicketUpdatedAsync(TicketUpdatedEvent @event, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling TicketUpdated event for ticket {TicketId}", @event.TicketId);
        
        await Task.CompletedTask;
    }

    private async Task HandleTicketCommentAddedAsync(TicketCommentAddedEvent @event, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling TicketCommentAdded event for ticket {TicketId}", @event.TicketId);
        
        await Task.CompletedTask;
    }
}