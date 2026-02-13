using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketManagement.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TicketManagement.Infrastructure.Persistence;
using TicketManagement.Infrastructure.Identity;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;
using System;
using System.Linq;
using System.Net.Http;

namespace TicketManagement.API.IntegrationTests;

/// <summary>
/// Factory para crear instancia de API en memoria para integration tests
/// Usa InMemory database para no depender de MySQL
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remover el DbContext real
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            

            // Agregar DbContext con InMemory database único por test
            var databaseName = $"TestDatabase_{Guid.NewGuid()}";
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.EnableSensitiveDataLogging();
            });

            // Crear y seedear la base de datos
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            try 
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
                
                dbContext.Database.EnsureCreated();
                SeedTestData(dbContext, passwordHasher);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing test database: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
                }
                throw;
            }
        });
    }


    private static void SeedTestData(ApplicationDbContext context, PasswordHasher passwordHasher)
    {
        // Seed Categories
        if (!context.Categories.Any())
        {
            var bugCategory = Category.Create("Bug", "Software bugs and issues").Value!;
            var featureCategory = Category.Create("Feature", "Feature requests and enhancements").Value!;
            var supportCategory = Category.Create("Support", "General support questions").Value!;
            
            context.Categories.AddRange(bugCategory, featureCategory, supportCategory);
            context.SaveChanges();
        }

        // Seed Users
        if (!context.Users.Any())
        {
            var testPasswordHash = passwordHasher.HashPassword("Test123!");
            
            var customer = User.Create("Test", "Customer", "customer@test.com", testPasswordHash, UserRole.Customer).Value!;
            var agent = User.Create("Test", "Agent", "agent@test.com", testPasswordHash, UserRole.Agent).Value!;
            var admin = User.Create("Test", "Admin", "admin@test.com", testPasswordHash, UserRole.Admin).Value!;

            context.Users.AddRange(customer, agent, admin);
            context.SaveChanges();
        }

        // Seed Sample Tickets
        if (!context.Tickets.Any())
        {
            var customer = context.Users.First(u => u.Role == UserRole.Customer);
            var agent = context.Users.First(u => u.Role == UserRole.Agent);
            var category = context.Categories.First();

            var openTicket = Ticket.Create("Sample Open Ticket", "This is a sample open ticket for testing", TicketPriority.Medium, category.Id, customer.Id).Value!;
            var assignedTicket = Ticket.Create("Sample Assigned Ticket", "This is a sample assigned ticket for testing", TicketPriority.High, category.Id, customer.Id).Value!;
            assignedTicket.Assign(agent.Id);

            var closedTicket = Ticket.Create("Sample Closed Ticket", "This is a sample closed ticket for testing", TicketPriority.Low, category.Id, customer.Id).Value!;
            closedTicket.Assign(agent.Id);
            closedTicket.Close();

            context.Tickets.AddRange(openTicket, assignedTicket, closedTicket);
            context.SaveChanges();
        }
    }

    public HttpClient CreateAuthenticatedClient(UserRole role = UserRole.Customer)
    {
        return CreateClient();
    }

    public ApplicationDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
}
