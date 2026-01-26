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
using TicketManagement.Domain.Enums;

namespace TicketManagement.WebApi.Controllers;

/// <summary>
/// Endpoints para gestión de tickets
/// </summary>
[Authorize]
public class TicketsController : ApiControllerBase
{
    /// <summary>
    /// Obtiene tickets con paginación y filtros
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTickets([FromQuery] GetTicketsWithPaginationQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un ticket por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicketById(int id)
    {
        var query = new GetTicketByIdQuery { TicketId = id };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene tickets del usuario autenticado
    /// </summary>
    [HttpGet("my-tickets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTickets([FromQuery] GetTicketsByUserQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene tickets asignados al agente autenticado
    /// </summary>
    [HttpGet("assigned-to-me")]
    [Authorize(Roles = "Agent,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignedTickets([FromQuery] GetTicketsByAgentQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo ticket
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketCommand command)
    {
        var ticketId = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetTicketById), new { id = ticketId }, new { id = ticketId });
    }

    /// <summary>
    /// Actualiza un ticket existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTicketCommand command)
    {
        // Ensure route ID takes precedence over body ID to prevent tampering
        var commandWithRouteId = command with { TicketId = id };
        await Mediator.Send(commandWithRouteId);
        return NoContent();
    }

    /// <summary>
    /// Asigna un ticket a un agente
    /// </summary>
    [HttpPost("{id}/assign")]
    [Authorize(Roles = "Agent,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignTicketCommand command)
    {
        // Ensure route ID takes precedence over body ID to prevent tampering
        var commandWithRouteId = command with { TicketId = id };
        await Mediator.Send(commandWithRouteId);
        return NoContent();
    }

    /// <summary>
    /// Cierra un ticket
    /// </summary>
    [HttpPost("{id}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseTicket(int id)
    {
        var command = new CloseTicketCommand { TicketId = id };
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Elimina un ticket (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var command = new DeleteTicketCommand { TicketId = id };
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Agrega un comentario a un ticket
    /// </summary>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentCommand command)
    {
        // Ensure route ID takes precedence over body ID to prevent tampering
        var commandWithRouteId = command with { TicketId = id };
        var result = await Mediator.Send(commandWithRouteId);
        return CreatedAtAction(nameof(GetTicketById), new { id }, result);
    }
}