using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TicketManagement.Application.Contracts.Authentication;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Domain.Enums;

namespace TicketManagement.API.IntegrationTests.Controllers;

/// <summary>
/// ? Integration tests para TicketsController
/// Verifica el flujo completo: API ? Application ? Infrastructure ? Database
/// </summary>
public class TicketsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TicketsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTicket_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Title = "Test Ticket",
            Description = "Test Description",
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
    }

    [Fact]
    public async Task CreateTicket_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var request = new
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = (int)TicketPriority.Medium,
            CategoryId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTicket_WithInvalidCategory_ReturnsBadRequest()
    {
        // Arrange
        var token = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = (int)TicketPriority.Medium,
            CategoryId = 999 // No existe
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("", "Description", TicketPriority.Medium, 1)] // Título vacío
    [InlineData("Title", "", TicketPriority.Medium, 1)] // Descripción vacía
    public async Task CreateTicket_WithInvalidData_ReturnsBadRequest(
        string title, string description, TicketPriority priority, int categoryId)
    {
        // Arrange
        var token = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
    public async Task GetTicketById_ExistingTicket_ReturnsOk()
    {
        // Arrange
        var token = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Crear ticket primero
        var createRequest = new
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = (int)TicketPriority.Medium,
            CategoryId = 1
        };
        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var createTicketResponse = await createResponse.Content.ReadFromJsonAsync<CreateTicketResponse>();
        var ticketId = createTicketResponse!.TicketId;

        // Act
        var response = await _client.GetAsync($"/api/tickets/{ticketId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var ticket = await response.Content.ReadFromJsonAsync<TicketDetailsDto>();
        ticket.Should().NotBeNull();
        ticket!.Title.Should().Be("Test Ticket");
    }

    [Fact]
    public async Task GetTicketById_NonExistingTicket_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/tickets/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignTicket_AsAgent_ReturnsOk()
    {
        // Arrange - Crear ticket como customer
        var customerToken = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", customerToken);

        var createRequest = new
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = (int)TicketPriority.High,
            CategoryId = 1
        };
        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var createTicketResponse = await createResponse.Content.ReadFromJsonAsync<CreateTicketResponse>();
        var ticketId = createTicketResponse!.TicketId;

        // Cambiar a agent token
        var agentToken = await GetAuthTokenAsync("agent@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", agentToken);

        var assignRequest = new { AgentId = 2 }; // ID del agente

        // Act
        var response = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/assign", assignRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
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
