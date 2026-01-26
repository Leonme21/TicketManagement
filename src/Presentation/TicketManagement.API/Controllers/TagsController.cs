using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Application.Tags.Commands.AddTagToTicket;
using TicketManagement.Application.Tags.Commands.CreateTag;
// using TicketManagement.Application.Tags.Queries.GetAllTags; // (Futuro: Query para listar)

namespace TicketManagement.WebApi.Controllers;

[Authorize] // Solo usuarios registrados
public class TagsController : ApiControllerBase
{
    /// <summary>
    /// Crea una nueva etiqueta
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTagCommand command)
    {
        var result = await Mediator.Send(command);
        // En una API estricta REST, deberíamos devolver la URL del recurso creado.
        // Por simplificación devolvemos el objeto.
        return Ok(result);
    }

    /// <summary>
    /// Asigna una etiqueta existente a un ticket
    /// </summary>
    [HttpPost("{tagId}/assign-to-ticket/{ticketId}")] // Ejemplo de ruta: api/tags/5/assign-to-ticket/10
    public async Task<IActionResult> AssignToTicket(int tagId, int ticketId)
    {
        // Creamos el comando manualmente con los datos de la URL
        var command = new AddTagToTicketCommand(TicketId: ticketId, TagId: tagId);

        await Mediator.Send(command);

        return NoContent();
    }
}