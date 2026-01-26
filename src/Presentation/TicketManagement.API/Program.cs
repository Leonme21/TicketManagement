using TicketManagement.Application;
using TicketManagement.Infrastructure;
using TicketManagement.Infrastructure.Persistence;
using TicketManagement.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ==================== ADD SERVICES ====================

// Application Layer (MediatR, AutoMapper, FluentValidation)
builder.Services.AddApplication();

// Infrastructure Layer (DbContext, Repositories, JWT)
builder.Services.AddInfrastructure(builder.Configuration);

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Ticket Management API", Version = "v1" });

    // JWT Authentication en Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.  Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType. SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ==================== CORS CONFIGURATION ====================
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    // Fail fast in production if CORS is not configured
    if (builder.Environment.IsProduction())
    {
        throw new InvalidOperationException("CORS 'AllowedOrigins' is not configured! This is a security risk in Production.");
    }
    
    // Default for development
    Console.WriteLine("⚠️ CORS not configured. Defaulting to localhost ports for Development.");
    allowedOrigins = new[] { "https://localhost:7003", "http://localhost:5003" }; 
}
else
{
    Console.WriteLine("✅ CORS Allowed Origins:");
    foreach (var origin in allowedOrigins)
    {
        Console.WriteLine($"   ✓ {origin}");
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Requires specific origins, no wildcards
    });
});

var app = builder.Build();

// ==================== CONFIGURE PIPELINE ====================

// Exception Handling Middleware (PRIMERO)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger (solo en Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ==================== DATABASE INITIALIZATION ====================
// Estrategia de migraciones por ambiente
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var environment = services.GetRequiredService<IWebHostEnvironment>();

    try
    {
        var initializer = services.GetRequiredService<ApplicationDbContextInitializer>();

        if (environment.IsDevelopment())
        {
            // DESARROLLO: Aplicar migraciones y seed automáticamente
            logger.LogInformation("🔧 Development environment: Running migrations and seed data");
            await initializer.InitialiseAsync();
            await initializer.SeedAsync();
            logger.LogInformation("✅ Database initialized successfully");
        }
        else if (environment.IsStaging())
        {
            // STAGING: Aplicar migraciones y opcionalmente seed
            logger.LogInformation("🔧 Staging environment: Running migrations and seed data");
            await initializer.InitialiseAsync();
            await initializer.SeedAsync();
            logger.LogInformation("✅ Database initialized successfully");
        }
        else if (environment.IsProduction())
        {
            // PRODUCCIÓN:  Solo migraciones, SIN seed (datos de prueba)
            logger.LogInformation("🔧 Production environment: Running migrations only (no seed data)");
            await initializer.InitialiseAsync();
            logger.LogInformation("✅ Database migrations applied successfully");
            logger.LogWarning("⚠️ Seed data NOT applied in Production");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ An error occurred while initializing the database");

        // En desarrollo, lanzar la excepción para detener la app
        if (environment.IsDevelopment())
        {
            throw;
        }

        // En producción, TAMBIÉN lanzar la excepción.
        // Es crítico que la DB esté al día. Si falla la migración, la app no debe arrancar.
        if (environment.IsProduction())
        {
             throw new Exception("Database initialization failed in Production. Application cannot start.", ex);
        }

        // En otros entornos (Staging?), loggear y continuar (o decidir política)
        logger.LogWarning("⚠️ Application will continue without database initialization (Non-Production/Non-Development environment)");
    }
}

// ==================== MIDDLEWARE PIPELINE ====================

app.UseHttpsRedirection();

// CORS
app.UseCors("AllowBlazor");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();