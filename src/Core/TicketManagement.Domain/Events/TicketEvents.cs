using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.ValueObjects;

namespace TicketManagement.Domain.Events;

/// <summary>
/// 🔥 BIG TECH LEVEL: Domain events for ticket operations
/// Clean, immutable events that represent business-significant occurrences
/// All events inherit from DomainEvent base record which provides EventId and OccurredOn
/// </summary>

public sealed record TicketCreatedEvent(
    int TicketId,
    TicketTitle Title,
    int CreatorId,
    TicketPriority Priority,
    int CategoryId) : DomainEvent;

public sealed record TicketAssignedEvent(
    int TicketId,
    int? PreviousAgentId,
    int NewAgentId,
    TicketStatus NewStatus) : DomainEvent;

public sealed record TicketClosedEvent(
    int TicketId,
    int? AssignedToId,
    DateTimeOffset ClosedAt) : DomainEvent;

public sealed record TicketUpdatedEvent(
    int TicketId,
    string? OldTitle,
    string? NewTitle,
    TicketPriority OldPriority,
    TicketPriority NewPriority) : DomainEvent;

public sealed record TicketCommentAddedEvent(
    int TicketId,
    int CommentId,
    int AuthorId,
    string Content,
    DateTimeOffset CreatedAt) : DomainEvent;