using System;
using System.Collections.Generic;
using System.Linq;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.ValueObjects;

namespace TicketManagement.Domain.Entities;

/// <summary>
/// Ticket Aggregate Root
/// ✅ REFACTORED: Uses simplified AggregateRoot
/// </summary>
public class Ticket : AggregateRoot, ISoftDeletable
{
    // ==================== CONSTRUCTORS ====================
    private Ticket() { }

    private Ticket(TicketTitle title, TicketDescription description, TicketPriority priority, int categoryId, int creatorId)
    {
        Title = title;
        Description = description;
        Priority = priority;
        CategoryId = categoryId;
        CreatorId = creatorId;
        Status = TicketStatus.Open;
    }

    // ==================== FACTORY METHOD ====================
    public static Result<Ticket> Create(string title, string description, TicketPriority priority, int categoryId, int creatorId)
    {
        var titleResult = TicketTitle.Create(title);
        if (titleResult.IsFailure) 
            return Result.Failure<Ticket>(titleResult.Error);

        var descriptionResult = TicketDescription.Create(description);
        if (descriptionResult.IsFailure) 
            return Result.Failure<Ticket>(descriptionResult.Error);

        if (categoryId <= 0) 
            return Result.Failure<Ticket>(DomainErrors.Ticket.InvalidCategoryId);
        
        if (creatorId <= 0) 
            return Result.Failure<Ticket>(DomainErrors.Ticket.InvalidCreatorId);

        var ticket = new Ticket(titleResult.Value!, descriptionResult.Value!, priority, categoryId, creatorId);

        // Emit domain event
        ticket.AddDomainEvent(new Events.TicketCreatedEvent(
            ticket.Id, 
            titleResult.Value!,
            creatorId,
            priority,
            categoryId
        ));
        
        return Result.Success(ticket);
    }

    // ==================== PROPERTIES ====================
    public TicketTitle Title { get; private set; } = null!;
    public TicketDescription Description { get; private set; } = null!;
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }

    public int CreatorId { get; private set; }
    public int? AssignedToId { get; private set; }
    public int CategoryId { get; private set; }

    // Navigation properties (for EF Core)
    public User Creator { get; private set; } = null!;
    public User? AssignedTo { get; private set; }
    public Category Category { get; private set; } = null!;

    private readonly List<Comment> _comments = new();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    private readonly List<Attachment> _attachments = new();
    public IReadOnlyCollection<Attachment> Attachments => _attachments.AsReadOnly();

    private readonly List<Tag> _tags = new();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // ==================== BUSINESS LOGIC ====================

    public Result Assign(int agentId)
    {
        if (Status == TicketStatus.Closed)
            return Result.Failure(DomainErrors.Ticket.CannotAssignClosed);

        if (agentId <= 0)
            return Result.Failure(DomainErrors.Ticket.InvalidAgentId);

        var previousAgentId = AssignedToId;
        AssignedToId = agentId;

        if (Status == TicketStatus.Open)
            Status = TicketStatus.InProgress;

        AddDomainEvent(new Events.TicketAssignedEvent(
            Id,
            previousAgentId,
            agentId,
            Status
        ));

        return Result.Success();
    }

    public Result Unassign()
    {
        if (Status == TicketStatus.Closed)
            return Result.Failure(DomainErrors.Ticket.CannotAssignClosed);

        var previousAgentId = AssignedToId;
        AssignedToId = null;
        Status = TicketStatus.Open;

        if (previousAgentId.HasValue)
        {
            AddDomainEvent(new Events.TicketAssignedEvent(
                Id,
                previousAgentId,
                0,
                Status
            ));
        }

        return Result.Success();
    }

    public Result UpdateStatus(TicketStatus newStatus)
    {
        if (Status == TicketStatus.Closed && newStatus != TicketStatus.Reopened)
            return Result.Failure(DomainErrors.Ticket.InvalidStatusTransition);

        if (newStatus == TicketStatus.InProgress && AssignedToId == null)
            return Result.Failure(DomainErrors.Ticket.NotAssigned);

        Status = newStatus;
        return Result.Success();
    }

    public Result Resolve()
    {
        if (Status == TicketStatus.Closed)
            return Result.Failure(DomainErrors.Ticket.AlreadyClosed);

        if (AssignedToId == null)
            return Result.Failure(DomainErrors.Ticket.CannotResolveUnassigned);

        Status = TicketStatus.Resolved;
        return Result.Success();
    }

    public Result Close()
    {
        if (Status == TicketStatus.Closed)
            return Result.Failure(DomainErrors.Ticket.CannotCloseClosed);

        Status = TicketStatus.Closed;

        AddDomainEvent(new Events.TicketClosedEvent(
            Id,
            AssignedToId,
            DateTimeOffset.UtcNow
        ));

        return Result.Success();
    }

    public Result Reopen()
    {
        if (Status != TicketStatus.Closed)
            return Result.Failure(DomainErrors.Ticket.CannotReopenNotClosed);

        Status = TicketStatus.Reopened;
        return Result.Success();
    }

    public Result Update(string title, string description, TicketPriority priority)
    {
        if (Status == TicketStatus.Closed)
            return Result.Failure(DomainErrors.Ticket.CannotUpdateClosed);

        var titleResult = TicketTitle.Create(title);
        if (titleResult.IsFailure) 
            return Result.Failure(titleResult.Error);

        var descriptionResult = TicketDescription.Create(description);
        if (descriptionResult.IsFailure) 
            return Result.Failure(descriptionResult.Error);

        var oldTitle = Title.Value;
        var oldPriority = Priority;

        Title = titleResult.Value!;
        Description = descriptionResult.Value!;
        Priority = priority;

        if (oldTitle != title || oldPriority != priority)
        {
            AddDomainEvent(new Events.TicketUpdatedEvent(
                Id,
                oldTitle,
                title,
                oldPriority,
                priority
            ));
        }

        return Result.Success();
    }

    public Result ChangePriority(TicketPriority newPriority)
    {
        if (Status == TicketStatus.Closed)
            return Result.Failure(DomainErrors.Ticket.CannotUpdateClosed);

        var oldPriority = Priority;
        Priority = newPriority;

        if (oldPriority != newPriority)
        {
            AddDomainEvent(new Events.TicketUpdatedEvent(
                Id,
                null,
                null,
                oldPriority,
                newPriority
            ));
        }

        return Result.Success();
    }

    public Result ChangeCategory(int newCategoryId)
    {
        if (Status == TicketStatus.Closed)
            return Result.Failure(DomainErrors.Ticket.CannotUpdateClosed);

        if (newCategoryId <= 0)
            return Result.Failure(DomainErrors.Ticket.InvalidCategoryId);

        CategoryId = newCategoryId;
        return Result.Success();
    }

    // Domain comment addition
    public Result AddComment(string content, int authorId, bool isInternal = false)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure(DomainErrors.Comment.InvalidContent);

        if (content.Length > 2000)
            return Result.Failure(DomainErrors.Comment.ContentTooLong);

        if (authorId <= 0)
            return Result.Failure(DomainErrors.Comment.InvalidAuthorId);

        var comment = Comment.Create(content, Id, authorId, isInternal);
        if (comment.IsFailure)
            return Result.Failure(comment.Error);

        _comments.Add(comment.Value!);

        AddDomainEvent(new Events.TicketCommentAddedEvent(
            Id,
            comment.Value!.Id,
            authorId,
            content,
            DateTimeOffset.UtcNow
        ));

        return Result.Success();
    }

    public void AddAttachment(Attachment attachment)
    {
        if (attachment == null) return;
        _attachments.Add(attachment);
    }

    public Result AddTag(Tag tag)
    {
        if (tag == null) 
            return Result.Failure(Error.NullValue);
        
        if (_tags.Any(t => t.Id == tag.Id)) 
            return Result.Failure(DomainErrors.Tag.TagAlreadyAdded);

        _tags.Add(tag);
        return Result.Success();
    }

    public void RemoveTag(Tag tag)
    {
        if (tag == null) return;
        _tags.Remove(tag);
    }

    // Business rule validation methods
    public bool CanBeAssignedTo(int agentId)
    {
        return Status != TicketStatus.Closed && agentId > 0;
    }

    public bool CanBeUpdatedBy(int userId)
    {
        return Status != TicketStatus.Closed && 
               (CreatorId == userId || AssignedToId == userId);
    }

    public bool CanBeClosedBy(int userId)
    {
        return Status != TicketStatus.Closed && AssignedToId.HasValue;
    }


}
