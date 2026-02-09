using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TicketManagement.Infrastructure.Persistence;

public static class ApplicationDbContextInitializerExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContextInitializer>>();
        var environment = services.GetRequiredService<IWebHostEnvironment>();
        var initializer = services.GetRequiredService<ApplicationDbContextInitializer>();

        try
        {
            if (environment.IsDevelopment())
            {
                logger.LogInformation("🔧 Development environment: Running migrations and seed data");
                await initializer.InitializeAsync();
                await initializer.SeedAsync();
                logger.LogInformation("✅ Database initialized successfully");
            }
            else if (environment.IsStaging())
            {
                logger.LogInformation("🔧 Staging environment: Running migrations and seed data");
                await initializer.InitializeAsync();
                await initializer.SeedAsync();
                logger.LogInformation("✅ Database initialized successfully");
            }
            else if (environment.IsProduction())
            {
                logger.LogInformation("🔧 Production environment: Running migrations only (no seed data)");
                await initializer.InitializeAsync();
                logger.LogInformation("✅ Database migrations applied successfully");
                logger.LogWarning("⚠️ Seed data NOT applied in Production");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ An error occurred while initializing the database");

            if (environment.IsDevelopment() || environment.IsProduction())
            {
                throw new Exception("Database initialization failed. Application cannot start.", ex);
            }
            
            // Allow continuance in other envs if policy dictates, but simplified here to robust throw.
        }
    }
}
