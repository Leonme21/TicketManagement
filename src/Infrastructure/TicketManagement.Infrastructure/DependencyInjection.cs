using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Interfaces;
using TicketManagement.Infrastructure.Identity;
using TicketManagement.Infrastructure.Persistence;
using TicketManagement.Infrastructure.Persistence.Interceptors;
using TicketManagement.Infrastructure.Services;
using TicketManagement.Infrastructure.Persistence.Repositories;
using TicketManagement.Infrastructure.Logging;
using TicketManagement.Infrastructure.Observability;
using TicketManagement.Infrastructure.Caching;
using TicketManagement.Infrastructure.Queries;
using TicketManagement.Domain.Services;
using TicketManagement.Infrastructure.Resilience;
using TicketManagement.Infrastructure.FeatureFlags;
using TicketManagement.Infrastructure.HealthChecks;
using TicketManagement.Application.Common.Authorization;

namespace TicketManagement.Infrastructure;

/// <summary>
/// ✅ BIG TECH LEVEL: Infrastructure layer registration with ISP-compliant interfaces
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddAuthentication(services, configuration);
        AddEssentialServices(services);
        AddCaching(services, configuration);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        // Interceptors
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<OutboxInterceptor>();

        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // ✅ SECURITY: Get connection string from environment first
            var connectionString = Environment.GetEnvironmentVariable("TICKETMGMT_ConnectionStrings__DefaultConnection")
                ?? configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string is required. " +
                    "Set via User Secrets, environment variable TICKETMGMT_ConnectionStrings__DefaultConnection, or appsettings.json");
            }
            
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mySqlOptions =>
                {
                    mySqlOptions.CommandTimeout(30);
                    mySqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                });

            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development");
        });

        // CQRS Pattern - Separate read and write
        // Write Side (Repositories)
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        
        // ✅ ISP: Register segregated query interfaces
        // Single implementation, multiple interfaces (Interface Segregation Principle)
        services.AddScoped<TicketQueryService>();
        services.AddScoped<ITicketQueryService>(sp => sp.GetRequiredService<TicketQueryService>());
        services.AddScoped<ITicketStatisticsService>(sp => sp.GetRequiredService<TicketQueryService>());
        services.AddScoped<ITicketListQueryService>(sp => sp.GetRequiredService<TicketQueryService>());
        services.AddScoped<ITicketPaginatedQueryService>(sp => sp.GetRequiredService<TicketQueryService>());
        services.AddScoped<ITicketDetailsQueryService>(sp => sp.GetRequiredService<TicketQueryService>());
        
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ApplicationDbContextInitializer>();
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        // JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // ✅ SECURITY: Get secret from environment first
            var jwtSecret = Environment.GetEnvironmentVariable("TICKETMGMT_JwtSettings__Secret")
                ?? configuration["JwtSettings:Secret"];
            
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException(
                    "JWT secret is required. " +
                    "Set via User Secrets, environment variable TICKETMGMT_JwtSettings__Secret, or appsettings.json");
            }

            var issuer = configuration["JwtSettings:Issuer"] ?? "TicketManagement";
            var audience = configuration["JwtSettings:Audience"] ?? "TicketManagement";

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.IsAgentOrAdmin, policy =>
                policy.RequireRole(Roles.Agent, Roles.Admin));

            options.AddPolicy(Policies.IsAdmin, policy =>
                policy.RequireRole(Roles.Admin));

            options.AddPolicy(Policies.CanAssignTickets, policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole(Roles.Agent) || context.User.IsInRole(Roles.Admin)));

            options.AddPolicy(Policies.CanDeleteTickets, policy =>
                policy.RequireRole(Roles.Admin));
        });

        // Identity Services
        services.AddHttpContextAccessor();
        services.AddSingleton<PasswordHasher>();
        services.AddSingleton<JwtTokenGenerator>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
    }

    private static void AddEssentialServices(IServiceCollection services)
    {
        // Essential Services
        services.AddTransient<IDateTime, DateTimeService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IStructuredLogger, StructuredLogger>();
        services.AddSingleton<IMetricsService, MetricsService>();
        
        // Business metrics
        services.AddSingleton<IBusinessMetricsService, BusinessMetricsService>();
        
        // Circuit breaker
        services.AddSingleton<ICircuitBreakerService, CircuitBreakerService>();
        
        // Feature flags
        services.AddSingleton<IFeatureFlagService, FeatureFlagService>();
        
        // Health checks
        services.AddScoped<DatabaseHealthCheck>();
        services.AddScoped<CacheHealthCheck>();
        
        // Entity validation service
        services.AddScoped<IEntityValidator, EntityValidator>();

        // Domain Services
        services.AddScoped<ITicketDomainService>(sp =>
        {
            var businessRules = sp.GetRequiredService<BusinessRulesConfiguration>();
            var slaConfig = sp.GetRequiredService<SlaConfiguration>();
            return new TicketManagement.Domain.Services.TicketDomainService(businessRules, slaConfig);
        });
        
        // Infrastructure services
        services.AddScoped<TicketManagement.Application.Common.Interfaces.IAuthorizationService, TicketManagement.Infrastructure.Services.AuthorizationService>();
        services.AddScoped<TicketManagement.Application.Common.Interfaces.IRateLimitService, TicketManagement.Infrastructure.Services.RateLimitService>();
        
        // Outbox pattern
        services.AddScoped<Persistence.Outbox.IOutboxService, Persistence.Outbox.OutboxService>();
        
        // Background service for processing outbox events
        services.AddHostedService<BackgroundServices.OutboxProcessorService>();
        
        // ✅ NEW: Background service for cache warmup (removes responsibility from Controllers)
        services.AddHostedService<BackgroundServices.CacheWarmupBackgroundService>();
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        // Cache validation
        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
        services.AddScoped<ICacheWarmupService, CacheWarmupService>();
        
        // Ticket cache service
        services.AddScoped<Application.Common.Interfaces.ITicketCacheService, TicketCacheService>();

        services.AddSingleton<ICachePolicyRegistry>(sp => {
            var registry = new CachePolicyRegistry();
            registry.Register<TicketManagement.Domain.Entities.Ticket>(TimeSpan.FromMinutes(5));
            registry.Register<TicketManagement.Domain.Entities.User>(TimeSpan.FromHours(1));
            registry.Register<TicketManagement.Domain.Entities.Category>(TimeSpan.FromDays(1));
            return registry;
        });
        
        // Configure distributed cache based on environment
        var redisConnectionString = configuration.GetConnectionString("Redis");
        var enableDistributedCache = configuration.GetValue("Cache:EnableDistributedCache", false);
        
        if (!string.IsNullOrEmpty(redisConnectionString) && enableDistributedCache)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "TicketManagement";
            });
        }
        else
        {
            // Use DistributedMemoryCache for IDistributedCache abstraction
            services.AddDistributedMemoryCache();
        }
    }
}
