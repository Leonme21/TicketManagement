using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.WebApi.Controllers;

/// <summary>
/// Controller base con funcionalidad común
/// Todos los controllers heredan de esta clase
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    /// <summary>
    /// Instancia de MediatR (lazy loading)
    /// </summary>
    protected ISender Mediator
    {
        get
        {
            if (_mediator == null)
            {
                _mediator = HttpContext.RequestServices.GetRequiredService<ISender>();
            }
            return _mediator;
        }
    }
}