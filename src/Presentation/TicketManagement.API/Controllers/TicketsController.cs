using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Application.Tickets.Commands.AddComment;
using TicketManagement.Application.Tickets.Commands.AssignTicket;
using TicketManagement.Application.Tickets.Commands.CloseTicket;
using TicketManagement.Application.Tickets.Commands.CreateTicket;
using TicketManagement.Application.Tickets.Commands.DeleteTicket;
using TicketManagement.Application.Tickets.Commands.UpdateTicket;
using TicketManagement.Application.Tickets.Queries.GetTicketById;
using TicketManagement.Application.Tickets.Queries.GetTicketsByAgent;
using TicketManagement.Application.Tickets.Queries.GetTicketsByUser;
using TicketManagement.Application.Tickets.Queries.GetTicketsWithPagination;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Domain.Common;
using TicketManagement.Application.Common.Authorization;

namespace TicketManagement.WebApi.Controllers;

/// <summary>
/// Tickets API Controller
/// ✅ REFACTORED: Uses policy constants instead of magic strings
/// Controllers are pure HTTP adapters
/// </summary>
[Authorize]
public class TicketsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<TicketSummaryDto>), StatusCodes.Status200OK)]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<IActionResult> GetTickets(
        [FromQuery] GetTicketsWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TicketDetailsDto), StatusCodes.Status200OK)]
    [ResponseCache(Duration = 1800, Location = ResponseCacheLocation.Any, NoStore = false)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTicketById(int id, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetTicketByIdQuery { TicketId = id }, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateTicketResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTicket(
        [FromBody] CreateTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        if (result.IsSuccess)
        {
            // Cache invalidation handled by TicketCacheInvalidationHandler via TicketCreatedEvent
            return CreatedAtAction(nameof(GetTicketById), 
                new { id = result.Value!.TicketId }, result.Value);
        }
        
        return HandleResult(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateTicket(
        int id, 
        [FromBody] UpdateTicketApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateTicketCommand
        {
            TicketId = id,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            CategoryId = request.CategoryId,
            RowVersion = request.RowVersion
        };

        var result = await Mediator.Send(command, cancellationToken);
        
        // Cache invalidation handled by TicketCacheInvalidationHandler via TicketUpdatedEvent
        return HandleResult(result);
    }

    [HttpPost("{id:int}/assign")]
    [Authorize(Policy = Policies.CanAssignTickets)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTicket(
        int id, 
        [FromBody] AssignTicketApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AssignTicketCommand 
        { 
            TicketId = id,
            AgentId = request.AgentId
        };

        var result = await Mediator.Send(command, cancellationToken);
        
        // Cache invalidation handled by TicketCacheInvalidationHandler via TicketAssignedEvent
        return HandleResult(result);
    }

    [HttpPost("{id:int}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseTicket(
        int id, 
        [FromBody] CloseTicketApiRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var command = new CloseTicketCommand 
        { 
            TicketId = id,
            Reason = request?.Reason,
            Resolution = request?.Resolution
        };

        var result = await Mediator.Send(command, cancellationToken);
        
        // Cache invalidation handled by TicketCacheInvalidationHandler via TicketClosedEvent
        return HandleResult(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.CanDeleteTickets)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTicket(int id, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new DeleteTicketCommand { TicketId = id }, cancellationToken);
        
        // Cache invalidation handled by event handlers
        return HandleResult(result);
    }

    [HttpPost("{id:int}/comments")]
    [ProducesResponseType(typeof(CommentCreatedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AddComment(
        int id, 
        [FromBody] AddCommentApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AddCommentCommand 
        { 
            TicketId = id,
            Content = request.Content,
            IsInternal = request.IsInternal ?? false
        };

        var result = await Mediator.Send(command, cancellationToken);
        
        if (result.IsSuccess)
        {
            // Cache invalidation handled by TicketCacheInvalidationHandler via TicketCommentAddedEvent
            return CreatedAtAction(nameof(GetTicketById),
                new { id },
                result.Value);
        }

        return HandleResult(result);
    }

    [HttpGet("my-tickets")]
    [ProducesResponseType(typeof(PaginatedResult<TicketSummaryDto>), StatusCodes.Status200OK)]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<IActionResult> GetMyTickets(
        [FromQuery] GetTicketsByUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("assigned-to-me")]
    [Authorize(Policy = Policies.IsAgentOrAdmin)]
    [ProducesResponseType(typeof(PaginatedResult<TicketSummaryDto>), StatusCodes.Status200OK)]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<IActionResult> GetAssignedTickets(
        [FromQuery] GetTicketsByAgentQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    // ❌ REMOVED: Manual cache warmup endpoint
    // Cache warmup is now handled automatically by CacheWarmupBackgroundService on startup
    // If manual warmup is needed, create a dedicated Admin API or use a management tool
}
