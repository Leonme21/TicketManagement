using System.Diagnostics;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TicketManagement.API.IntegrationTests;

/// <summary>
/// Performance tests to ensure queries execute within acceptable time limits
/// </summary>
public class PerformanceTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PerformanceTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAllTickets_ShouldExecuteWithin500ms()
    {
        // Arrange
        using var dbContext = _factory.GetDbContext();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var tickets = await dbContext.Tickets
            .Include(t => t.Category)
            .Include(t => t.Creator)
            .Take(100)
            .ToListAsync();

        stopwatch.Stop();

        // Assert
        tickets.Should().NotBeEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, 
            "Query should execute within 500ms for good user experience");
    }

    [Fact]
    public async Task GetTicketsByCategory_ShouldExecuteWithin200ms()
    {
        // Arrange
        using var dbContext = _factory.GetDbContext();
        var category = await dbContext.Categories.FirstAsync();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var tickets = await dbContext.Tickets
            .Where(t => t.CategoryId == category.Id)
            .Take(50)
            .ToListAsync();

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200,
            "Filtered queries should be fast with proper indexing");
    }

    [Fact]
    public async Task CountTicketsByStatus_ShouldExecuteWithin100ms()
    {
        // Arrange
        using var dbContext = _factory.GetDbContext();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var counts = await dbContext.Tickets
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        stopwatch.Stop();

        // Assert
        counts.Should().NotBeEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100,
            "Aggregation queries should be optimized");
    }

    [Fact]
    public async Task CreateMultipleTickets_ShouldHandleBatchOperations()
    {
        // Arrange
        using var dbContext = _factory.GetDbContext();
        var category = await dbContext.Categories.FirstAsync();
        var user = await dbContext.Users.FirstAsync();

        var tickets = new List<Domain.Entities.Ticket>();
        for (int i = 0; i < 10; i++)
        {
            var ticketResult = Domain.Entities.Ticket.Create(
                $"Performance Test Ticket {i}",
                $"Description for ticket {i}",
                Domain.Enums.TicketPriority.Medium,
                category.Id,
                user.Id);
            
            tickets.Add(ticketResult.Value!);
        }

        var stopwatch = Stopwatch.StartNew();

        // Act
        dbContext.Tickets.AddRange(tickets);
        await dbContext.SaveChangesAsync();

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000,
            "Batch operations should be efficient");

        // Cleanup
        dbContext.Tickets.RemoveRange(tickets);
        await dbContext.SaveChangesAsync();
    }
}