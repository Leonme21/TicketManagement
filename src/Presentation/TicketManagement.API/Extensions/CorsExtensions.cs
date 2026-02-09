using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace TicketManagement.WebApi.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

        if (allowedOrigins == null || allowedOrigins.Length == 0)
        {
            // Fail fast in production if CORS is not configured
            if (environment.IsProduction())
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

        services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazor", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Requires specific origins, no wildcards
            });
        });

        return services;
    }
}
