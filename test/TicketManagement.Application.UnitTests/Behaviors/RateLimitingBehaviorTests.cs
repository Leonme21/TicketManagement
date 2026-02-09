using FluentAssertions;
using MediatR;
using Moq;
using TicketManagement.Application.Common.Behaviors;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Exceptions;
using Xunit;

namespace TicketManagement.Application.UnitTests.Behaviors;

/// <summary>
/// 🔥 BIG TECH LEVEL: Comprehensive behavior testing
/// Tests all edge cases and error scenarios
/// </summary>
public class RateLimitingBehaviorTests
{
    private readonly Mock<IRateLimitService> _rateLimitServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly RateLimitingBehavior<TestRateLimitedRequest, Result<string>> _behavior;

    public RateLimitingBehaviorTests()
    {
        _rateLimitServiceMock = new Mock<IRateLimitService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _behavior = new RateLimitingBehavior<TestRateLimitedRequest, Result<string>>(
            _rateLimitServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithNonRateLimitedRequest_ShouldPassThrough()
    {
        // Arrange
        var request = new TestNonRateLimitedRequest();
        var expectedResult = Result<string>.Success("test");
        var next = new Mock<RequestHandlerDelegate<Result<string>>>();
        next.Setup(x => x(It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);
        
        var behavior = new RateLimitingBehavior<TestNonRateLimitedRequest, Result<string>>(
            _rateLimitServiceMock.Object,
            _currentUserServiceMock.Object);

        // Act
        var result = await behavior.Handle(request, next.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        _rateLimitServiceMock.Verify(x => x.CheckLimitAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithRateLimitedRequestAndAllowed_ShouldExecuteAndRecord()
    {
        // Arrange
        var request = new TestRateLimitedRequest();
        var userId = 123;
        var expectedResult = Result<string>.Success("test");
        
        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _rateLimitServiceMock.Setup(x => x.CheckLimitAsync(userId, request.OperationType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RateLimitResult { IsAllowed = true, RemainingRequests = 5 });
        
        var next = new Mock<RequestHandlerDelegate<Result<string>>>();
        next.Setup(x => x(It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(request, next.Object, CancellationToken.None);

        // Assert
        var opType = request.OperationType;
        result.Should().Be(expectedResult);
        _rateLimitServiceMock.Verify(x => x.CheckLimitAsync(userId, opType, It.IsAny<CancellationToken>()), Times.Once);
        _rateLimitServiceMock.Verify(x => x.RecordOperationAsync(userId, opType, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithRateLimitedRequestAndNotAllowed_ShouldThrowException()
    {
        // Arrange
        var request = new TestRateLimitedRequest();
        var userId = 123;
        var retryAfter = TimeSpan.FromMinutes(5);
        
        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _rateLimitServiceMock.Setup(x => x.CheckLimitAsync(userId, request.OperationType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RateLimitResult { IsAllowed = false, RemainingRequests = 0, RetryAfter = retryAfter });
        
        var next = new Mock<RequestHandlerDelegate<Result<string>>>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RateLimitExceededException>(() => 
            _behavior.Handle(request, next.Object, CancellationToken.None));
        
        exception.Operation.Should().Be(request.OperationType);
        exception.RetryAfter.Should().Be(retryAfter);
        
        next.Verify(x => x(It.IsAny<CancellationToken>()), Times.Never);
        _rateLimitServiceMock.Verify(x => x.RecordOperationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithRateLimitedRequestAndNoRetryAfter_ShouldUseDefaultRetryAfter()
    {
        // Arrange
        var request = new TestRateLimitedRequest();
        var userId = 123;
        
        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _rateLimitServiceMock.Setup(x => x.CheckLimitAsync(userId, request.OperationType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RateLimitResult { IsAllowed = false, RemainingRequests = 0, RetryAfter = null });
        
        var next = new Mock<RequestHandlerDelegate<Result<string>>>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RateLimitExceededException>(() => 
            _behavior.Handle(request, next.Object, CancellationToken.None));
        
        exception.RetryAfter.Should().Be(TimeSpan.FromMinutes(1)); // Default value
    }

    // Test request classes
    private record TestRateLimitedRequest : IRequest<Result<string>>, IRateLimitedRequest
    {
        public string OperationType => "test_operation";
    }

    private record TestNonRateLimitedRequest : IRequest<Result<string>>;
}