using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the Ticket entity business logic
/// </summary>
public class TicketTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidData_ShouldCreateTicketWithOpenStatus()
    {
        // Arrange
        var title = "Test Ticket";
        var description = "Test Description";
        var priority = TicketPriority.Medium;
        var categoryId = 1;
        var creatorId = 1;

        // Act
        var ticket = new Ticket(title, description, priority, categoryId, creatorId);

        // Assert
        Assert.Equal(title, ticket.Title);
        Assert.Equal(description, ticket.Description);
        Assert.Equal(priority, ticket.Priority);
        Assert.Equal(categoryId, ticket.CategoryId);
        Assert.Equal(creatorId, ticket.CreatorId);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Null(ticket.AssignedToId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyTitle_ShouldThrowDomainException(string? invalidTitle)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            new Ticket(invalidTitle!, "Description", TicketPriority.Low, 1, 1));

        Assert.Equal("Ticket title cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyDescription_ShouldThrowDomainException(string? invalidDescription)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            new Ticket("Title", invalidDescription!, TicketPriority.Low, 1, 1));

        Assert.Equal("Ticket description cannot be empty", exception.Message);
    }

    #endregion

    #region Assign Tests

    [Fact]
    public void Assign_WhenOpen_ShouldSetAgentAndChangeStatusToInProgress()
    {
        // Arrange
        var ticket = CreateValidTicket();
        var agentId = 2;

        // Act
        ticket.Assign(agentId);

        // Assert
        Assert.Equal(agentId, ticket.AssignedToId);
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
    }

    [Fact]
    public void Assign_WhenClosed_ShouldThrowDomainException()
    {
        // Arrange
        var ticket = CreateValidTicket();
        ticket.Close();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => ticket.Assign(2));
        Assert.Contains("Cannot assign a closed ticket", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Assign_WithInvalidAgentId_ShouldThrowDomainException(int invalidAgentId)
    {
        // Arrange
        var ticket = CreateValidTicket();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => ticket.Assign(invalidAgentId));
        Assert.Equal("Invalid agent ID", exception.Message);
    }

    #endregion

    #region Close Tests

    [Fact]
    public void Close_WhenOpen_ShouldChangeStatusToClosed()
    {
        // Arrange
        var ticket = CreateValidTicket();

        // Act
        ticket.Close();

        // Assert
        Assert.Equal(TicketStatus.Closed, ticket.Status);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldThrowDomainException()
    {
        // Arrange
        var ticket = CreateValidTicket();
        ticket.Close();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => ticket.Close());
        Assert.Equal("Ticket is already closed", exception.Message);
    }

    #endregion

    #region Reopen Tests

    [Fact]
    public void Reopen_WhenClosed_ShouldChangeStatusToReopened()
    {
        // Arrange
        var ticket = CreateValidTicket();
        ticket.Close();

        // Act
        ticket.Reopen();

        // Assert
        Assert.Equal(TicketStatus.Reopened, ticket.Status);
    }

    [Fact]
    public void Reopen_WhenNotClosed_ShouldThrowDomainException()
    {
        // Arrange
        var ticket = CreateValidTicket();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => ticket.Reopen());
        Assert.Equal("Only closed tickets can be reopened", exception.Message);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateFields()
    {
        // Arrange
        var ticket = CreateValidTicket();
        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var newPriority = TicketPriority.Critical;

        // Act
        ticket.Update(newTitle, newDescription, newPriority);

        // Assert
        Assert.Equal(newTitle, ticket.Title);
        Assert.Equal(newDescription, ticket.Description);
        Assert.Equal(newPriority, ticket.Priority);
    }

    [Fact]
    public void Update_WhenClosed_ShouldThrowDomainException()
    {
        // Arrange
        var ticket = CreateValidTicket();
        ticket.Close();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            ticket.Update("New Title", "New Description", TicketPriority.High));

        Assert.Equal("Cannot update a closed ticket", exception.Message);
    }

    #endregion

    #region Resolve Tests

    [Fact]
    public void Resolve_WhenAssigned_ShouldChangeStatusToResolved()
    {
        // Arrange
        var ticket = CreateValidTicket();
        ticket.Assign(2);

        // Act
        ticket.Resolve();

        // Assert
        Assert.Equal(TicketStatus.Resolved, ticket.Status);
    }

    [Fact]
    public void Resolve_WhenNotAssigned_ShouldThrowDomainException()
    {
        // Arrange
        var ticket = CreateValidTicket();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => ticket.Resolve());
        Assert.Equal("Only assigned tickets can be resolved", exception.Message);
    }

    #endregion

    #region Helper Methods

    private static Ticket CreateValidTicket()
    {
        return new Ticket("Test Ticket", "Test Description", TicketPriority.Medium, 1, 1);
    }

    #endregion
}
