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
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<CreateTicketCommandHandler>>();

        _handler = new CreateTicketCommandHandler(
            _ticketRepositoryMock.Object,
            _contextMock.Object,
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
        
        // ✅ FIXED: Setup Categories DbSet to return a valid category
        var categories = new List<Domain.Entities.Category>
        {
            Domain.Entities.Category.Create("Test Category", "A test category").Value!
        }.AsQueryable();
        
        var dbSetMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Domain.Entities.Category>>();
        dbSetMock.As<IQueryable<Domain.Entities.Category>>().Setup(m => m.Provider).Returns(categories.Provider);
        dbSetMock.As<IQueryable<Domain.Entities.Category>>().Setup(m => m.Expression).Returns(categories.Expression);
        dbSetMock.As<IQueryable<Domain.Entities.Category>>().Setup(m => m.ElementType).Returns(categories.ElementType);
        dbSetMock.As<IQueryable<Domain.Entities.Category>>().Setup(m => m.GetEnumerator()).Returns(categories.GetEnumerator());
        
        dbSetMock.Setup(x => x.FindAsync(
            It.Is<object[]>(o => o.Length == 1 && (int)o[0] == 1),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Domain.Entities.Category.Create("Test Category", "A test category").Value!);
        
        _contextMock.Setup(x => x.Categories).Returns(dbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Message.Should().Be("Ticket created successfully");
        result.Value.Priority.Should().Be(TicketPriority.Medium);
        result.Value.Status.Should().Be(TicketStatus.Open);

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
            CategoryId = 1  // Valid ID so category check passes, then title validation fails
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(1);
        
        // ✅ FIXED: Setup Categories mock to return valid category so title validation is tested
        var categoryMock = Domain.Entities.Category.Create("Test Category", "A test category").Value!;
        var dbSetMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Domain.Entities.Category>>();
        dbSetMock.Setup(x => x.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(categoryMock);
        
        _contextMock.Setup(x => x.Categories).Returns(dbSetMock.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().ContainEquivalentOf("title");

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
        
        // ✅ FIXED: Setup Categories mock
        var dbSetMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Domain.Entities.Category>>();
        dbSetMock.Setup(x => x.FindAsync(
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync((Domain.Entities.Category)null!);
        
        _contextMock.Setup(x => x.Categories).Returns(dbSetMock.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().ContainEquivalentOf("category");

        _ticketRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Ticket>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
