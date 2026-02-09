using Prometheus;

namespace TicketManagement.Infrastructure.Observability;

/// <summary>
/// ? PRODUCTION-READY: Custom Prometheus metrics for business and technical monitoring
/// </summary>
public class ApplicationMetrics
{
    // ==================== BUSINESS METRICS ====================
    
    /// <summary>
    /// Total tickets created
    /// </summary>
    public static readonly Counter TicketsCreated = Metrics
        .CreateCounter(
            "tickets_created_total",
            "Total number of tickets created",
            new CounterConfiguration
            {
                LabelNames = new[] { "priority", "category" }
            });

    /// <summary>
    /// Total tickets closed
    /// </summary>
    public static readonly Counter TicketsClosed = Metrics
        .CreateCounter(
            "tickets_closed_total",
            "Total number of tickets closed",
            new CounterConfiguration
            {
                LabelNames = new[] { "priority", "category" }
            });

    /// <summary>
    /// Total tickets assigned
    /// </summary>
    public static readonly Counter TicketsAssigned = Metrics
        .CreateCounter(
            "tickets_assigned_total",
            "Total number of tickets assigned to agents",
            new CounterConfiguration
            {
                LabelNames = new[] { "agent_id" }
            });

    /// <summary>
    /// Active tickets by status
    /// </summary>
    public static readonly Gauge ActiveTickets = Metrics
        .CreateGauge(
            "tickets_active",
            "Current number of active tickets",
            new GaugeConfiguration
            {
                LabelNames = new[] { "status", "priority" }
            });

    /// <summary>
    /// Ticket resolution time in seconds
    /// </summary>
    public static readonly Histogram TicketResolutionTime = Metrics
        .CreateHistogram(
            "ticket_resolution_time_seconds",
            "Time taken to resolve tickets",
            new HistogramConfiguration
            {
                LabelNames = new[] { "priority", "category" },
                Buckets = Histogram.ExponentialBuckets(60, 2, 10) // 1min, 2min, 4min, 8min, etc.
            });

    // ==================== PERFORMANCE METRICS ====================
    
    /// <summary>
    /// Query execution duration
    /// </summary>
    public static readonly Histogram QueryDuration = Metrics
        .CreateHistogram(
            "query_duration_seconds",
            "Duration of database queries",
            new HistogramConfiguration
            {
                LabelNames = new[] { "query_name", "entity" },
                Buckets = Histogram.ExponentialBuckets(0.001, 2, 15) // 1ms to ~32s
            });

    /// <summary>
    /// Command execution duration
    /// </summary>
    public static readonly Histogram CommandDuration = Metrics
        .CreateHistogram(
            "command_duration_seconds",
            "Duration of command execution",
            new HistogramConfiguration
            {
                LabelNames = new[] { "command_name", "success" },
                Buckets = Histogram.ExponentialBuckets(0.01, 2, 12) // 10ms to ~40s
            });

    /// <summary>
    /// Cache hit/miss counter
    /// </summary>
    public static readonly Counter CacheOperations = Metrics
        .CreateCounter(
            "cache_operations_total",
            "Total cache operations",
            new CounterConfiguration
            {
                LabelNames = new[] { "operation", "result" } // operation: get/set, result: hit/miss/error
            });

    /// <summary>
    /// Cache hit rate (derived metric)
    /// </summary>
    public static readonly Gauge CacheHitRate = Metrics
        .CreateGauge(
            "cache_hit_rate",
            "Cache hit rate percentage (0-100)");

    // ==================== RESILIENCE METRICS ====================
    
    /// <summary>
    /// Circuit breaker state changes
    /// </summary>
    public static readonly Counter CircuitBreakerStateChanges = Metrics
        .CreateCounter(
            "circuit_breaker_state_changes_total",
            "Total circuit breaker state changes",
            new CounterConfiguration
            {
                LabelNames = new[] { "pipeline", "state" } // state: open/closed/half-open
            });

    /// <summary>
    /// Retry attempts
    /// </summary>
    public static readonly Counter RetryAttempts = Metrics
        .CreateCounter(
            "retry_attempts_total",
            "Total retry attempts",
            new CounterConfiguration
            {
                LabelNames = new[] { "pipeline", "attempt" }
            });

    // ==================== EVENT PROCESSING METRICS ====================
    
    /// <summary>
    /// Domain events published
    /// </summary>
    public static readonly Counter DomainEventsPublished = Metrics
        .CreateCounter(
            "domain_events_published_total",
            "Total domain events published",
            new CounterConfiguration
            {
                LabelNames = new[] { "event_type" }
            });

    /// <summary>
    /// Domain events processed
    /// </summary>
    public static readonly Counter DomainEventsProcessed = Metrics
        .CreateCounter(
            "domain_events_processed_total",
            "Total domain events processed",
            new CounterConfiguration
            {
                LabelNames = new[] { "event_type", "status" } // status: success/failed
            });

    /// <summary>
    /// Outbox message processing lag (seconds)
    /// </summary>
    public static readonly Gauge OutboxProcessingLag = Metrics
        .CreateGauge(
            "outbox_processing_lag_seconds",
            "Time difference between event creation and processing");

    // ==================== ERROR METRICS ====================
    
    /// <summary>
    /// Application errors
    /// </summary>
    public static readonly Counter ApplicationErrors = Metrics
        .CreateCounter(
            "application_errors_total",
            "Total application errors",
            new CounterConfiguration
            {
                LabelNames = new[] { "error_type", "severity" }
            });

    /// <summary>
    /// Validation failures
    /// </summary>
    public static readonly Counter ValidationFailures = Metrics
        .CreateCounter(
            "validation_failures_total",
            "Total validation failures",
            new CounterConfiguration
            {
                LabelNames = new[] { "command_type", "field" }
            });

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Records a ticket creation
    /// </summary>
    public static void RecordTicketCreated(string priority, string category)
    {
        TicketsCreated.WithLabels(priority, category).Inc();
    }

    /// <summary>
    /// Records a ticket closure
    /// </summary>
    public static void RecordTicketClosed(string priority, string category)
    {
        TicketsClosed.WithLabels(priority, category).Inc();
    }

    /// <summary>
    /// Records ticket resolution time
    /// </summary>
    public static void RecordTicketResolution(TimeSpan duration, string priority, string category)
    {
        TicketResolutionTime.WithLabels(priority, category).Observe(duration.TotalSeconds);
    }

    /// <summary>
    /// Records cache hit
    /// </summary>
    public static void RecordCacheHit()
    {
        CacheOperations.WithLabels("get", "hit").Inc();
    }

    /// <summary>
    /// Records cache miss
    /// </summary>
    public static void RecordCacheMiss()
    {
        CacheOperations.WithLabels("get", "miss").Inc();
    }

    /// <summary>
    /// Records cache error
    /// </summary>
    public static void RecordCacheError()
    {
        CacheOperations.WithLabels("get", "error").Inc();
    }

    /// <summary>
    /// Measures query duration
    /// </summary>
    public static IDisposable MeasureQueryDuration(string queryName, string entity)
    {
        return QueryDuration.WithLabels(queryName, entity).NewTimer();
    }

    /// <summary>
    /// Measures command duration
    /// </summary>
    public static IDisposable MeasureCommandDuration(string commandName, bool success)
    {
        return CommandDuration.WithLabels(commandName, success.ToString()).NewTimer();
    }
}
