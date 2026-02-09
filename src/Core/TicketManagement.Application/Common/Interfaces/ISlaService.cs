using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// 🔥 PRODUCTION-READY: SLA (Service Level Agreement) service interface
/// Provides business-critical SLA calculations and monitoring
/// </summary>
public interface ISlaService
{
    /// <summary>
    /// Calculates estimated resolution time based on priority and category
    /// </summary>
    Task<TimeSpan> CalculateEstimatedResolutionAsync(TicketPriority priority, int categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a ticket is approaching SLA breach
    /// </summary>
    Task<SlaStatus> CheckSlaStatusAsync(int ticketId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets SLA metrics for reporting
    /// </summary>
    Task<SlaMetrics> GetSlaMetricsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculates business hours between two dates
    /// </summary>
    TimeSpan CalculateBusinessHours(DateTimeOffset start, DateTimeOffset end);
}

public record SlaStatus
{
    public bool IsBreached { get; init; }
    public bool IsAtRisk { get; init; }
    public TimeSpan TimeRemaining { get; init; }
    public TimeSpan TimeElapsed { get; init; }
    public DateTimeOffset DueDate { get; init; }
}

public record SlaMetrics
{
    public int TotalTickets { get; init; }
    public int TicketsWithinSla { get; init; }
    public int TicketsBreachedSla { get; init; }
    public double SlaCompliancePercentage { get; init; }
    public TimeSpan AverageResolutionTime { get; init; }
    public Dictionary<TicketPriority, TimeSpan> AverageResolutionByPriority { get; init; } = new();
}