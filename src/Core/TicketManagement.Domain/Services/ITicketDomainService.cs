using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Domain.Services;

/// <summary>
/// 🔥 BIG TECH LEVEL: Pure domain service interface without external dependencies
/// All business rules are encapsulated and testable in isolation
/// </summary>
public interface ITicketDomainService
{
    /// <summary>
    /// Validates if a ticket can be created based on business rules
    /// </summary>
    Result ValidateTicketCreation(TicketCreationData data);

    /// <summary>
    /// Validates if a ticket can be updated by the requesting user
    /// </summary>
    Result ValidateTicketUpdate(TicketUpdateData data);

    /// <summary>
    /// Validates if a ticket can be assigned to an agent
    /// </summary>
    Result ValidateTicketAssignment(TicketAssignmentData data);

    /// <summary>
    /// Calculates estimated resolution time based on priority, category and system load
    /// </summary>
    TimeSpan CalculateEstimatedResolutionTime(TicketPriority priority, CategoryType categoryType, int currentSystemLoad);

    /// <summary>
    /// Determines if a ticket is at risk of SLA violation
    /// </summary>
    bool IsTicketAtRisk(TicketRiskData data);

    /// <summary>
    /// Checks if the given time is within business hours
    /// </summary>
    bool IsWithinBusinessHours(DateTime dateTime);
}

// 🔥 BIG TECH LEVEL: Pure data structures for domain service
public record TicketCreationData(
    int UserId,
    TicketPriority Priority,
    DateTime CreationTime,
    int UserTicketsToday,
    int UserCriticalTicketsToday,
    int UserRecentTicketsCount);

public record TicketUpdateData(
    int RequestingUserId,
    UserRole RequestingUserRole,
    int CreatorId,
    int? AssignedToId,
    TicketStatus CurrentStatus);

public record TicketAssignmentData(
    UserRole RequestingUserRole,
    UserRole TargetAgentRole,
    TicketStatus CurrentTicketStatus);

public record TicketRiskData(
    TicketPriority Priority,
    TicketStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset CurrentTime);

public record BusinessRulesConfiguration(
    int MaxTicketsPerUserPerDay,
    int MaxCriticalTicketsPerUserPerDay,
    bool AllowTicketCreationOutsideBusinessHours,
    int BusinessHoursStart,
    int BusinessHoursEnd,
    int DuplicateCheckHours);

public record SlaConfiguration(
    double CriticalHours,
    double HighHours,
    double MediumHours,
    double LowHours);

public enum CategoryType
{
    Technical,
    Billing,
    General
}