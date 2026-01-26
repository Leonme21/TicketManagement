using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Application.Categories.Queries.GetAllCategories;

namespace TicketManagement.WebApi.Controllers;

/// <summary>
/// Endpoints para categorías
/// </summary>
[AllowAnonymous]
public class CategoriesController : ApiControllerBase
{
    /// <summary>
    /// Obtiene todas las categorías activas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var query = new GetAllCategoriesQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }
}
