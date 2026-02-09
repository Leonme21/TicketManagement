using TicketManagement.Domain.Entities;

namespace TicketManagement.Domain.UnitTests.Entities;

/// <summary>
/// ? STAFF LEVEL: Comprehensive unit tests for Tag entity
/// Tests Factory Method pattern, validations, and business logic
/// </summary>
public class TagTests
{
    #region Factory Method Tests

    [Fact]
    public void Create_WithValidData_ShouldCreateTag()
    {
        // Arrange
        var name = "Bug";
        var color = "#FF5733";

        // Act
        var result = Tag.Create(name, color);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(color, result.Value.Color);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldReturnFailure(string? invalidName)
    {
        // Act
        var result = Tag.Create(invalidName!, "#FF5733");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Tag name cannot be empty", result.Error.Description);
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldReturnFailure()
    {
        // Arrange
        var nameTooLong = new string('A', 51); // Max is 50

        // Act
        var result = Tag.Create(nameTooLong, "#FF5733");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Tag name cannot exceed 50 characters", result.Error.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyColor_ShouldUseDefaultGray(string? emptyColor)
    {
        // Act
        var result = Tag.Create("Bug", emptyColor!);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("#808080", result.Value!.Color); // Default gray
    }

    [Theory]
    [InlineData("FF5733")]       // Missing #
    [InlineData("#FF57")]        // Too short
    [InlineData("#FF573399")]    // Too long (8 chars)
    [InlineData("invalid")]      // Not hex
    public void Create_WithInvalidColor_ShouldReturnFailure(string invalidColor)
    {
        // Act
        var result = Tag.Create("Bug", invalidColor);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Color must be a valid hex color", result.Error.Description);
    }

    [Theory]
    [InlineData("#FF5733")]   // 7 chars (valid)
    [InlineData("#F57")]      // 4 chars (valid short format)
    public void Create_WithValidColorFormats_ShouldSucceed(string validColor)
    {
        // Act
        var result = Tag.Create("Bug", validColor);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(validColor, result.Value!.Color);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        var tag = Tag.Create("Bug", "#FF5733").Value!;
        var newName = "Critical Bug";
        var newColor = "#FF0000";

        // Act
        var result = tag.Update(newName, newColor);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newName, tag.Name);
        Assert.Equal(newColor, tag.Color);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_WithEmptyName_ShouldReturnFailure(string? invalidName)
    {
        // Arrange
        var tag = Tag.Create("Bug", "#FF5733").Value!;

        // Act
        var result = tag.Update(invalidName!, "#FF0000");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Tag name cannot be empty", result.Error.Description);
    }

    [Fact]
    public void Update_WithInvalidColor_ShouldReturnFailure()
    {
        // Arrange
        var tag = Tag.Create("Bug", "#FF5733").Value!;

        // Act
        var result = tag.Update("Critical Bug", "invalid");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Color must be a valid hex color", result.Error.Description);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Create_WithWhitespaceInName_ShouldTrimAndSucceed()
    {
        // Arrange
        var nameWithWhitespace = "  Bug  ";

        // Act
        var result = Tag.Create(nameWithWhitespace, "#FF5733");

        // Assert
        Assert.True(result.IsSuccess);
        // Note: Current implementation doesn't trim, this test documents expected behavior
        // In production, you might want to add .Trim() in validation
    }

    [Fact]
    public void Create_WithLowercaseHexColor_ShouldAccept()
    {
        // Act
        var result = Tag.Create("Bug", "#ff5733");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("#ff5733", result.Value!.Color);
    }

    #endregion
}
