using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketManagement.Domain.Common;

namespace TicketManagement.WebApi.Controllers;

/// <summary>
/// Base controller with result handling
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Gets the current user ID from the JWT token
    /// </summary>
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value 
                         ?? User.FindFirst("userId")?.Value;
        
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Gets the current user email from the JWT token
    /// </summary>
    protected string GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value 
               ?? User.FindFirst("email")?.Value 
               ?? string.Empty;
    }

    /// <summary>
    /// Gets the current user role from the JWT token
    /// </summary>
    protected string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value 
               ?? User.FindFirst("role")?.Value 
               ?? string.Empty;
    }

    /// <summary>
    /// Handles Result objects and maps them to appropriate HTTP responses
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value is null ? NoContent() : Ok(result.Value);
        }

        return MapErrorToActionResult(result.Error, result.Status);
    }

    /// <summary>
    /// Handles non-generic Result objects
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return MapErrorToActionResult(result.Error, result.Status);
    }

    /// <summary>
    /// Maps Error and ResultStatus to appropriate HTTP response
    /// </summary>
    private IActionResult MapErrorToActionResult(Error error, ResultStatus status)
    {
        var problemDetails = CreateProblemDetails(
            GetTitleForStatus(status),
            error.Description,
            (int)status,
            error.Code);

        return status switch
        {
            ResultStatus.NotFound => NotFound(problemDetails),
            ResultStatus.Unauthorized => Unauthorized(problemDetails),
            ResultStatus.Forbidden => CreateForbiddenResult(problemDetails),
            ResultStatus.Invalid => BadRequest(problemDetails),
            ResultStatus.Conflict => Conflict(problemDetails),
            ResultStatus.RateLimitExceeded => StatusCode(StatusCodes.Status429TooManyRequests, problemDetails),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetails)
        };
    }

    /// <summary>
    /// Gets a user-friendly title based on the result status
    /// </summary>
    private static string GetTitleForStatus(ResultStatus status) => status switch
    {
        ResultStatus.NotFound => "Resource Not Found",
        ResultStatus.Unauthorized => "Unauthorized",
        ResultStatus.Forbidden => "Access Denied",
        ResultStatus.Invalid => "Validation Failed",
        ResultStatus.Conflict => "Resource Conflict",
        ResultStatus.RateLimitExceeded => "Rate Limit Exceeded",
        _ => "Internal Server Error"
    };

    /// <summary>
    /// Creates structured problem details for consistent error responses
    /// </summary>
    private ProblemDetails CreateProblemDetails(string title, string detail, int statusCode, string errorCode)
    {
        return new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = HttpContext.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}",
            Extensions =
            {
                ["correlationId"] = HttpContext.TraceIdentifier,
                ["errorCode"] = errorCode,
                ["timestamp"] = DateTimeOffset.UtcNow
            }
        };
    }

    /// <summary>
    /// Creates a Forbid result with structured problem details
    /// </summary>
    private IActionResult CreateForbiddenResult(ProblemDetails problemDetails)
    {
        return new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}