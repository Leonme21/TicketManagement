using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;
using TicketManagement.Infrastructure.Identity;

namespace TicketManagement.Infrastructure.Persistence;

/// <summary>
/// Inicializador del DbContext
/// - Ejecuta migraciones
/// - Inserta datos de prueba (seeding)
/// </summary>
public class ApplicationDbContextInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly PasswordHasher _passwordHasher;

    public ApplicationDbContextInitializer(
        ApplicationDbContext context,
        ILogger<ApplicationDbContextInitializer> logger,
        PasswordHasher passwordHasher)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Ejecuta migraciones pendientes
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    /// <summary>
    /// Inserta datos de prueba si la BD est vaca
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // Verificar si ya hay datos
        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Database already seeded.");
            return;
        }

        _logger.LogInformation("Seeding database...");

        // ==================== USERS ====================
        var adminPassword = _passwordHasher.HashPassword("Admin@123");
        var agentPassword = _passwordHasher.HashPassword("Agent@123");
        var customerPassword = _passwordHasher.HashPassword("Customer@123");

        var users = new List<User>
        {
            User.Create("Admin", "User", "admin@ticketmanagement.com", adminPassword, UserRole.Admin).Value!,
            User.Create("John", "Agent", "agent@ticketmanagement.com", agentPassword, UserRole.Agent).Value!,
            User.Create("Jane", "Customer", "customer@ticketmanagement.com", customerPassword, UserRole.Customer).Value!
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} users", users.Count);

        // ==================== CATEGORIES ====================
        var categories = new List<Category>
        {
            Category.Create("Bug", "Software bugs and errors").Value!,
            Category.Create("Feature Request", "New feature suggestions").Value!,
            Category.Create("Support", "Technical support requests").Value!,
            Category.Create("Question", "General questions").Value!,
            Category.Create("Documentation", "Documentation issues").Value!
        };

        _context.Categories.AddRange(categories);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} categories", categories.Count);

        // ==================== TICKETS ====================
        var tickets = new List<Ticket>
        {
            Ticket.Create(
                "Login page not loading",
                "When I try to access the login page, I get a 500 error.",
                TicketPriority.High,
                categories[0].Id,
                users[2].Id).Value!,

            Ticket.Create(
                "Add dark mode",
                "It would be great to have a dark mode option in the settings.",
                TicketPriority.Medium,
                categories[1].Id,
                users[2].Id).Value!,

            Ticket.Create(
                "How to reset password?",
                "I forgot my password and can't find the reset option.",
                TicketPriority.Low,
                categories[3].Id,
                users[2].Id).Value!
        };

        // Asignar el primer ticket al agente
        tickets[0].Assign(users[1].Id);

        _context.Tickets.AddRange(tickets);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} tickets", tickets.Count);

        // ==================== COMMENTS ====================
        var comments = new List<Comment>();
        
        var comment1Result = Comment.Create("I'm looking into this issue.", tickets[0].Id, users[1].Id);
        if (comment1Result.IsSuccess) comments.Add(comment1Result.Value!);
        
        var comment2Result = Comment.Create("Thanks for the quick response!", tickets[0].Id, users[2].Id);
        if (comment2Result.IsSuccess) comments.Add(comment2Result.Value!);

        _context.Comments.AddRange(comments);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} comments", comments.Count);

        _logger.LogInformation("Database seeding completed successfully.");
    }
}
