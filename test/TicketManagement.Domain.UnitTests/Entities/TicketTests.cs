using FluentAssertions;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Events;
using Xunit;

namespace TicketManagement.Domain.UnitTests.Entities;

/// <summary>
/// ✅ SENIOR LEVEL: Comprehensive unit tests for Ticket domain entity
/// Tests business rules, edge cases, and domain invariants
/// </summary>
public class TicketTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var title = "Test Ticket";
        var description = "Test Description";
        var priority = TicketPriority.Medium;
        var categoryId = 1;
        var creatorId = 1;

        // Act
        var result = Ticket.Create(title, description, priority, categoryId, creatorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Value.Should().Be(title);
        result.Value.Description.Value.Should().Be(description);
        result.Value.Priority.Should().Be(priority);
        result.Value.CategoryId.Should().Be(categoryId);
        result.Value.CreatorId.Should().Be(creatorId);
        result.Value.Status.Should().Be(TicketStatus.Open);
        result.Value.AssignedToId.Should().BeNull();
    }

    [Theory]
    [InlineData("", "Description", TicketPriority.Low, 1, 1)]
    [InlineData("Title", "", TicketPriority.Low, 1, 1)]
    [InlineData("Title", "Description", TicketPriority.Low, 0, 1)]
    [InlineData("Title", "Description", TicketPriority.Low, 1, 0)]
    public void Create_WithInvalidData_ShouldFail(string title, string description, TicketPriority priority, int categoryId, int creatorId)
    {
        // Act
        var result = Ticket.Create(title, description, priority, categoryId, creatorId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ShouldEmitTicketCreatedEvent()
    {
        // Arrange & Act
        var result = Ticket.Create("Title", "Description", TicketPriority.High, 1, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.DomainEvents.Should().HaveCount(1);
        result.Value.DomainEvents.First().Should().BeOfType<TicketCreatedEvent>();
    }

    [Fact]
    public void Assign_WithValidAgent_ShouldSucceed()
    {
        // Arrange
        var ticket = CreateValidTicket();
        var agentId = 2;

        // Act
        var result = ticket.Assign(agentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ticket.AssignedToId.Should().Be(agentId);
        ticket.Status.Should().Be(TicketStatus.InProgress);
        ticket.DomainEvents.Should().Contain(e => e is TicketAssignedEvent);
    }

    [Fact]
    public void Assign_ToClosedTicket_ShouldFail()
    {
        // Arrange
        var ticket = CreateValidTicket();
        ticket.Close();
        ticket.ClearDomainEvents(); // Clear previous events

        // Act
        var result = ticket.Assign(2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("closed ticket");
        ticket.AssignedToId.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Assign_WithInvalidAgentId_ShouldFail(int agentId)
    {
        // Arrange
        var ticket = CreateValidTicket();

        // Act
        var result = ticket.Assign(agentId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Invalid agent ID");
    }

    [Fact]
    public void Close_OpenTicket_ShouldSucceed()
    {
        // Arrange
        var ticket = CreateValidTicket();

        // Act
        var result = ticket.Close();

        // Assert
        result.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(TicketStatus.Closed);
        ticket.DomainEvents.Should().Contain(e => e is TicketClosedEvent);
    }

    [Fact]
    public void Close_AlreadyClosedTicket_ShouldFail()
    {
        // Arrange
        var ticket = CreateValidTicket();
        ticket.Close();
        ticket.ClearDomainEvents();

        // Act
        var result = ticket.Close();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already closed");
    }

    [Fact]
    public void Reopen_ClosedTicket_ShouldSucceed()
    {
        // Arrange
        var ticket = CreateValidTicket();
        ticket.Close();

        // Act
        var result = ticket.Reopen();

        // Assert
        result.IsSuccess.Should().BeTrue();
        ticket.Status.Should().Be(TicketStatus.Reopened);
    }

    [Fact]
    public void Reopen_OpenTicket_ShouldFail()
    {
        // Arrange
        var ticket = CreateValidTicket();

        // Act
        var result = ticket.Reopen();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Only closed tickets can be reopened");
    }

    [Fact]
    public void Update_ValidData_ShouldSucceed()
    {
        // Arrange
        var ticket = CreateValidTicket();
        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var newPriority = TicketPriority.Critical;

        // Act
        var result = ticket.Update(newTitle, newDescription, newPriority);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ticket.Title.Value.Should().Be(newTitle);
        ticket.Description.Value.Should().Be(newDescription);
        ticket.Priority.Should().Be(newPriority);
        ticket.DomainEvents.Should().Contain(e => e is TicketUpdatedEvent);
    }

    [Fact]
    public void Update_ClosedTicket_ShouldFail()
    {
        // Arrange
        var ticket = CreateValidTicket();
        ticket.Close();

        // Act
        var result = ticket.Update("New Title", "New Description", TicketPriority.High);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Cannot update a closed ticket");
    }

    [Fact]
    public void AddComment_ValidData_ShouldSucceed()
    {
        // Arrange
        var ticket = CreateValidTicket();
        var content = "This is a test comment";
        var authorId = 1;

        // Act
        var result = ticket.AddComment(content, authorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ticket.Comments.Should().HaveCount(1);
        ticket.Comments.First().Content.Should().Be(content);
        ticket.Comments.First().AuthorId.Should().Be(authorId);
        ticket.DomainEvents.Should().Contain(e => e is TicketCommentAddedEvent);
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData("Valid content", 0)]
    [InlineData("Valid content", -1)]
    public void AddComment_InvalidData_ShouldFail(string content, int authorId)
    {
        // Arrange
        var ticket = CreateValidTicket();

        // Act
        var result = ticket.AddComment(content, authorId);

        // Assert
        result.IsFailure.Should().BeTrue();
        ticket.Comments.Should().BeEmpty();
    }

    [Fact]
    public void AddComment_TooLongContent_ShouldFail()
    {
        // Arrange
        var ticket = CreateValidTicket();
        var longContent = new string('a', 2001); // Exceeds 2000 character limit

        // Act
        var result = ticket.AddComment(longContent, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("cannot exceed 2000 characters");
    }

    [Fact]
    public void CanBeAssignedTo_ValidAgent_ShouldReturnTrue()
    {
        // Arrange
        var ticket = CreateValidTicket();

        // Act & Assert
        ticket.CanBeAssignedTo(1).Should().BeTrue();
    }

    [Fact]
    public void CanBeAssignedTo_ClosedTicket_ShouldReturnFalse()
    {
        // Arrange
        var ticket = CreateValidTicket();
        ticket.Close();

        // Act & Assert
        ticket.CanBeAssignedTo(1).Should().BeFalse();
    }

    [Fact]
    public void CanBeUpdatedBy_Creator_ShouldReturnTrue()
    {
        // Arrange
        var ticket = CreateValidTicket();

        // Act & Assert
        ticket.CanBeUpdatedBy(ticket.CreatorId).Should().BeTrue();
    }

    [Fact]
    public void CanBeUpdatedBy_AssignedAgent_ShouldReturnTrue()
    {
        // Arrange
        var ticket = CreateValidTicket();
        ticket.Assign(2);

        // Act & Assert
        ticket.CanBeUpdatedBy(2).Should().BeTrue();
    }

    [Fact]
    public void CanBeUpdatedBy_OtherUser_ShouldReturnFalse()
    {
        // Arrange
        var ticket = CreateValidTicket();

        // Act & Assert
        ticket.CanBeUpdatedBy(999).Should().BeFalse();
    }



    private static Ticket CreateValidTicket()
    {
        var result = Ticket.Create("Test Ticket", "Test Description", TicketPriority.Medium, 1, 1);
        result.IsSuccess.Should().BeTrue();
        result.Value!.ClearDomainEvents(); // Clear creation event for cleaner tests
        return result.Value;
    }
}