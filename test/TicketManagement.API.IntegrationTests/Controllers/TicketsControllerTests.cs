using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TicketManagement.Application.Contracts.Authentication;
using TicketManagement.Application.Contracts.Tickets;

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

        var request = new CreateTicketRequest
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = "Medium",
            CategoryId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var ticketId = await response.Content.ReadFromJsonAsync<int>();
        ticketId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateTicket_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreateTicketRequest
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = "Medium",
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

        var request = new CreateTicketRequest
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = "Medium",
            CategoryId = 999 // No existe
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("", "Description", "Medium", 1)] // Título vacío
    [InlineData("Title", "", "Medium", 1)] // Descripción vacía
    public async Task CreateTicket_WithInvalidData_ReturnsBadRequest(
        string title, string description, string priority, int categoryId)
    {
        // Arrange
        var token = await GetAuthTokenAsync("customer@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new CreateTicketRequest
        {
            Title = title,
            Description = description,
            Priority = priority,
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
        var createRequest = new CreateTicketRequest
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = "Medium",
            CategoryId = 1
        };
        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var ticketId = await createResponse.Content.ReadFromJsonAsync<int>();

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

        var createRequest = new CreateTicketRequest
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = "High",
            CategoryId = 1
        };
        var createResponse = await _client.PostAsJsonAsync("/api/tickets", createRequest);
        var ticketId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Cambiar a agent token
        var agentToken = await GetAuthTokenAsync("agent@test.com", "Test123!");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", agentToken);

        var assignRequest = new AssignTicketRequest { AgentId = 2 }; // ID del agente

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tickets/{ticketId}/assign", assignRequest);

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
