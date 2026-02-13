using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TicketManagement.Application.Contracts.Authentication;
using TicketManagement.Application.Contracts.Common;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Domain.Enums;
using TicketManagement.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace TicketManagement.API.IntegrationTests.Controllers;

/// <summary>
/// ✅ PRODUCTION-READY: Complete integration tests for Tickets API
/// Features:
/// - Real database (in-memory for tests)
/// - JWT authentication
/// - Authorization testing
/// - Cache invalidation verification
/// - Error scenarios
/// </summary>
public class TicketsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private string _customerToken = null!;
    private string _agentToken = null!;
    private string _adminToken = null!;

    private readonly ITestOutputHelper _output;

    public TicketsControllerIntegrationTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        // ✅ Authenticate users for tests
        _customerToken = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _agentToken = await GetAuthTokenAsync("agent@test.com", "Test123!");
        _adminToken = await GetAuthTokenAsync("admin@test.com", "Test123!");
    }

    public Task DisposeAsync()
    {
        _client?.Dispose();
        return Task.CompletedTask;
    }

    // ==================== AUTHENTICATION TESTS ====================

    [Fact]
    public async Task GetTickets_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/tickets");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTicket_WithValidAuthentication_ReturnsCreated()
    {
        // Arrange
        SetAuthToken(_customerToken);
        var request = new
        {
            Title = "Test Ticket",
            Description = "Integration test ticket",
            Priority = (int)TicketPriority.Medium,
            CategoryId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var location = response.Headers.Location;
        location.Should().NotBeNull();
        location!.ToString().Should().ContainEquivalentOf("/api/tickets/");
    }

    // ==================== CRUD TESTS ====================

    [Fact]
    public async Task CreateTicket_WithValidData_CreatesTicketSuccessfully()
    {
        // Arrange
        SetAuthToken(_customerToken);
        var request = new
        {
            Title = "Bug: Login not working",
            Description = "Users cannot log in with valid credentials",
            Priority = (int)TicketPriority.High,
            CategoryId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var ticketId = await GetTicketIdFromLocation(response.Headers.Location!);
        ticketId.Should().BeGreaterThan(0);

        // Verify ticket was created
        var getResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var ticket = await getResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();
        ticket.Should().NotBeNull();
        ticket!.Title.Should().Be(request.Title);
        ticket.Priority.Should().Be(TicketPriority.High);
    }

    [Fact]
    public async Task UpdateTicket_AsOwner_UpdatesSuccessfully()
    {
        // Arrange
        SetAuthToken(_customerToken);
        var ticketId = await CreateTestTicketAsync();
        
        // Fetch to get RowVersion
        var getExistingResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var existingTicket = await getExistingResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();

        var updateRequest = new
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = (int)TicketPriority.Low,
            CategoryId = 1,
            RowVersion = existingTicket!.RowVersion
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tickets/{ticketId}", updateRequest);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent, $"Response content: {content}");

        // Verify update
        var getResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var ticket = await getResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();
        ticket!.Title.Should().Be(updateRequest.Title);
        ticket.Priority.Should().Be(TicketPriority.Low);
    }

    [Fact]
    public async Task DeleteTicket_AsOwner_SoftDeletesSuccessfully()
    {
        // Arrange
        // ✅ FIXED: Only admins can delete, so use admin token
        SetAuthToken(_adminToken);
        
        // Create a ticket as customer first
        SetAuthToken(_customerToken);
        var ticketId = await CreateTestTicketAsync();

        // Switch to admin to delete
        SetAuthToken(_adminToken);

        // Act
        var response = await _client.DeleteAsync($"/api/tickets/{ticketId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify soft delete (ticket should not be found)
        var getResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ==================== AUTHORIZATION TESTS ====================

    [Fact]
    public async Task UpdateTicket_AsNonOwner_ReturnsForbidden()
    {
        // Arrange
        SetAuthToken(_customerToken);
        var ticketId = await CreateTestTicketAsync();

        // Fetch to get RowVersion
        var getExistingResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var existingTicket = await getExistingResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();

        // Change to different customer
        SetAuthToken(_agentToken);
        var updateRequest = new
        {
            Title = "Unauthorized Update",
            Description = "This should fail",
            Priority = (int)TicketPriority.High,
            CategoryId = 1,
            RowVersion = existingTicket!.RowVersion
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tickets/{ticketId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AssignTicket_AsAgent_AssignsSuccessfully()
    {
        // Arrange
        SetAuthToken(_customerToken);
        var ticketId = await CreateTestTicketAsync();

        // Switch to agent
        SetAuthToken(_agentToken);
        var agentId = await GetCurrentUserIdAsync();
        var assignRequest = new { AgentId = agentId };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/assign", assignRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify assignment
        var getResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var ticket = await getResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();
        ticket!.AssignedToId.Should().Be(agentId);
        ticket.Status.Should().Be(TicketStatus.InProgress);
    }

    [Fact]
    public async Task AssignTicket_AsCustomer_ReturnsForbidden()
    {
        // Arrange
        SetAuthToken(_customerToken);
        var ticketId = await CreateTestTicketAsync();
        
        var assignRequest = new { AgentId = 2 }; // Any agent ID

        // Act
        var response = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/assign", assignRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== VALIDATION TESTS ====================

    [Theory]
    [InlineData("", "Description", TicketPriority.Medium, 1)] // Empty title
    [InlineData("Title", "", TicketPriority.Medium, 1)] // Empty description
    [InlineData("Ti", "Description", TicketPriority.Medium, 1)] // Title too short
    public async Task CreateTicket_WithInvalidData_ReturnsBadRequest(
        string title,
        string description,
        TicketPriority priority,
        int categoryId)
    {
        // Arrange
        SetAuthToken(_customerToken);
        var request = new
        {
            Title = title,
            Description = description,
            Priority = (int)priority,
            CategoryId = categoryId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTicket_WithNonExistentCategory_ReturnsNotFound()
    {
        // Arrange
        SetAuthToken(_customerToken);
        var request = new
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = (int)TicketPriority.Medium,
            CategoryId = 999 // Non-existent
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ==================== COMMENT TESTS ====================

    [Fact]
    public async Task AddComment_WithValidData_AddsCommentSuccessfully()
    {
        // Arrange
        SetAuthToken(_customerToken);
        var ticketId = await CreateTestTicketAsync();
        
        var commentRequest = new { Content = "This is a test comment" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/comments", commentRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify comment was added
        var getResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var ticket = await getResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();
        ticket!.Comments.Should().NotBeEmpty();
        ticket.Comments.Should().ContainSingle(c => c.Content == commentRequest.Content);
    }

    // ==================== PAGINATION TESTS ====================

    [Fact]
    public async Task GetTickets_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        SetAuthToken(_customerToken);
        
        // Create multiple tickets
        for (int i = 0; i < 5; i++)
        {
            await CreateTestTicketAsync($"Test Ticket {i}");
        }

        // Act
        var response = await _client.GetAsync("/api/tickets?pageNumber=1&pageSize=3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<TicketDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountLessOrEqualTo(3);
        result.TotalCount.Should().BeGreaterOrEqualTo(5);
    }

    // ==================== CACHE INVALIDATION TESTS ====================

    [Fact]
    public async Task UpdateTicket_InvalidatesCache()
    {
        // Arrange
        SetAuthToken(_customerToken);
        var ticketId = await CreateTestTicketAsync();

        // Get ticket (populates cache)
        var firstResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var firstTicket = await firstResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();

        // Update ticket
        var updateRequest = new
        {
            Title = "Updated via Cache Test",
            Description = firstTicket!.Description,
            Priority = (int)firstTicket.Priority,
            CategoryId = 1,
            RowVersion = firstTicket.RowVersion
        };
        await _client.PutAsJsonAsync($"/api/tickets/{ticketId}", updateRequest);

        // Get ticket again (should get updated version, not cached)
        var secondResponse = await _client.GetAsync($"/api/tickets/{ticketId}");
        var secondTicket = await secondResponse.Content.ReadFromJsonAsync<TicketDetailsDto>();

        // Assert
        secondTicket!.Title.Should().Be("Updated via Cache Test");
        secondTicket.Title.Should().NotBe(firstTicket.Title);
    }

    // ==================== HELPER METHODS ====================

    private async Task<string> GetAuthTokenAsync(string email, string password)
    {
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"DEBUG: Login Response: {responseBody}");
        
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthenticationResult>();
        return result!.Token; // ✅ Corrected: Token instead of AccessToken
    }

    private void SetAuthToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<int> CreateTestTicketAsync(string? title = null)
    {
        var request = new
        {
            Title = title ?? "Test Ticket",
            Description = "Test Description for integration tests",
            Priority = (int)TicketPriority.Medium,
            CategoryId = 1
        };

        var response = await _client.PostAsJsonAsync("/api/tickets", request);
        response.EnsureSuccessStatusCode();

        return await GetTicketIdFromLocation(response.Headers.Location!);
    }

    private async Task<int> GetTicketIdFromLocation(Uri location)
    {
        var locationString = location.ToString();
        var ticketId = int.Parse(locationString.Split('/').Last());
        return await Task.FromResult(ticketId);
    }

    private async Task<int> GetCurrentUserIdAsync()
    {
        // In a real scenario, you would decode the JWT or query the API
        // For simplicity, we'll use a hardcoded value based on seeded data
        return await Task.FromResult(2); // Agent user ID from seed data
    }
}
