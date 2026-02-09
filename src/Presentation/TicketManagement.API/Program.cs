using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using TicketManagement.Application;
using TicketManagement.Infrastructure;
using TicketManagement.Infrastructure.Persistence;
using TicketManagement.Application.Common.Configuration;
using TicketManagement.Application.Common.Interfaces;
using FluentValidation;
using TicketManagement.Domain.Services;
using TicketManagement.Application.Common.Authorization;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TicketManagement")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/app-.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting TicketManagement API");

    var builder = WebApplication.CreateBuilder(args);
    
    // ✅ SECURITY: Add User Secrets in Development
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets<Program>();
    }
    
    // ✅ SECURITY: Environment variables override (for production/containers)
    builder.Configuration.AddEnvironmentVariables("TICKETMGMT_");
    
    builder.Host.UseSerilog();

    // Configuration validation and binding
    var appConfig = ValidateConfiguration(builder.Configuration, builder.Environment);
    var businessRules = BindBusinessRules(builder.Configuration);
    var slaConfig = BindSlaConfiguration(builder.Configuration);

    // Add services
    builder.Services.AddSingleton(appConfig);
    builder.Services.AddSingleton(businessRules);
    builder.Services.AddSingleton(slaConfig);
    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);
    
    // ✅ OpenTelemetry for distributed tracing (commented out - requires NuGet packages)
    // Uncomment after installing: OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.AspNetCore
    /*
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddSource("TicketManagement.*");

            if (builder.Environment.IsDevelopment())
            {
                tracing.AddConsoleExporter();
            }
            else
            {
                var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            }
        })
        .WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("TicketManagement.Business");

            if (builder.Environment.IsDevelopment())
            {
                metrics.AddConsoleExporter();
            }
        });
    */

    builder.Services.AddControllers();

    // Global exception handling
    builder.Services.AddExceptionHandler<TicketManagement.API.Middleware.GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Configure caching based on environment
    ConfigureCaching(builder.Services, builder.Configuration);
    
    builder.Services.AddResponseCaching();

    // ✅ Enhanced health checks with detailed monitoring
    builder.Services.AddHealthChecks()
        .AddCheck<TicketManagement.Infrastructure.HealthChecks.DatabaseHealthCheck>(
            "database",
            tags: new[] { "db", "mysql", "critical" })
        .AddCheck<TicketManagement.Infrastructure.HealthChecks.CacheHealthCheck>(
            "cache",
            tags: new[] { "cache", "performance" });

    // CORS configuration
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                ?? new[] { "https://localhost:7001", "http://localhost:5001" };
            
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // Swagger configuration
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "TicketManagement API",
            Version = "v3.0",
            Description = "Ticket Management System API - Clean Architecture with CQRS"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // Middleware pipeline
    app.UseExceptionHandler();
    
    // Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
        
        if (context.Request.IsHttps)
        {
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        }
        
        await next();
    });

    app.UseCors();
    
    // Custom middlewares
    app.UseMiddleware<TicketManagement.API.Middleware.RateLimitingMiddleware>();
    app.UseMiddleware<TicketManagement.API.Middleware.AuthorizationMiddleware>();
    
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => 
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TicketManagement API v3.0");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseHttpsRedirection();
    app.UseResponseCaching();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Health check endpoints
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");
    app.MapHealthChecks("/health/live");

    // Database initialization
    await InitializeDatabaseAsync(app);

    // ✅ REMOVED: Manual cache warmup (now handled by CacheWarmupBackgroundService)

    Log.Information("TicketManagement API started successfully on {Environment}", 
        app.Environment.EnvironmentName);
    
    // ✅ Cache warmup is now handled automatically by CacheWarmupBackgroundService
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 Application terminated unexpectedly");
    throw;
}
finally
{
    Log.Information("🛑 TicketManagement API shutting down");
    await Log.CloseAndFlushAsync();
}

/// <summary>
/// ✅ SECURITY: Enhanced configuration validation with environment-aware secret handling
/// </summary>
static AppConfiguration ValidateConfiguration(IConfiguration configuration, IWebHostEnvironment environment)
{
    // ✅ SECURITY: Get connection string from environment variable first, then config
    var connectionString = Environment.GetEnvironmentVariable("TICKETMGMT_ConnectionStrings__DefaultConnection")
        ?? configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        if (environment.IsDevelopment())
        {
            Log.Warning("⚠️ Database connection string not configured. Use 'dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"your-connection-string\"'");
        }
        throw new InvalidOperationException(
            "Database connection string is required. " +
            "Set via User Secrets (dev), environment variable TICKETMGMT_ConnectionStrings__DefaultConnection, or appsettings.json");
    }

    // ✅ SECURITY: Get JWT secret from environment variable first, then config
    var jwtSecret = Environment.GetEnvironmentVariable("TICKETMGMT_JwtSettings__Secret")
        ?? configuration["JwtSettings:Secret"];
    
    if (string.IsNullOrEmpty(jwtSecret))
    {
        if (environment.IsDevelopment())
        {
            Log.Warning("⚠️ JWT secret not configured. Use 'dotnet user-secrets set \"JwtSettings:Secret\" \"your-32-char-secret-key\"'");
        }
        throw new InvalidOperationException(
            "JWT secret is required. " +
            "Set via User Secrets (dev), environment variable TICKETMGMT_JwtSettings__Secret, or appsettings.json");
    }

    // ✅ SECURITY: Validate JWT secret length
    if (jwtSecret.Length < 32)
    {
        throw new InvalidOperationException("JWT secret must be at least 32 characters long for security");
    }

    return new AppConfiguration
    {
        Database = new DatabaseConfiguration
        {
            ConnectionString = connectionString,
            CommandTimeout = configuration.GetValue("Database:CommandTimeout", 30),
            MaxRetryCount = configuration.GetValue("Database:MaxRetryCount", 3)
        },
        Jwt = new JwtConfiguration
        {
            Secret = jwtSecret,
            Issuer = configuration["JwtSettings:Issuer"] ?? "TicketManagement",
            Audience = configuration["JwtSettings:Audience"] ?? "TicketManagement",
            ExpirationMinutes = configuration.GetValue("JwtSettings:ExpirationMinutes", 60)
        },
        Cache = new CacheConfiguration
        {
            RedisConnectionString = configuration.GetConnectionString("Redis"),
            DefaultExpirationMinutes = configuration.GetValue("Cache:DefaultExpirationMinutes", 30),
            EnableDistributedCache = configuration.GetValue("Cache:EnableDistributedCache", false)
        }
    };
}

// Business rules configuration binding
static BusinessRulesConfiguration BindBusinessRules(IConfiguration configuration)
{
    return new BusinessRulesConfiguration(
        MaxTicketsPerUserPerDay: configuration.GetValue("BusinessRules:MaxTicketsPerUserPerDay", 10),
        MaxCriticalTicketsPerUserPerDay: configuration.GetValue("BusinessRules:MaxCriticalTicketsPerUserPerDay", 2),
        AllowTicketCreationOutsideBusinessHours: configuration.GetValue("BusinessRules:AllowTicketCreationOutsideBusinessHours", true),
        BusinessHoursStart: configuration.GetValue("BusinessRules:BusinessHoursStart", 8),
        BusinessHoursEnd: configuration.GetValue("BusinessRules:BusinessHoursEnd", 18),
        DuplicateCheckHours: configuration.GetValue("BusinessRules:DuplicateCheckHours", 24)
    );
}

// SLA configuration binding
static SlaConfiguration BindSlaConfiguration(IConfiguration configuration)
{
    return new SlaConfiguration(
        CriticalHours: configuration.GetValue("Sla:Critical:Hours", 2.0),
        HighHours: configuration.GetValue("Sla:High:Hours", 8.0),
        MediumHours: configuration.GetValue("Sla:Medium:Hours", 24.0),
        LowHours: configuration.GetValue("Sla:Low:Hours", 72.0)
    );
}

// Configure caching services
static void ConfigureCaching(IServiceCollection services, IConfiguration configuration)
{
    var redisConnectionString = configuration.GetConnectionString("Redis");
    var enableDistributedCache = configuration.GetValue("Cache:EnableDistributedCache", false);

    if (!string.IsNullOrEmpty(redisConnectionString) && enableDistributedCache)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "TicketManagement";
        });
        Log.Information("Redis distributed cache configured");
    }
    else
    {
        services.AddMemoryCache();
        Log.Information("In-memory cache configured");
    }
}

// Database initialization
static async Task InitializeDatabaseAsync(WebApplication app)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();
        
        await initializer.InitializeAsync();
        await initializer.SeedAsync();
        
        Log.Information("Database initialization completed");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "💥 Database initialization failed");
        throw;
    }
}

// ❌ REMOVED: WarmupCacheAsync method (moved to CacheWarmupBackgroundService)

public partial class Program { }
