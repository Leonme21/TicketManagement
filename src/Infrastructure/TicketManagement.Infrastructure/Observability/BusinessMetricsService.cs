using System.Diagnostics.Metrics;

namespace TicketManagement.Infrastructure.Observability;

/// <summary>
/// ✅ NEW: Business metrics service for tracking KPIs
/// Tracks tickets created, SLA compliance, resolution times, etc.
/// </summary>
public interface IBusinessMetricsService
{
    void RecordTicketCreated(string priority);
    void RecordTicketClosed(string priority, TimeSpan resolutionTime);
    void RecordSlaViolation(string priority);
    void RecordSlaCompliance(string priority);
}

public sealed class BusinessMetricsService : IBusinessMetricsService
{
    private readonly Counter<long> _ticketsCreatedCounter;
    private readonly Counter<long> _ticketsClosedCounter;
    private readonly Histogram<double> _resolutionTimeHistogram;
    private readonly Counter<long> _slaViolationsCounter;
    private readonly Counter<long> _slaComplianceCounter;

    public BusinessMetricsService(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("TicketManagement.Business");

        _ticketsCreatedCounter = meter.CreateCounter<long>(
            "tickets.created",
            description: "Number of tickets created");

        _ticketsClosedCounter = meter.CreateCounter<long>(
            "tickets.closed",
            description: "Number of tickets closed");

        _resolutionTimeHistogram = meter.CreateHistogram<double>(
            "tickets.resolution_time",
            unit: "hours",
            description: "Time taken to resolve tickets");

        _slaViolationsCounter = meter.CreateCounter<long>(
            "tickets.sla_violations",
            description: "Number of SLA violations");

        _slaComplianceCounter = meter.CreateCounter<long>(
            "tickets.sla_compliance",
            description: "Number of tickets meeting SLA");
    }

    public void RecordTicketCreated(string priority)
    {
        _ticketsCreatedCounter.Add(1, new KeyValuePair<string, object?>("priority", priority));
    }

    public void RecordTicketClosed(string priority, TimeSpan resolutionTime)
    {
        _ticketsClosedCounter.Add(1, new KeyValuePair<string, object?>("priority", priority));
        _resolutionTimeHistogram.Record(resolutionTime.TotalHours, new KeyValuePair<string, object?>("priority", priority));
    }

    public void RecordSlaViolation(string priority)
    {
        _slaViolationsCounter.Add(1, new KeyValuePair<string, object?>("priority", priority));
    }

    public void RecordSlaCompliance(string priority)
    {
        _slaComplianceCounter.Add(1, new KeyValuePair<string, object?>("priority", priority));
    }
}
