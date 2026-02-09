using TicketManagement.Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TicketManagement.Application.Contracts.Authentication;
using TicketManagement.Application.Contracts.Tickets;
using Xunit;

namespace TicketManagement.API.IntegrationTests.Authorization;

/// <summary>
/// 🔥 ENTERPRISE LEVEL: Resource-level authorization integration tests
/// Verifies fine-grained ticket-level permission checks
/// </summary>
public class ResourceAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ResourceAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateTicket_AsCreator_Succeeds()
    {
        // Arrange
        var token = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create a ticket as customer
        var createRequest = new CreateTicketRequest
        {
            Title = "Creator's Ticket",
            Description = "Created by customer",
            Priority = "Medium",
            CategoryId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var ticketId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Get the ticket to get RowVersion
        var getResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var ticket = await getResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();

        // Update as the same user (creator)
        var updateRequest = new UpdateTicketApiRequest
        {
            Title = "Updated by Creator",
            Description = "Creator can update their own ticket",
            Priority = TicketPriority.High,
            CategoryId = 1,
            RowVersion = ticket!.RowVersion
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/tickets/{ticketId}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateTicket_AsNonCreatorNonAssignee_ReturnsForbidden()
    {
        // Arrange
        var customerToken = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", customerToken);

        // Create a ticket as customer
        var createRequest = new CreateTicketRequest
        {
            Title = "Customer's Ticket",
            Description = "Created by customer",
            Priority = "Medium",
            CategoryId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var ticketId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Get the ticket as the creator
        var getResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var ticket = await getResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();

        // Try to update as a different agent (not creator, not assigned)
        var agentToken = await GetAuthTokenAsync("agent@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", agentToken);

        var updateRequest = new UpdateTicketApiRequest
        {
            Title = "Attempted Update",
            Description = "Should fail - not authorized",
            Priority = TicketPriority.High,
            CategoryId = 1,
            RowVersion = ticket!.RowVersion
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/tickets/{ticketId}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateTicket_AsAdmin_Succeeds()
    {
        // Arrange
        var customerToken = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", customerToken);

        // Create a ticket as customer
        var createRequest = new CreateTicketRequest
        {
            Title = "Customer Ticket",
            Description = "Will be updated by admin",
            Priority = "Low",
            CategoryId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var ticketId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Get the ticket
        var getResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var ticket = await getResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();

        // Update as admin
        var adminToken = await GetAuthTokenAsync("admin@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var updateRequest = new UpdateTicketApiRequest
        {
            Title = "Updated by Admin",
            Description = "Admin can update any ticket",
            Priority = TicketPriority.High,
            CategoryId = 1,
            RowVersion = ticket!.RowVersion
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/tickets/{ticketId}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTicket_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var customerToken = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", customerToken);

        // Create a ticket as customer
        var createRequest = new CreateTicketRequest
        {
            Title = "Ticket to Delete",
            Description = "Customer will try to delete",
            Priority = "Low",
            CategoryId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var ticketId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Try to delete as customer (should fail - only admins can delete)
        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/tickets/{ticketId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteTicket_AsAdmin_Succeeds()
    {
        // Arrange
        var customerToken = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", customerToken);

        // Create a ticket as customer
        var createRequest = new CreateTicketRequest
        {
            Title = "Admin Will Delete",
            Description = "Admin has permission",
            Priority = "Low",
            CategoryId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var ticketId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Delete as admin
        var adminToken = await GetAuthTokenAsync("admin@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/tickets/{ticketId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
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
