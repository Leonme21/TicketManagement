using FluentAssertions;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Application.UnitTests;

/// <summary>
/// Tests unitarios para la entidad RefreshToken (Domain Layer)
/// Cobertura: Factory Methods, Validaciones, Logica de negocio de tokens
/// </summary>
public class RefreshTokenDomainTests
{
    private const string ValidToken = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890AB"; // 64 chars

    [Fact]
    public void RefreshToken_Create_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var token = ValidToken;
        var userId = 1;
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var deviceInfo = "Chrome/Windows";

        // Act
        var result = RefreshToken.Create(token, userId, expiresAt, deviceInfo);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var refreshToken = result.Value!;
        refreshToken.Token.Should().Be(token);
        refreshToken.UserId.Should().Be(userId);
        refreshToken.ExpiresAt.Should().Be(expiresAt);
        refreshToken.DeviceInfo.Should().Be(deviceInfo);
        refreshToken.IsActive.Should().BeTrue();
        refreshToken.IsUsed.Should().BeFalse();
        refreshToken.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_CreateWithDefaultExpiry_ShouldSetCorrectExpiration()
    {
        // Arrange
        var token = ValidToken;
        var userId = 1;
        var deviceInfo = "Mobile App";
        var expectedExpiry = DateTime.UtcNow.AddDays(RefreshToken.DefaultExpiryDays);

        // Act
        var result = RefreshToken.CreateWithDefaultExpiry(token, userId, deviceInfo);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var refreshToken = result.Value!;
        refreshToken.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(1));
        refreshToken.DeviceInfo.Should().Be(deviceInfo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RefreshToken_Create_WithInvalidToken_ShouldReturnFailure(string invalidToken)
    {
        // Arrange
        var userId = 1;
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var result = RefreshToken.Create(invalidToken, userId, expiresAt);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Token is required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RefreshToken_Create_WithInvalidUserId_ShouldReturnFailure(int invalidUserId)
    {
        // Arrange
        var token = ValidToken;
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var result = RefreshToken.Create(token, invalidUserId, expiresAt);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Invalid user ID");
    }

    [Fact]
    public void RefreshToken_Create_WithPastExpiryDate_ShouldReturnFailure()
    {
        // Arrange
        var token = ValidToken;
        var userId = 1;
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = RefreshToken.Create(token, userId, pastDate);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Expiry date must be in the future");
    }

    [Fact]
    public void RefreshToken_MarkAsUsed_WhenValid_ShouldMarkSuccessfully()
    {
        // Arrange
        var refreshToken = RefreshToken.CreateWithDefaultExpiry(ValidToken, 1).Value!;
        var replacedByToken = "new-token-" + ValidToken[..20];

        // Act
        var result = refreshToken.MarkAsUsed(replacedByToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        refreshToken.IsUsed.Should().BeTrue();
        refreshToken.UsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        refreshToken.ReplacedByToken.Should().Be(replacedByToken);
        refreshToken.IsValid.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_MarkAsUsed_WhenAlreadyUsed_ShouldReturnFailure()
    {
        // Arrange
        var refreshToken = RefreshToken.CreateWithDefaultExpiry(ValidToken, 1).Value!;
        refreshToken.MarkAsUsed();

        // Act
        var result = refreshToken.MarkAsUsed();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Refresh token is already used");
    }

    [Fact]
    public void RefreshToken_Revoke_WhenActive_ShouldRevokeSuccessfully()
    {
        // Arrange
        var refreshToken = RefreshToken.CreateWithDefaultExpiry(ValidToken, 1).Value!;

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.IsActive.Should().BeFalse();
        refreshToken.IsValid.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_ExtendExpiry_WithValidDays_ShouldExtendSuccessfully()
    {
        // Arrange
        var refreshToken = RefreshToken.CreateWithDefaultExpiry(ValidToken, 1).Value!;
        var originalExpiry = refreshToken.ExpiresAt;
        var additionalDays = 5;

        // Act
        refreshToken.Extend(additionalDays);

        // Assert
        refreshToken.ExpiresAt.Should().Be(originalExpiry.AddDays(additionalDays));
    }

    [Fact]
    public void RefreshToken_ValidateForUse_WhenValid_ShouldBeSuccess()
    {
        // Arrange
        var refreshToken = RefreshToken.CreateWithDefaultExpiry(ValidToken, 1).Value!;

        // Act
        var result = refreshToken.ValidateForUse();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_ValidateForUse_WhenRevoked_ShouldReturnFailure()
    {
        // Arrange
        var refreshToken = RefreshToken.CreateWithDefaultExpiry(ValidToken, 1).Value!;
        refreshToken.Revoke();

        // Act
        var result = refreshToken.ValidateForUse();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Token is inactive");
    }

    [Fact]
    public void RefreshToken_ValidateForUse_WhenUsed_ShouldReturnFailure()
    {
        // Arrange
        var refreshToken = RefreshToken.CreateWithDefaultExpiry(ValidToken, 1).Value!;
        refreshToken.MarkAsUsed();

        // Act
        var result = refreshToken.ValidateForUse();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Token already used");
    }

    [Fact]
    public void RefreshToken_Constants_ShouldHaveCorrectValues()
    {
        // Assert
        RefreshToken.DefaultExpiryDays.Should().Be(7);
        RefreshToken.TokenLength.Should().Be(64);
    }
}
