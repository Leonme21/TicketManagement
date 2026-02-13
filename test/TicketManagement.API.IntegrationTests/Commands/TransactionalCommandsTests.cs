using TicketManagement.Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TicketManagement.Application.Contracts.Authentication;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Infrastructure.Persistence;
using Xunit;

namespace TicketManagement.API.IntegrationTests.Commands;

/// <summary>
/// 🔥 ENTERPRISE LEVEL: Transactional command integration tests
/// Verifies that commands execute within transactions and rollback on failure
/// </summary>
public class TransactionalCommandsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TransactionalCommandsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTicket_OnSuccess_CommitsTransaction()
    {
        // Arrange
        var token = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Title = "Transaction Test Ticket",
            Description = "Testing transaction commit",
            Priority = (int)TicketPriority.Medium,
            CategoryId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createTicketResponse = await response.Content.ReadFromJsonAsync<CreateTicketResponse>();
        var ticketId = createTicketResponse!.TicketId;
        ticketId.Should().BeGreaterThan(0);

        // Verify the ticket was actually persisted
        var getResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var ticket = await getResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();
        ticket.Should().NotBeNull();
        ticket!.Title.Should().Be("Transaction Test Ticket");
    }

    [Fact]
    public async Task CreateTicket_WithInvalidData_RollsBackTransaction()
    {
        // Arrange
        var token = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Title = "", // Invalid - empty title should fail validation
            Description = "Testing transaction rollback",
            Priority = (int)TicketPriority.Medium,
            CategoryId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        // Verify no ticket was persisted (transaction rolled back)
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var ticketsWithEmptyTitle = dbContext.Tickets.Count(t => t.Title.Value == "");
        ticketsWithEmptyTitle.Should().Be(0);
    }

    [Fact]
    public async Task UpdateTicket_OnSuccess_CommitsTransaction()
    {
        // Arrange
        var token = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create a ticket first
        var createRequest = new
        {
            Title = "Original Title",
            Description = "Original Description",
            Priority = (int)TicketPriority.Low,
            CategoryId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var createTicketResponse = await createResponse.Content.ReadFromJsonAsync<CreateTicketResponse>();
        var ticketId = createTicketResponse!.TicketId;

        // Get the ticket to get its RowVersion
        var getResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var ticket = await getResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();

        // Update the ticket
        var updateRequest = new
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = (int)TicketPriority.High,
            CategoryId = 1,
            RowVersion = ticket!.RowVersion
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/tickets/{ticketId}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update was persisted
        var verifyResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var updatedTicket = await verifyResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();
        updatedTicket!.Title.Should().Be("Updated Title");
        updatedTicket.Priority.Should().Be(TicketPriority.High);
    }

    [Fact]
    public async Task DeleteTicket_OnSuccess_CommitsTransaction()
    {
        // Arrange - Login as admin (only admins can delete)
        var token = await GetAuthTokenAsync("admin@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create a ticket first as customer
        var customerToken = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", customerToken);

        var createRequest = new
        {
            Title = "Ticket to Delete",
            Description = "Will be deleted",
            Priority = (int)TicketPriority.Low,
            CategoryId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var createTicketResponse = await createResponse.Content.ReadFromJsonAsync<CreateTicketResponse>();
        var ticketId = createTicketResponse!.TicketId;

        // Switch back to admin
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/tickets/{ticketId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the ticket was deleted (soft delete or hard delete)
        var getResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ==================== HELPERS ====================

    private async Task<string> GetAuthTokenAsync(string email, string password)
    {
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Failed to authenticate test user: {email}. " +
                $"Status: {response.StatusCode}");
        }

        var authResult = await response.Content.ReadFromJsonAsync<AuthenticationResult>();
        return authResult?.Token ?? throw new InvalidOperationException("Token is null");
    }
}
