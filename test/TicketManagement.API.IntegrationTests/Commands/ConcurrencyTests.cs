using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TicketManagement.Application.Contracts.Authentication;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Domain.Enums;
using Xunit;

namespace TicketManagement.API.IntegrationTests.Commands;

/// <summary>
/// 🔥 ENTERPRISE LEVEL: Concurrency conflict integration tests
/// Verifies optimistic locking and ConcurrencyException handling
/// </summary>
public class ConcurrencyTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ConcurrencyTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateTicket_WithStaleRowVersion_ReturnsConflict()
    {
        // Arrange
        var token = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create a ticket
        var createRequest = new CreateTicketRequest
        {
            Title = "Concurrency Test Ticket",
            Description = "Testing concurrency handling",
            Priority = "High",
            CategoryId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var ticketId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Get the ticket to get its RowVersion
        var getResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var ticket = await getResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();
        var originalRowVersion = ticket!.RowVersion;

        // First update (this should succeed)
        var updateRequest1 = new UpdateTicketApiRequest
        {
            Title = "Updated Title 1",
            Description = "Updated Description 1",
            Priority = TicketPriority.High,
            CategoryId = 1,
            RowVersion = originalRowVersion
        };

        var updateResponse1 = await _client.PutAsJsonAsync($"/api/tickets/{ticketId}", updateRequest1);
        updateResponse1.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Second update with stale RowVersion (this should fail with Conflict)
        var updateRequest2 = new UpdateTicketApiRequest
        {
            Title = "Updated Title 2",
            Description = "Updated Description 2",
            Priority = TicketPriority.Medium,
            CategoryId = 1,
            RowVersion = originalRowVersion // Stale version
        };

        // Act
        var updateResponse2 = await _client.PutAsJsonAsync($"/api/tickets/{ticketId}", updateRequest2);

        // Assert
        updateResponse2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        var problemDetails = await updateResponse2.Content.ReadAsStringAsync();
        problemDetails.Should().Contain("modified by another user");
    }

    [Fact]
    public async Task UpdateTicket_WithCurrentRowVersion_Succeeds()
    {
        // Arrange
        var token = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create a ticket
        var createRequest = new CreateTicketRequest
        {
            Title = "Fresh Update Test",
            Description = "Testing fresh update",
            Priority = "Medium",
            CategoryId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var ticketId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Get the ticket to get its current RowVersion
        var getResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var ticket = await getResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();

        // Update with current RowVersion
        var updateRequest = new UpdateTicketApiRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = TicketPriority.High,
            CategoryId = 1,
            RowVersion = ticket!.RowVersion
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/tickets/{ticketId}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
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
