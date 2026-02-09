using FluentAssertions;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Application.UnitTests;

/// <summary>
/// Tests unitarios para la entidad Category (Domain Layer)
/// Cobertura: Factory Methods, Validaciones, Logica de negocio
/// </summary>
public class CategoryDomainTests
{
    [Fact]
    public void Category_Create_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var name = "Bug Reports";
        var description = "Software bugs and issues";

        // Act
        var result = Category.Create(name, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var category = result.Value!;
        category.Name.Should().Be(name);
        category.Description.Should().Be(description);
        category.IsActive.Should().BeTrue();
        category.IsDeleted.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "Description")]
    [InlineData("   ", "Description")]
    [InlineData(null, "Description")]
    public void Category_Create_WithInvalidName_ShouldReturnFailure(string invalidName, string description)
    {
        // Act
        var result = Category.Create(invalidName, description);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Category name cannot be empty");
    }

    [Theory]
    [InlineData("Name", "")]
    [InlineData("Name", "   ")]
    [InlineData("Name", null)]
    public void Category_Create_WithInvalidDescription_ShouldReturnFailure(string name, string invalidDescription)
    {
        // Act
        var result = Category.Create(name, invalidDescription);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Category description cannot be empty");
    }

    [Fact]
    public void Category_Create_WithNameTooLong_ShouldReturnFailure()
    {
        // Arrange
        var longName = new string('A', Category.MaxNameLength + 1);
        var description = "Valid description";

        // Act
        var result = Category.Create(longName, description);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be($"Category name cannot exceed {Category.MaxNameLength} characters");
    }

    [Fact]
    public void Category_UpdateDetails_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var category = Category.Create("Old Name", "Old Description").Value!;
        var newName = "New Name";
        var newDescription = "New Description";

        // Act
        var result = category.UpdateDetails(newName, newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be(newName);
        category.Description.Should().Be(newDescription);
    }

    [Fact]
    public void Category_Deactivate_WhenActive_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var category = Category.Create("Test Category", "Test Description").Value!;

        // Act
        var result = category.Deactivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Category_Deactivate_WhenAlreadyInactive_ShouldReturnFailure()
    {
        // Arrange
        var category = Category.Create("Test Category", "Test Description").Value!;
        category.Deactivate();

        // Act
        var result = category.Deactivate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Category is already inactive");
    }

    [Fact]
    public void Category_Activate_WhenInactive_ShouldActivateSuccessfully()
    {
        // Arrange
        var category = Category.Create("Test Category", "Test Description").Value!;
        category.Deactivate();

        // Act
        var result = category.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Category_Activate_WhenAlreadyActive_ShouldReturnFailure()
    {
        // Arrange
        var category = Category.Create("Test Category", "Test Description").Value!;

        // Act
        var result = category.Activate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Category is already active");
    }

    [Fact]
    public void Category_Constants_ShouldHaveCorrectValues()
    {
        // Assert
        Category.MaxNameLength.Should().Be(100);
        Category.MaxDescriptionLength.Should().Be(500);
    }
}
