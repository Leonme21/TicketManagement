using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Tickets.Commands.CreateTicket;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Interfaces;
using Xunit;

namespace TicketManagement.Application.UnitTests.Handlers;

/// <summary>
/// ✅ REFACTORED: Unit tests updated for simplified handler dependencies
/// Uses IApplicationDbContext instead of IUnitOfWork
/// </summary>
public class CreateTicketCommandHandlerTests
{
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<IApplicationDbContext> _contextMock; // ✅ Changed from IUnitOfWork
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<CreateTicketCommandHandler>> _loggerMock;
    private readonly CreateTicketCommandHandler _handler;

    public CreateTicketCommandHandlerTests()
    {
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _contextMock = new Mock<IApplicationDbContext>(); // ✅ Changed from IUnitOfWork
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<CreateTicketCommandHandler>>();

        _handler = new CreateTicketCommandHandler(
            _ticketRepositoryMock.Object,
            _contextMock.Object, // ✅ Changed from _unitOfWorkMock
            _currentUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTicketSuccessfully()
    {
        // Arrange
        var command = new CreateTicketCommand
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = TicketPriority.Medium,
            CategoryId = 1
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(1);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        // ✅ Fixed: In unit tests with mocks, ID won't be generated
        // We verify the behavior, not the database-generated ID
        result.Value!.Message.Should().Be("Ticket created successfully");
        result.Value.Priority.Should().Be("Medium");
        result.Value.Status.Should().Be("Open");

        _ticketRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Ticket>(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _contextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyTitle_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateTicketCommand
        {
            Title = "",
            Description = "Test Description",
            Priority = TicketPriority.Medium,
            CategoryId = 1
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("title");

        _ticketRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Ticket>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidCategoryId_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateTicketCommand
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = TicketPriority.Medium,
            CategoryId = 0
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("category");

        _ticketRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Ticket>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
