using FluentAssertions;
using TicketManagement.Domain.ValueObjects;
using Xunit;

namespace TicketManagement.Domain.UnitTests.ValueObjects;

/// <summary>
/// ✅ SENIOR LEVEL: Value object tests ensuring immutability and validation
/// </summary>
public class TicketTitleTests
{
    [Fact]
    public void Create_WithValidTitle_ShouldSucceed()
    {
        // Arrange
        var title = "Valid Ticket Title";

        // Act
        var result = TicketTitle.Create(title);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Value.Should().Be(title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyTitle_ShouldFail(string? title)
    {
        // Act
        var result = TicketTitle.Create(title!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("cannot be empty");
    }

    [Fact]
    public void Create_WithTooLongTitle_ShouldFail()
    {
        // Arrange
        var longTitle = new string('a', 201); // Exceeds 200 character limit

        // Act
        var result = TicketTitle.Create(longTitle);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("must not exceed 200 characters");
    }

    [Fact]
    public void Create_WithMaxLengthTitle_ShouldSucceed()
    {
        // Arrange
        var maxTitle = new string('a', 200); // Exactly 200 characters

        // Act
        var result = TicketTitle.Create(maxTitle);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().Be(maxTitle);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var title1 = TicketTitle.Create("Same Title").Value!;
        var title2 = TicketTitle.Create("Same Title").Value!;

        // Act & Assert
        title1.Should().Be(title2);
        title1.Equals(title2).Should().BeTrue();
        (title1 == title2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var title1 = TicketTitle.Create("Title 1").Value!;
        var title2 = TicketTitle.Create("Title 2").Value!;

        // Act & Assert
        title1.Should().NotBe(title2);
        title1.Equals(title2).Should().BeFalse();
        (title1 == title2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHash()
    {
        // Arrange
        var title1 = TicketTitle.Create("Same Title").Value!;
        var title2 = TicketTitle.Create("Same Title").Value!;

        // Act & Assert
        title1.GetHashCode().Should().Be(title2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var titleValue = "Test Title";
        var title = TicketTitle.Create(titleValue).Value!;

        // Act & Assert
        title.ToString().Should().Be(titleValue);
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var titleValue = "Test Title";
        var title = TicketTitle.Create(titleValue).Value!;

        // Act
        string convertedTitle = title;

        // Assert
        convertedTitle.Should().Be(titleValue);
    }
}