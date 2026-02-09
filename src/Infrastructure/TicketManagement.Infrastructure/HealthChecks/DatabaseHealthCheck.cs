using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TicketManagement.Infrastructure.Persistence;

namespace TicketManagement.Infrastructure.HealthChecks;

/// <summary>
/// ✅ NEW: Detailed database health check with connection and query validation
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DatabaseHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if database can be connected
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy(
                    "Cannot connect to database",
                    data: new Dictionary<string, object>
                    {
                        ["database"] = _context.Database.GetConnectionString() ?? "unknown"
                    });
            }

            // Execute a simple query to verify database is responsive
            var ticketCount = await _context.Tickets.CountAsync(cancellationToken);

            return HealthCheckResult.Healthy(
                "Database is healthy",
                data: new Dictionary<string, object>
                {
                    ["database"] = _context.Database.GetConnectionString() ?? "unknown",
                    ["ticketCount"] = ticketCount,
                    ["timestamp"] = DateTimeOffset.UtcNow
                });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database health check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                });
        }
    }
}
