using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TicketManagement.Application.Contracts.Authentication;

namespace TicketManagement.API.IntegrationTests.Controllers;

/// <summary>
/// ? Integration tests para AuthController
/// Verifica autenticación, registro y manejo de errores
/// </summary>
public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "customer@test.com",
            Password = "Test123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var authResult = await response.Content.ReadFromJsonAsync<AuthenticationResult>();
        authResult.Should().NotBeNull();
        authResult!.Token.Should().NotBeNullOrEmpty();
        authResult.Email.Should().Be("customer@test.com");
        authResult.Role.Should().Be("Customer");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "customer@test.com",
            Password = "WrongPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@test.com",
            Password = "Test123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "New",
            LastName = "User",
            Email = "newuser@test.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var authResult = await response.Content.ReadFromJsonAsync<AuthenticationResult>();
        authResult.Should().NotBeNull();
        authResult!.Token.Should().NotBeNullOrEmpty();
        authResult.Email.Should().Be("newuser@test.com");
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "customer@test.com", // Ya existe en seed
            Password = "Test123!",
            ConfirmPassword = "Test123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("", "User", "test@test.com", "Test123!", "Test123!")] // FirstName vacío
    [InlineData("Test", "", "test@test.com", "Test123!", "Test123!")] // LastName vacío
    [InlineData("Test", "User", "invalid-email", "Test123!", "Test123!")] // Email inválido
    [InlineData("Test", "User", "test@test.com", "short", "short")] // Password corto
    public async Task Register_WithInvalidData_ReturnsBadRequest(
        string firstName, string lastName, string email, string password, string confirmPassword)
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Password = password,
            ConfirmPassword = confirmPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CorrelationId_IsReturnedInResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "customer@test.com",
            Password = "Test123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.Headers.Should().ContainKey("X-Correlation-ID");
        var correlationId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }
}
