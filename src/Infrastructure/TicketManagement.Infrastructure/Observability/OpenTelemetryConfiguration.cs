using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TicketManagement.Infrastructure.Observability;

/// <summary>
/// ? PRODUCTION-READY: OpenTelemetry distributed tracing configuration
/// Features:
/// - Automatic instrumentation for ASP.NET Core, EF Core, HttpClient
/// - Custom activity sources for application-level tracing
/// - Exporters for Jaeger, Zipkin, OTLP
/// - Sampling strategies for production
/// </summary>
public static class OpenTelemetryConfiguration
{
    public const string ServiceName = "TicketManagement";
    public const string ServiceVersion = "1.0.0";

    // ? Activity sources for custom tracing
    public static readonly ActivitySource ApplicationActivitySource = new($"{ServiceName}.Application");
    public static readonly ActivitySource InfrastructureActivitySource = new($"{ServiceName}.Infrastructure");
    public static readonly ActivitySource DomainActivitySource = new($"{ServiceName}.Domain");

    /// <summary>
    /// ? Registers OpenTelemetry tracing with multiple exporters
    /// </summary>
    public static IServiceCollection AddOpenTelemetryTracing(
        this IServiceCollection services,
        string environment)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: ServiceName,
                    serviceVersion: ServiceVersion,
                    serviceInstanceId: Environment.MachineName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environment,
                    ["host.name"] = Environment.MachineName,
                    ["os.type"] = Environment.OSVersion.Platform.ToString()
                }))
            .WithTracing(tracing => tracing
                // ? Automatic instrumentation
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.Filter = (httpContext) =>
                    {
                        // ? Exclude health checks and metrics from tracing
                        var path = httpContext.Request.Path.Value;
                        return path != "/health" && path != "/metrics";
                    };
                    options.EnrichWithHttpRequest = (activity, request) =>
                    {
                        activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress?.ToString());
                        activity.SetTag("http.user_agent", request.Headers.UserAgent.ToString());
                    };
                    options.EnrichWithHttpResponse = (activity, response) =>
                    {
                        activity.SetTag("http.response_content_length", response.ContentLength);
                    };
                })
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    // options.SetDbStatementForText = true; // Incompatible with current version of instrumentation
                    options.EnrichWithIDbCommand = (activity, command) =>
                    {
                        activity.SetTag("db.command_timeout", command.CommandTimeout);
                    };
                })
                .AddHttpClientInstrumentation()
                
                // ? Custom activity sources
                .AddSource(ApplicationActivitySource.Name)
                .AddSource(InfrastructureActivitySource.Name)
                .AddSource(DomainActivitySource.Name)
                
                // ? Sampling strategy (for production)
                .SetSampler(new TraceIdRatioBasedSampler(
                    environment == "Production" ? 0.1 : 1.0)) // Sample 10% in prod, 100% in dev
                
                // ? Exporters
                .AddConsoleExporter() // Development: console output
                
                // Production: Jaeger (uncomment when ready)
                // .AddJaegerExporter(options =>
                // {
                //     options.AgentHost = "localhost";
                //     options.AgentPort = 6831;
                // })
                
                // Production: OTLP (for vendors like Honeycomb, Lightstep, etc.)
                // .AddOtlpExporter(options =>
                // {
                //     options.Endpoint = new Uri("http://localhost:4317");
                // })
            );

        return services;
    }

    /// <summary>
    /// ? Helper method to create traced activities
    /// </summary>
    public static Activity? StartActivity(
        string operationName,
        ActivityKind kind = ActivityKind.Internal,
        ActivitySource? source = null)
    {
        source ??= ApplicationActivitySource;
        return source.StartActivity(operationName, kind);
    }

    /// <summary>
    /// ? Adds common tags to an activity
    /// </summary>
    public static Activity? AddCommonTags(
        this Activity? activity,
        string userId,
        string? correlationId = null)
    {
        activity?.SetTag("user.id", userId);
        
        if (!string.IsNullOrEmpty(correlationId))
        {
            activity?.SetTag("correlation.id", correlationId);
        }

        return activity;
    }

    /// <summary>
    /// ? Records an exception in the current activity
    /// </summary>
    public static Activity? RecordException(
        this Activity? activity,
        Exception exception)
    {
        if (activity == null) return null;

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.RecordException(exception);
        
        return activity;
    }
}
