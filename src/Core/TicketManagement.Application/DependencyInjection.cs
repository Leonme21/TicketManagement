using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TicketManagement.Application.Common.Behaviors;
using Microsoft.Extensions.Configuration;

namespace TicketManagement.Application;

/// <summary>
/// 🔥 BIG TECH LEVEL: Complete MediatR pipeline with all essential behaviors
/// Pipeline order: Logging -> Idempotency -> RateLimiting -> Validation -> Transaction
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // ✅ COMPLETE: MediatR with all essential behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            
            // ✅ Pipeline Order matters:
            // 1. Logging - Always first to capture all requests
            // 2. Idempotency - Check for duplicate requests before processing (only for IIdempotentCommand)
            // 3. RateLimiting - Check rate limits (only for IRateLimitedRequest)
            // 4. Validation - Validate request data
            // 5. Transaction - Wrap in transaction
            
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RateLimitingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        });

        return services;
    }
}
