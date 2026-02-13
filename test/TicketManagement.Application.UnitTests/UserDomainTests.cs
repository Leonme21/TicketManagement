using FluentAssertions;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Application.UnitTests;

/// <summary>
/// Tests unitarios para la entidad User (Domain Layer)
/// Cobertura: Factory Methods, Validaciones, Logica de negocio
/// </summary>
public class UserDomainTests
{
    private const string ValidPasswordHash = "$2a$11$abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQR"; // 60 chars BCrypt

    [Fact]
    public void User_Create_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var role = UserRole.Customer;

        // Act
        var result = User.Create(firstName, lastName, email, ValidPasswordHash, role);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var user = result.Value!;
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Value.Should().Be(email);  // ? FIXED: Compare Email.Value not Email object
        user.PasswordHash.Should().Be(ValidPasswordHash);
        user.Role.Should().Be(role);
        user.IsActive.Should().BeTrue();
        user.FullName.Should().Be("John Doe");
    }

    [Theory]
    [InlineData("", "Doe")]
    [InlineData("   ", "Doe")]
    [InlineData(null, "Doe")]
    public void User_Create_WithInvalidFirstName_ShouldReturnFailure(string invalidFirstName, string lastName)
    {
        // Act
        var result = User.Create(invalidFirstName, lastName, "test@example.com", ValidPasswordHash, UserRole.Customer);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("First name cannot be empty");
    }

    [Theory]
    [InlineData("John", "")]
    [InlineData("John", "   ")]
    [InlineData("John", null)]
    public void User_Create_WithInvalidLastName_ShouldReturnFailure(string firstName, string invalidLastName)
    {
        // Act
        var result = User.Create(firstName, invalidLastName, "test@example.com", ValidPasswordHash, UserRole.Customer);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Last name cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("invalid-email")]
    [InlineData("missing@domain")]
    [InlineData("missing.com")]
    public void User_Create_WithInvalidEmail_ShouldReturnFailure(string invalidEmail)
    {
        // Act
        var result = User.Create("John", "Doe", invalidEmail, ValidPasswordHash, UserRole.Customer);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().ContainEquivalentOf("email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("short")] // Less than 60 characters
    public void User_Create_WithInvalidPasswordHash_ShouldReturnFailure(string invalidPasswordHash)
    {
        // Act
        var result = User.Create("John", "Doe", "test@example.com", invalidPasswordHash, UserRole.Customer);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().ContainEquivalentOf("password");
    }

    [Fact]
    public void User_Create_WithNameTooLong_ShouldReturnFailure()
    {
        // Arrange
        var longName = new string('A', User.MaxNameLength + 1);

        // Act
        var result = User.Create(longName, "Doe", "test@example.com", ValidPasswordHash, UserRole.Customer);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be($"First name cannot exceed {User.MaxNameLength} characters");
    }

    [Fact]
    public void User_UpdateProfile_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", ValidPasswordHash, UserRole.Customer).Value!;
        var newFirstName = "Jane";
        var newLastName = "Smith";

        // Act
        var result = user.UpdateProfile(newFirstName, newLastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        user.FullName.Should().Be("Jane Smith");
    }

    [Fact]
    public void User_UpdatePassword_WithValidHash_ShouldUpdateSuccessfully()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", ValidPasswordHash, UserRole.Customer).Value!;
        var newPasswordHash = "$2a$11$abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQR"; // 60 chars

        // Act
        var result = user.UpdatePassword(newPasswordHash);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be(newPasswordHash);
    }

    [Fact]
    public void User_UpdatePassword_WithInvalidHash_ShouldReturnFailure()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", ValidPasswordHash, UserRole.Customer).Value!;
        var invalidHash = "short";

        // Act
        var result = user.UpdatePassword(invalidHash);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().ContainEquivalentOf("password");
    }

    [Fact]
    public void User_ChangeRole_WithDifferentRole_ShouldUpdateSuccessfully()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", ValidPasswordHash, UserRole.Customer).Value!;
        var newRole = UserRole.Agent;

        // Act
        var result = user.ChangeRole(newRole);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Role.Should().Be(newRole);
    }

    [Fact]
    public void User_ChangeRole_WithSameRole_ShouldReturnFailure()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", ValidPasswordHash, UserRole.Customer).Value!;

        // Act
        var result = user.ChangeRole(UserRole.Customer);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("User already has role Customer");
    }

    [Fact]
    public void User_Deactivate_WhenActive_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", ValidPasswordHash, UserRole.Customer).Value!;

        // Act
        var result = user.Deactivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void User_Deactivate_WhenAlreadyInactive_ShouldReturnFailure()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", ValidPasswordHash, UserRole.Customer).Value!;
        user.Deactivate();

        // Act
        var result = user.Deactivate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("User is already deactivated");
    }

    [Fact]
    public void User_Activate_WhenInactive_ShouldActivateSuccessfully()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", ValidPasswordHash, UserRole.Customer).Value!;
        user.Deactivate();

        // Act
        var result = user.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void User_Activate_WhenAlreadyActive_ShouldReturnFailure()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", ValidPasswordHash, UserRole.Customer).Value!;

        // Act
        var result = user.Activate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("User is already active");
    }

    [Fact]
    public void User_Constants_ShouldHaveCorrectValues()
    {
        // Assert
        User.MaxNameLength.Should().Be(100);
        User.MinPasswordHashLength.Should().Be(60);
    }
}
