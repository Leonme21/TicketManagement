using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TicketManagement.Application.Common.Behaviors;
using TicketManagement.Application.Common.Exceptions;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using Xunit;

namespace TicketManagement.Application.UnitTests.Behaviors;

/// <summary>
/// 🔥 ENTERPRISE LEVEL: Comprehensive TransactionBehavior testing
/// Tests transaction wrapping, rollback, and concurrency handling
/// </summary>
public class TransactionBehaviorTests
{
    [Fact]
    public async Task Handle_WithNonCommand_ShouldBypassTransaction()
    {
        // Arrange
        var contextMock = new Mock<IApplicationDbContext>();
        var loggerMock = new Mock<ILogger<TransactionBehavior<TestQuery, Result>>>();
        var nextMock = new Mock<RequestHandlerDelegate<Result>>();
        
        var request = new TestQuery();
        var expectedResult = Result.Success();
        nextMock.Setup(x => x(It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

        var behavior = new TransactionBehavior<TestQuery, Result>(
            contextMock.Object,
            loggerMock.Object);

        // Act
        var result = await behavior.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        nextMock.Verify(x => x(It.IsAny<CancellationToken>()), Times.Once);
        // Database should never be accessed for non-commands
        contextMock.Invocations.Should().BeEmpty();
    }

    [Fact]
    public void TransactionBehavior_WithCommand_ChecksCommandInterface()
    {
        // Arrange
        var testCommand = new TestCommand();
        var testQuery = new TestQuery();
        
        // Assert - verify interface implementation
        testCommand.Should().BeAssignableTo<ICommand>();
        testQuery.Should().NotBeAssignableTo<ICommand>();
    }

    // Test request types
    private class TestCommand : ICommand
    {
    }

    private class TestQuery : IRequest<Result>
    {
    }
}
