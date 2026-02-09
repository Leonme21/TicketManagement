namespace TicketManagement.Domain.Exceptions;

/// <summary>
/// 🔥 SENIOR LEVEL: Specific domain exceptions for better error handling
/// Each exception represents a specific business rule violation
/// </summary>

public sealed class TicketNotFoundException : DomainException
{
    public TicketNotFoundException(int ticketId) 
        : base($"Ticket with ID {ticketId} was not found")
    {
        TicketId = ticketId;
    }

    public int TicketId { get; }
}

public sealed class TicketAlreadyClosedException : DomainException
{
    public TicketAlreadyClosedException(int ticketId) 
        : base($"Ticket {ticketId} is already closed and cannot be modified")
    {
        TicketId = ticketId;
    }

    public int TicketId { get; }
}

public sealed class TicketAssignmentException : DomainException
{
    public TicketAssignmentException(int ticketId, string reason) 
        : base($"Cannot assign ticket {ticketId}: {reason}")
    {
        TicketId = ticketId;
    }

    public int TicketId { get; }
}

public sealed class DailyTicketLimitExceededException : DomainException
{
    public DailyTicketLimitExceededException(int userId, int limit) 
        : base($"User {userId} has exceeded the daily ticket creation limit of {limit}")
    {
        UserId = userId;
        Limit = limit;
    }

    public int UserId { get; }
    public int Limit { get; }
}

public sealed class BusinessHoursViolationException : DomainException
{
    public BusinessHoursViolationException(string operation) 
        : base($"Operation '{operation}' is not allowed outside business hours")
    {
        Operation = operation;
    }

    public string Operation { get; }
}

public sealed class InvalidTicketStateException : DomainException
{
    public InvalidTicketStateException(int ticketId, string currentState, string attemptedAction) 
        : base($"Cannot perform '{attemptedAction}' on ticket {ticketId} in state '{currentState}'")
    {
        TicketId = ticketId;
        CurrentState = currentState;
        AttemptedAction = attemptedAction;
    }

    public int TicketId { get; }
    public string CurrentState { get; }
    public string AttemptedAction { get; }
}

public sealed class UnauthorizedTicketAccessException : DomainException
{
    public UnauthorizedTicketAccessException(int userId, int ticketId, string action) 
        : base($"User {userId} is not authorized to perform '{action}' on ticket {ticketId}")
    {
        UserId = userId;
        TicketId = ticketId;
        Action = action;
    }

    public int UserId { get; }
    public int TicketId { get; }
    public string Action { get; }
}

public sealed class CategoryNotFoundException : DomainException
{
    public CategoryNotFoundException(int categoryId) 
        : base($"Category with ID {categoryId} was not found or is inactive")
    {
        CategoryId = categoryId;
    }

    public int CategoryId { get; }
}

public sealed class SlaViolationException : DomainException
{
    public SlaViolationException(int ticketId, TimeSpan expectedResolution, TimeSpan actualTime) 
        : base($"Ticket {ticketId} violated SLA: expected {expectedResolution.TotalHours:F1}h, actual {actualTime.TotalHours:F1}h")
    {
        TicketId = ticketId;
        ExpectedResolution = expectedResolution;
        ActualTime = actualTime;
    }

    public int TicketId { get; }
    public TimeSpan ExpectedResolution { get; }
    public TimeSpan ActualTime { get; }
}