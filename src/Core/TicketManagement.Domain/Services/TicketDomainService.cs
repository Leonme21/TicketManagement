using TicketManagement.Domain.Common;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Domain.Services;

/// <summary>
/// Ticket domain service
/// </summary>
public sealed class TicketDomainService : ITicketDomainService
{
    private readonly BusinessRulesConfiguration _businessRules;
    private readonly SlaConfiguration _slaConfig;

    public TicketDomainService(
        BusinessRulesConfiguration businessRules,
        SlaConfiguration slaConfig)
    {
        _businessRules = businessRules;
        _slaConfig = slaConfig;
    }

    /// <summary>
    /// Default constructor with sensible defaults for testing
    /// </summary>
    public TicketDomainService() : this(
        new BusinessRulesConfiguration(
            MaxTicketsPerUserPerDay: 10,
            MaxCriticalTicketsPerUserPerDay: 2,
            AllowTicketCreationOutsideBusinessHours: true,
            BusinessHoursStart: 8,
            BusinessHoursEnd: 18,
            DuplicateCheckHours: 24),
        new SlaConfiguration(
            CriticalHours: 2,
            HighHours: 8,
            MediumHours: 24,
            LowHours: 72))
    {
    }

    public Result ValidateTicketCreation(TicketCreationData data)
    {
        // Validate daily ticket limits per user
        var dailyLimitResult = ValidateDailyTicketLimit(data.UserId, data.Priority, data.UserTicketsToday, data.UserCriticalTicketsToday);
        if (dailyLimitResult.IsFailure)
            return dailyLimitResult;

        // Validate business hours for critical tickets
        var businessHoursResult = ValidateBusinessHours(data.Priority, data.CreationTime);
        if (businessHoursResult.IsFailure)
            return businessHoursResult;

        // Validate recent ticket duplicates
        var duplicateResult = ValidateDuplicateTickets(data.UserRecentTicketsCount);
        if (duplicateResult.IsFailure)
            return duplicateResult;

        return Result.Success();
    }

    public Result ValidateTicketUpdate(TicketUpdateData data)
    {
        // Only creator, assigned agent, or admin can update
        if (data.RequestingUserId != data.CreatorId && 
            data.RequestingUserId != data.AssignedToId &&
            data.RequestingUserRole != UserRole.Admin && 
            data.RequestingUserRole != UserRole.Agent)
        {
            return Result.Forbidden("You don't have permission to update this ticket");
        }

        // Cannot update closed tickets
        if (data.CurrentStatus == TicketStatus.Closed)
            return Result.Invalid("Cannot update a closed ticket");

        return Result.Success();
    }

    public Result ValidateTicketAssignment(TicketAssignmentData data)
    {
        // Validate that requesting user has permissions
        if (data.RequestingUserRole != UserRole.Admin && data.RequestingUserRole != UserRole.Agent)
            return Result.Forbidden("Only agents and admins can assign tickets");

        // Validate that target is a valid agent
        if (data.TargetAgentRole != UserRole.Agent && data.TargetAgentRole != UserRole.Admin)
            return Result.Invalid("Target user is not an agent or admin");

        // Cannot assign closed tickets
        if (data.CurrentTicketStatus == TicketStatus.Closed)
            return Result.Invalid("Cannot assign a closed ticket");

        return Result.Success();
    }

    public TimeSpan CalculateEstimatedResolutionTime(TicketPriority priority, CategoryType categoryType, int currentSystemLoad)
    {
        // Get base SLA by priority
        var baseSlaHours = GetBaseSlaHours(priority);
        
        // Adjust by category
        var categoryMultiplier = GetCategoryMultiplier(categoryType);
        
        // Adjust by current system load
        var workloadMultiplier = GetWorkloadMultiplier(currentSystemLoad);
        
        var totalHours = baseSlaHours * categoryMultiplier * workloadMultiplier;
        return TimeSpan.FromHours(totalHours);
    }

    public bool IsTicketAtRisk(TicketRiskData data)
    {
        if (data.Status == TicketStatus.Closed)
            return false;

        var slaHours = GetBaseSlaHours(data.Priority);
        var slaDeadline = data.CreatedAt.AddHours(slaHours);
        var timeRemaining = slaDeadline - data.CurrentTime;
        
        // Consider at risk if less than 25% of SLA time remains
        var riskThreshold = TimeSpan.FromHours(slaHours * 0.25);
        return timeRemaining <= riskThreshold;
    }

    public bool IsWithinBusinessHours(DateTime dateTime)
    {
        if (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday)
            return false;

        return dateTime.Hour >= _businessRules.BusinessHoursStart && 
               dateTime.Hour < _businessRules.BusinessHoursEnd;
    }

    private Result ValidateDailyTicketLimit(int userId, TicketPriority priority, int userTicketsToday, int userCriticalTicketsToday)
    {
        if (userTicketsToday >= _businessRules.MaxTicketsPerUserPerDay)
            return Result.RateLimitExceeded($"Daily ticket limit of {_businessRules.MaxTicketsPerUserPerDay} exceeded. Please try again tomorrow.");

        if (priority == TicketPriority.Critical && userCriticalTicketsToday >= _businessRules.MaxCriticalTicketsPerUserPerDay)
            return Result.RateLimitExceeded($"Daily critical ticket limit of {_businessRules.MaxCriticalTicketsPerUserPerDay} exceeded.");

        return Result.Success();
    }

    private Result ValidateBusinessHours(TicketPriority priority, DateTime creationTime)
    {
        // If creation outside business hours is allowed or it's not critical, allow it
        if (_businessRules.AllowTicketCreationOutsideBusinessHours || priority != TicketPriority.Critical)
            return Result.Success();

        if (!IsWithinBusinessHours(creationTime))
            return Result.Invalid("Critical tickets can only be created during business hours (Monday-Friday, 8AM-6PM)");

        return Result.Success();
    }

    private Result ValidateDuplicateTickets(int userRecentTicketsCount)
    {
        if (userRecentTicketsCount >= 3)
            return Result.RateLimitExceeded($"Too many tickets created in the last {_businessRules.DuplicateCheckHours} hours. Please wait before creating another ticket.");

        return Result.Success();
    }

    private double GetBaseSlaHours(TicketPriority priority)
    {
        return priority switch
        {
            TicketPriority.Critical => _slaConfig.CriticalHours,
            TicketPriority.High => _slaConfig.HighHours,
            TicketPriority.Medium => _slaConfig.MediumHours,
            TicketPriority.Low => _slaConfig.LowHours,
            _ => _slaConfig.MediumHours
        };
    }

    private static double GetCategoryMultiplier(CategoryType categoryType)
    {
        return categoryType switch
        {
            CategoryType.Technical => 1.0,
            CategoryType.Billing => 0.8,
            CategoryType.General => 1.2,
            _ => 1.0
        };
    }

    private static double GetWorkloadMultiplier(int activeTickets)
    {
        return activeTickets switch
        {
            < 50 => 1.0,      // Normal load
            < 100 => 1.2,     // Medium load - 20% more time
            < 200 => 1.5,     // High load - 50% more time
            _ => 2.0          // Critical load - double time
        };
    }
}
