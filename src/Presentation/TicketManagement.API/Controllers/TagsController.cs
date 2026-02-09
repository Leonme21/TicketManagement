using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Application.Tags.Commands;
using TicketManagement.Application.Tags.Commands.AddTagToTicket;
using TicketManagement.Domain.Common;

namespace TicketManagement.WebApi.Controllers;

[Authorize]
public class TagsController : ApiControllerBase
{
    private readonly ILogger<TagsController> _logger;

    public TagsController(ILogger<TagsController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTagCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(Create), new { id = result.Value }, new { id = result.Value });
        }
        return Problem(detail: result.Error.Description, statusCode: StatusCodes.Status400BadRequest);
    }

    [HttpPost("{tagId}/assign-to-ticket/{ticketId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignToTicket(int tagId, int ticketId)
    {
        var command = new AddTagToTicketCommand(TicketId: ticketId, TagId: tagId);
        var result = await Mediator.Send(command);

        if (result.IsSuccess) return NoContent();

        return result.Error switch
        {
            var error when error.Description.Contains("not found") => NotFound(new { error = error.Description }),
            var error when error.Description.Contains("exists") || error.Description.Contains("duplicate") => 
                Conflict(new { error = error.Description }),
            _ => BadRequest(new { error = result.Error.Description })
        };
    }
}
