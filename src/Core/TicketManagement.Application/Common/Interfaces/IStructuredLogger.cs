namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
/// 🔥 PRODUCTION-READY: Structured logging interface for business operations
/// Provides contextual logging with business semantics
/// </summary>
public interface IStructuredLogger
{
    /// <summary>
    /// Creates a logging scope with business context
    /// </summary>
    IDisposable BeginBusinessScope(string operation, object context);
    
    /// <summary>
    /// Logs business operation success
    /// </summary>
    void LogBusinessSuccess(string operation, object? data = null);
    
    /// <summary>
    /// Logs business operation failure
    /// </summary>
    void LogBusinessFailure(string operation, string reason, object? context = null);
    
    /// <summary>
    /// Logs security-related events
    /// </summary>
    void LogSecurityEvent(string eventType, string description, object? context = null);
    
    /// <summary>
    /// Logs performance metrics
    /// </summary>
    void LogPerformanceMetric(string operation, TimeSpan duration, object? metadata = null);
}